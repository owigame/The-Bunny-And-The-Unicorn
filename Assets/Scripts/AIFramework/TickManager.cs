using UnityEngine;
using Logging;
using System.Collections;

public class TickManager : MonoBehaviour
{
    #region  Singleton management
    public static TickManager _instance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void OnDestroy()
    {
        _instance = null;
    }
    #endregion

    public TickState tickState;
    private IResponse[] P1, P2;
    int ResponsesRecieved = 0;
    public void OnResponse(IResponse[] ResponseChain)
    {
        switch (tickState)
        {
            case TickState.AwaitingResponses:
                ResponsesRecieved++;
                if (ResponseChain[0].player == TournamentManager._instance.P1)
                {
                    P1 = ResponseChain;
                }
                else
                {
                    P2 = ResponseChain;
                }
                 if (ResponsesRecieved == 2){ //Max ready players 2
                      ResponsesRecieved = 0;
                    tickState = TickState.PerformingResponses;
                    OnResponse(P1);
                    OnResponse(P2);
                }
                  LogStack.Log("Awaiting Responses, "+ ResponsesRecieved + " Players ready", LogLevel.Stack);
                break;
            case TickState.ValidateResponses:
                LogStack.Log("Validate Responses | TODO", LogLevel.Stack);
                //AIResponseManager.Move/Attack/Spawn validate | use code

                //for loop of the response chain double checks everthing is good
                // if all good go to TickState.PerformingResponses
                // if a problem send fail event and go back to TickState.AwaitingResponses:
                tickState = TickState.PerformingResponses;
                break;
            case TickState.PerformingResponses:
                LogStack.Log("Performing Responses", LogLevel.Stack);
                StartCoroutine(PerformActions(ResponseChain));
                // stagger out perform phase based on wait times from tournament manager
                // step to TickState.FireTick:
                break;
            case TickState.FireTick:
                LogStack.Log("Fire Tick", LogLevel.Stack);
                // restart the loop
                // eject states for win lose conditions
                TournamentManager._instance.OnTick.Invoke(FindObjectOfType<LaneManager>());
                break;
            default:
                break;
        }
    }

    IEnumerator PerformActions(IResponse[] ResponseChain)
    {
        //Spawning
        yield return new WaitForSeconds(TournamentManager._instance.laneReadyWait);
        foreach (IResponse response in ResponseChain)
        {
            if (response.responseActionType == ResponseActionType.Spawn)
            {
                Spawn(response);
            }
        }

        //Move
        yield return new WaitForSeconds(TournamentManager._instance.moveWait);
        foreach (IResponse response in ResponseChain)
        {
            if (response.creature.CreatureType == Spawnable.Bunny && response.responseActionType == ResponseActionType.Move)
            {
                response.creature.Move();
            }
            else if (response.creature.CreatureType == Spawnable.Unicorn && response.responseActionType == ResponseActionType.Attack)
            {
                yield return new WaitForSeconds(TournamentManager._instance.moveWait);
                response.creature.Attack();
            }
        }

        //Attack
        yield return new WaitForSeconds(TournamentManager._instance.attackWait);
        foreach (IResponse response in ResponseChain)
        {
            if (response.creature.CreatureType == Spawnable.Bunny && response.responseActionType == ResponseActionType.Attack)
            {
                response.creature.Attack();
            }
            else if (response.creature.CreatureType == Spawnable.Unicorn && response.responseActionType == ResponseActionType.Move)
            {
                yield return new WaitForSeconds(TournamentManager._instance.attackWait);
                response.creature.Move();
            }
        }

        tickState = TickState.FireTick;
        OnResponse(null);
    }

    public void Spawn(IResponse response)
    {
        LogStack.Log("--- OnResponse", LogLevel.System);
        ResponsesRecieved++;

        
        LogStack.Log("Player" + ((response.player == TournamentManager._instance.P1) ? " 1 " : " 2 ") + "spawned " + response.creature + " in lane " + response.Lane, LogLevel.Stack);

        //Spawn Creature
        GameObject _spawnable = null;
        if (response.creature.CreatureType == Spawnable.Bunny)
        {
            _spawnable = Instantiate(TournamentManager._instance.bunnyPrefab);
        }
        else if (response.creature.CreatureType == Spawnable.Unicorn)
        {
            _spawnable = Instantiate(TournamentManager._instance.unicornPrefab);
        }
        CreatureBase _creature = _spawnable.AddComponent<CreatureBase>();
        TournamentManager._instance.OnTick.AddListener(_creature.onTick);

        //Spawn HealthBar
        GameObject _healthBar = Instantiate(TournamentManager._instance.uiHealthBar, TournamentManager._instance.mainCanvas);
        _healthBar.GetComponent<UIFollow>().Init(_spawnable.transform, TournamentManager._instance.mainCanvas.GetComponent<RectTransform>());

        //Creature Setup
        _creature.Init(
            response.player,
            TournamentManager._instance.lanes[response.Lane - 1],
            response.creature.CreatureType,
            _healthBar.GetComponent<UIHealth>(),
            response.creature.CreatureType == Spawnable.Bunny ? TournamentManager._instance.bunnyDamage : TournamentManager._instance.unicornDamage
        );

        TournamentManager._instance.lanes[response.Lane - 1]?.AssignToLane(_creature, response.player == TournamentManager._instance.P1 ? true : false);

    }

    //    IEnumerator TickTockUpdate()
    //    {

    //        while (true)
    //        {

    //            if (playersReady == 2)
    //            { //Max ready players 2
    //                playersReady = 0;

    //                yield return new WaitForSeconds(offTickInterval);

    //                int lastLanesReady = -1;

    //                //Wait for lanes to execute move, attack per lane
    //                while (lanesReady < lanes.Count)
    //                {
    //                    if (lastLanesReady != lanesReady)
    //                    {
    //                        lastLanesReady = lanesReady;

    //                        if (lanes[lanesReady].creatures.Count > 0)
    //                        {
    //                            lanes[lanesReady].OffTick();
    //                        }
    //                        else
    //                        {
    //                            LaneReady();
    //                        }
    //                    }

    //                    yield return null;
    //                }

    //                lanesReady = 0;
    //                yield return new WaitForSeconds(onTickInterval);

    //                IBoardState ondata = new LaneManager();
    //                OnTick.Invoke(ondata);
    //            }
    //            else
    //            {
    //                yield return null;
    //            }

    //        }
    //    }
}
public enum TickState
{
    AwaitingResponses,
    ValidateResponses,
    PerformingResponses,
    FireTick
}