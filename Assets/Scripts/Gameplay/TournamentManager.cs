using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class TournamentManager : MonoBehaviour {
    #region  Singleton management
    public static TournamentManager _instance;
    public delegate void StartEvent();
    public static StartEvent OnStart;


    private void Awake () {
        if (_instance == null) {
            _instance = this;
            OnTick = new TickEvent ();

            //Set Player listeners
            OnTick.AddListener (P1.OnTick);
            OnTick.AddListener (P2.OnTick);
        } else {
            Destroy (this);
        }
    }
    private void OnDestroy () {
        _instance = null;
    }
    #endregion

    public TickEvent OnTick;

    public delegate void CreatureEvents(CreatureBase creature);
    public static CreatureEvents OnCreatureDead;

    [Header ("Player Setup")]
    public AI.LogicBase P1, P2;

    [Header ("Lanes")]
    public List<LaneManager> lanes = new List<LaneManager> ();

    [Header ("Character Settings")]
    public int offTickInterval = 1;
    public int onTickInterval = 2;
    public int bunnyRange = 1;
    public int unicornRange = 3;
    public float bunnyDamage = 3;
    public float unicornDamage = 1;

    [Header ("Game Settings")]
    public int tokensPerRound = 1;

    [Header ("Timing")]
    public float moveWait = 1;
    public float attackWait = 1;
    public float dieWait = 1;
    public float laneReadyWait = 1;

    [Header ("Prefabs")]
    public GameObject unicornPrefab;
    public GameObject bunnyPrefab;
    public GameObject uiHealthBar;

    [Header ("References")]
    public Transform mainCanvas;

    [Header ("Game State")]
    public int player1Score = 0;
    public int player2Score = 0;

    int playersReady = 0;
    int lanesReady = 0;

    // Setup the two Ai players 
    private void Start () {

        if (OnStart != null){
            OnStart();
        }

        //Set Lane listeners
        foreach (LaneManager lane in lanes) {
            lane.OnLaneReady.AddListener (LaneReady);
        }

        P1.init ();
        P2.init ();

        IBoardState[] data = FindObjectsOfType<LaneManager> ();
        OnTick.Invoke (data);

        // LogStack.Log ("Tournament Manager initialised", LogLevel.System);
    }

    void LaneReady () {
        lanesReady++;
    }

    void Winner (AI.LogicBase winner) {
        Time.timeScale = 0;
        UIManager._instance.Winner(winner.name);
    }

    public void ScoreUpdate (CreatureBase creature) {
        if (creature.Owner == P1) {
            player1Score++;
            if (player1Score >= 3) {
                Winner (creature.Owner);
            }
        } else if (creature.Owner == P2) {
            player2Score++;
            if (player2Score >= 3) {
                Winner (creature.Owner);
            }
        }
        UIManager._instance.UpdateScore ();
    }

    public MonoBehaviour SpawnHelper(Type helper){
        GameObject helperGO = new GameObject();
        return helperGO.AddComponent(helper) as MonoBehaviour;
    }

}