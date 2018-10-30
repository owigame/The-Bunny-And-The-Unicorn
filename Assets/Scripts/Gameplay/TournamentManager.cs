using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class TournamentManager : MonoBehaviour {
    #region  Singleton management
    public static TournamentManager _instance;
    public delegate void StartEvent ();
    public static StartEvent OnStart;

    private void Awake () {
        if (_instance == null) {
            _instance = this;
            OnTick = new TickEvent ();
            OnTick.AddListener (RoundCount);

            //Set Player listeners
            // OnTick.AddListener (P1.OnTick);
            // OnTick.AddListener (P2.OnTick);

        } else {
            Destroy (this);
        }
    }

    private void OnDestroy () {
        _instance = null;
    }
    #endregion

    public TickEvent OnTick;

    public delegate void CreatureEvents (CreatureBase creature);
    public static CreatureEvents OnCreatureDead;

    [Header ("Player Round Robin")]
    public List<AI.LogicBase> participants = new List<AI.LogicBase> ();
    int p1Index, p2Index;

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
    public int roundLimit = 150;

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
    public int roundCount = 0;

    int playersReady = 0;
    int lanesReady = 0;

    // Setup the two Ai players 
    private void Start () {

        if (OnStart != null) {
            OnStart ();
        }

        //Set Lane listeners
        foreach (LaneManager lane in lanes) {
            lane.OnLaneReady.AddListener (LaneReady);
        }

        // P1.init ();
        // P2.init ();
        NextPlayers ();

        IBoardState[] data = lanes.ToArray ();
        OnTick.Invoke (data);

        // LogStack.Log ("Tournament Manager initialised", LogLevel.System);
    }

    void LaneReady () {
        lanesReady++;
    }

    void RoundCount (IBoardState[] data) {
        roundCount++;
        if (roundCount >= roundLimit) {
            if (player1Score > player2Score) {
                Winner (P1);
            } else if (player1Score < player2Score) {
                Winner (P2);
            } else {
                Time.timeScale = 0;
                UIManager._instance.Winner ("", true);
            }
        }
    }

    void Winner (AI.LogicBase winner) {
        Time.timeScale = 0;
        UIManager._instance.Winner (winner.name);
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

    public void NextPlayers () {
        if (P1 != null) {
            OnTick.RemoveListener (P1.OnTick);

            foreach (CreatureBase creature in P1._Creatures) {
                DestroyImmediate (creature.gameObject);
            }
            P1._Creatures = new List<CreatureBase> ();
        }
        if (P2 != null) {
            OnTick.RemoveListener (P2.OnTick);

            foreach (CreatureBase creature in P2._Creatures) {
                DestroyImmediate (creature.gameObject);
            }
            P2._Creatures = new List<CreatureBase> ();
        }

        TickManager._instance.ResetTickManager();

        player1Score = 0;
        player2Score = 0;
        roundCount = 0;

        UIManager._instance.WinnerReset ();

        foreach (LaneManager lane in lanes) {
            foreach (LaneNode node in lane.allNodes) {
                node.activeCreature = null;
            }
        }

        //Next from round robin
        if (!IterateP2 ()) {
            Time.timeScale = 0;
            UIManager._instance.EndGame ();
            return;
        }
        P1 = participants[p1Index];
        P2 = participants[p2Index];

        OnTick.AddListener (P1.OnTick);
        OnTick.AddListener (P2.OnTick);

        P1.init ();
        P2.init ();

        UIManager._instance.NextRound ();

        Time.timeScale = 1;
    }

    bool IterateP1 () {
        if (p1Index < participants.Count) {
            p1Index++;
            return true;
        } else {
            Debug.LogWarning ("$$$$ REACHED END OF ROUND ROBIN $$$$");
            return false;
        }
    }

    bool IterateP2 () {
        if (p2Index < participants.Count) {
            p2Index++;
            if (participants[p2Index] == participants[p1Index]) {
                IterateP2 ();
            }
        } else {
            p2Index = 0;
            if (!IterateP1 ()) {
                return false;
            }
        }
        return true;
    }

}