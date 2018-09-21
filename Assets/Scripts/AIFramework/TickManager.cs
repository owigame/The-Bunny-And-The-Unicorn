using System.Collections;
using System.Linq;
using Logging;
using UnityEngine;

public class TickManager : MonoBehaviour {
    #region  Singleton management
    public static TickManager _instance;
    private void Awake () {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy (this);
        }

        P1 = new IResponse[0];
        P2 = new IResponse[0];
    }
    private void OnDestroy () {
        _instance = null;
    }
    #endregion

    public TickState tickState;
    private IResponse[] P1, P2;
    int ResponsesRecieved = 0;
    public void OnResponse (IResponse[] ResponseChain) {
        switch (tickState) {
            case TickState.AwaitingResponses:
                ResponsesRecieved++;
                // LogStack.Log ("ResponseChain Length: " + ResponseChain.Length, LogLevel.Stack);
                if (ResponseChain.Length > 0) {
                    // LogStack.Log ("Awaiting Responses, " + ResponsesRecieved + " Players ready", LogLevel.Stack);
                    if (ResponseChain[0].player == TournamentManager._instance.P1) {
                        P1 = ResponseChain;
                    } else {
                        P2 = ResponseChain;
                    }
                }
                if (ResponsesRecieved == 2) {
                    ResponsesRecieved = 0;
                    tickState = TickState.PerformingResponses;
                    OnResponse (null);
                }
                break;
            case TickState.ValidateResponses:
                // LogStack.Log ("Validate Responses | TODO", LogLevel.Stack);
                //AIResponseManager.Move/Attack/Spawn validate | use code

                //for loop of the response chain double checks everthing is good
                // if all good go to TickState.PerformingResponses
                // if a problem send fail event and go back to TickState.AwaitingResponses:
                tickState = TickState.PerformingResponses;
                break;
            case TickState.PerformingResponses:
                LogStack.Log ("Performing Responses", LogLevel.Stack);
                if (P1 != null && P2 != null) {
                    StartCoroutine (PerformActions (P1.Concat (P2).ToArray ()));
                } else if (P1 != null) {
                    StartCoroutine (PerformActions (P1));
                } else {
                    StartCoroutine (PerformActions (P2));
                }
                // stagger out perform phase based on wait times from tournament manager
                // step to TickState.FireTick:
                break;
            case TickState.FireTick:
                // LogStack.Log ("Fire Tick", LogLevel.Stack);
                // restart the loop
                // eject states for win lose conditions

                // ResponsesRecieved++;
                // if (ResponsesRecieved == 2) {
                // ResponsesRecieved = 0;
                tickState = TickState.AwaitingResponses;
                TournamentManager._instance.OnTick.Invoke (FindObjectOfType<LaneManager> ());
                P1 = new IResponse[0];
                P2 = new IResponse[0];
                // }
                break;
            default:
                break;
        }
    }

    IEnumerator PerformActions (IResponse[] ResponseChain) {
        LogStack.Log ("--- PerformActions BEGIN", LogLevel.System);
        //Spawning
        foreach (IResponse response in ResponseChain) {
            if (response.responseActionType == ResponseActionType.Spawn) {
                LogStack.Log ("Response: " + response.player + " | " + response.responseActionType + " in lane " + response.Lane, LogLevel.System);
                Spawn (response);
            }
        }
        yield return new WaitForSeconds (TournamentManager._instance.laneReadyWait);

        //Move
        foreach (IResponse response in ResponseChain) {
            if (response.creature.CreatureType == Spawnable.Bunny && response.responseActionType == ResponseActionType.Move) {
                LogStack.Log ("Response: " + response.player + " | " + response.responseActionType + " in lane " + response.Lane, LogLevel.System);
                response.creature.Move (response.laneNode);
            } else if (response.creature.CreatureType == Spawnable.Unicorn && response.responseActionType == ResponseActionType.Attack) {
                yield return new WaitForSeconds (TournamentManager._instance.moveWait);
                LogStack.Log ("Response: " + response.player + " | " + response.responseActionType + " in lane " + response.Lane, LogLevel.System);
                response.creature.Attack ();
                LaneNode nextNode = response.laneNode.laneManager.GetNextLaneNode (response.laneNode, response.creature.RightFacing, 1, true);
                if ((nextNode == response.laneNode.laneManager.startNode && !response.creature.RightFacing) || (nextNode == response.laneNode.laneManager.endNode && response.creature.RightFacing)) {
                    response.creature.Move(nextNode);
                }
            }
        }
        yield return new WaitForSeconds (TournamentManager._instance.moveWait);

        //Attack
        foreach (IResponse response in ResponseChain) {
            if (response.creature.CreatureType == Spawnable.Bunny && response.responseActionType == ResponseActionType.Attack) {
                LogStack.Log ("Response: " + response.player + " | " + response.responseActionType + " in lane " + response.Lane, LogLevel.System);
                response.creature.Attack ();
            } else if (response.creature.CreatureType == Spawnable.Unicorn && response.responseActionType == ResponseActionType.Move) {
                yield return new WaitForSeconds (TournamentManager._instance.attackWait);
                LogStack.Log ("Response: " + response.player + " | " + response.responseActionType + " in lane " + response.Lane, LogLevel.System);
                response.creature.Move (response.laneNode);
            }
        }
        yield return new WaitForSeconds (TournamentManager._instance.attackWait);

        tickState = TickState.FireTick;
        OnResponse (null);
        LogStack.Log ("--- PerformActions END", LogLevel.System);
    }

    public void Spawn (IResponse response) {
        // ResponsesRecieved++;

        // LogStack.Log ("Player" + ((response.player == TournamentManager._instance.P1) ? " 1 " : " 2 ") + "spawned " + response.creature + " in lane " + response.Lane, LogLevel.Stack);

        //Spawn Creature
        GameObject _spawnable = null;
        if (response.creature.CreatureType == Spawnable.Bunny) {
            _spawnable = Instantiate (TournamentManager._instance.bunnyPrefab);
        } else if (response.creature.CreatureType == Spawnable.Unicorn) {
            _spawnable = Instantiate (TournamentManager._instance.unicornPrefab);
        }
        CreatureBase _creature = _spawnable.GetComponent<CreatureBase> ();
        TournamentManager._instance.OnTick.AddListener (_creature.onTick);

        //Spawn HealthBar
        GameObject _healthBar = Instantiate (TournamentManager._instance.uiHealthBar, TournamentManager._instance.mainCanvas);
        _healthBar.GetComponent<UIFollow> ().Init (_spawnable.transform, TournamentManager._instance.mainCanvas.GetComponent<RectTransform> ());

        //Creature Setup
        _creature.Init (
            response.player,
            TournamentManager._instance.lanes[response.Lane - 1],
            response.creature.CreatureType,
            _healthBar.GetComponent<UIHealth> (),
            response.creature.CreatureType == Spawnable.Bunny ? TournamentManager._instance.bunnyDamage : TournamentManager._instance.unicornDamage
        );

        TournamentManager._instance.lanes[response.Lane - 1]?.AssignToLane (_creature, response.player == TournamentManager._instance.P1 ? true : false);

    }

}
public enum TickState {
    AwaitingResponses,
    ValidateResponses,
    PerformingResponses,
    FireTick
}