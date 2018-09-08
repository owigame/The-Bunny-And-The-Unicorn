using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentManager : MonoBehaviour {
    public TickEvent OnTick;
    public static TournamentManager _instance;

    [Header ("Player Setup")]
    public AI.LogicBase P1, P2;

    [Header ("Lanes")]
    public List<LanesNodes> lanes = new List<LanesNodes> ();

    [Header ("Game Settings")]
    public int offTickInterval = 1;
    public int onTickInterval = 2;
    public int bunnyRange = 1;
    public int unicornRange = 3;

    [Header ("Timing")]
    public float moveWait = 1;
    public float attackWait = 1;
    public float dieWait = 1;
    public float laneReadyWait = 1;

    [Header ("Prefabs")]
    public GameObject unicornPrefab;
    public GameObject bunnyPrefab;

    int playersReady = 0;
    int lanesReady = 0;

    //Singleton management
    private void Awake () {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy (this);
        }
    }
    private void OnDestroy () {
        _instance = null;
    }

    // Setup the two Ai players 
    private void Start () {
        OnTick = new TickEvent ();

        //Set Player listeners
        OnTick.AddListener (P1.OnTick);
        OnTick.AddListener (P2.OnTick);

        //Set Lane listeners
        foreach (LanesNodes lane in lanes) {
            lane.OnLaneReady.AddListener (LaneReady);
        }

        P1.init ();
        P2.init ();

        IBoardState data = new LanesNodes ();
        OnTick.Invoke (data);

        StartCoroutine (TickTockUpdate ());
    }

    void LaneReady () {
        lanesReady++;
    }

    IEnumerator TickTockUpdate () {
        while (true) {

            if (playersReady == 2) { //Max ready players 2
                playersReady = 0;

                yield return new WaitForSeconds (offTickInterval);

                int lastLanesReady = -1;

                //Wait for lanes to execute move, attack per lane
                while (lanesReady < lanes.Count) {
                    if (lastLanesReady != lanesReady) {
                        lastLanesReady = lanesReady;

                        if (lanes[lanesReady].creatures.Count > 0){
                            lanes[lanesReady].OffTick ();
                        } else {
                            LaneReady();
                        }
                    }

                    yield return null;
                }

                lanesReady = 0;
                yield return new WaitForSeconds (onTickInterval);

                IBoardState ondata = new LanesNodes ();
                OnTick.Invoke (ondata);
            } else {
                yield return null;
            }

        }
    }

    public void OnResponse (IResponse[] ResponseChain) {
        Debug.Log ("--- OnResponse");
        playersReady++;

        foreach (var item in ResponseChain) {
            Debug.Log ("Player" + ((item.player == P1) ? " 1 " : " 2 ") + "spawned " + item.spawnable + " in lane " + item.Lane);

            GameObject _spawnable = null;
            if (item.spawnable == Spawnable.Bunny) {
                _spawnable = Instantiate (bunnyPrefab);
            } else if (item.spawnable == Spawnable.Unicorn) {
                _spawnable = Instantiate (unicornPrefab);
            }
            CreatureBase _creature = _spawnable.AddComponent<CreatureBase> ();
            OnTick.AddListener (_creature.onTick);
            _creature.Init (item.player, TournamentManager._instance.lanes[item.Lane - 1], item.spawnable);

            lanes[item.Lane - 1]?.AssignToLane (_creature, item.player == P1 ? true : false);

        }
    }
}