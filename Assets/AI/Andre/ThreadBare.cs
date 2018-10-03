﻿using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "ThreadBare", menuName = "AI/ThreadBare", order = 0)]
public class ThreadBare : LogicBase {
    LaneManager[] boardState;
    public override void OnTick (IBoardState[] board) {
        boardState = (LaneManager[]) board;

        Debug.Log (boardState.Length);
        LogStack.Log ("Run Auto Response Nearest for:" + AIResponse.Tokens, Logging.LogLevel.Color);
        //for (int i = 0; i < AIResponse.Tokens; i++)
        //{
        //    Auto_Nearest(boardState);
        //}
        Auto_Nearest (boardState);
        //if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        //{
        for (int i = 0; i < boardState.Length; i++) {
            boardState[i].GetFriendliesInLane (this);
            boardState[i].GetEnemiesInLane (this);
        }
        //}
        ////IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

    public CreatureBase GetNearestEnemy (LaneManager[] Board) {
        CreatureBase[] NearestEnemies = GetNearestEnemies (Board);
        if (NearestEnemies.Length > 0) {
            LogStack.Log ("GetNearestEnemy()" + NearestEnemies[0], Logging.LogLevel.Color);
            return NearestEnemies[0];
        } else {
            LogStack.Log ("GetNearestEnemy()" + null, Logging.LogLevel.Color);
            return null;
        }
    }
    public int GetNearestEnemyNodesAway (LaneManager[] Board) {
        LogStack.Log ("GetNearestEnemyNodesAway()", Logging.LogLevel.Color);
        CreatureBase creature = GetNearestEnemy (Board);
        if (creature != null) {
            return creature.ActiveLaneNode.laneManager.GetNodeCount - creature.LaneProgress;
        } else return -1;
    }
    public int GetNearestEnemyLane (LaneManager[] Board) {
        LogStack.Log ("GetNearestEnemyLane()", Logging.LogLevel.Color);
        CreatureBase creature = GetNearestEnemy (Board);
        if (creature != null) {
            return TournamentManager._instance.lanes.IndexOf (creature.ActiveLaneNode.laneManager);
        } else return -1;

    }
    public CreatureBase[] GetNearestEnemies (LaneManager[] Board) {
        LogStack.Log ("GetNearestEnemies()", Logging.LogLevel.Color);
        List<CreatureBase> nearestCreatures = new List<CreatureBase> ();
        foreach (LaneManager lane in Board) {
            LaneNode startNode = _PlayerNumber == 1 ? lane.startNode : lane.endNode;
            List<CreatureBase> tempCreatures = lane.GetEnemiesInLane (this);
            if (tempCreatures.Count > 0){
                nearestCreatures.Add (tempCreatures[0]);
            }
        }
        LogStack.Log ("Three nearest enemies: " + nearestCreatures.Count, Logging.LogLevel.Debug);
        // Func<int, int, bool> FindNearestOfTwoDependatOnPlayerSide = (x, y) => _PlayerNumber == 1 ? x > y : x < y;

        if (_PlayerNumber == 1)
            nearestCreatures.Sort ((a, b) => a.ActiveLaneNode.laneManager.allNodes.IndexOf (a.ActiveLaneNode).CompareTo (b.ActiveLaneNode.laneManager.allNodes.IndexOf (b.ActiveLaneNode))); // ascending sort
        else
            nearestCreatures.Sort ((a, b) => -1 * a.ActiveLaneNode.laneManager.allNodes.IndexOf (a.ActiveLaneNode).CompareTo (b.ActiveLaneNode.laneManager.allNodes.IndexOf (b.ActiveLaneNode))); // descending sort

        return nearestCreatures.ToArray ();
    }
    //returns your nearest creature and how far away it is
    // retuns null if no creature in the lane
    public Tuple<CreatureBase, int> GetMyNearestCreature (CreatureBase OpponentCreature, LaneManager[] Board) {
        if (OpponentCreature == null) return null;
        List<CreatureBase> creaturesStillInLane = OpponentCreature.ActiveLaneNode.laneManager.SearchRange (OpponentCreature.ActiveLaneNode.laneManager.GetNodeCount, OpponentCreature.ActiveLaneNode, _PlayerNumber == 1 ? TournamentManager._instance.P2 : TournamentManager._instance.P1);
        Func<int, bool> TestIfMine = (x) => creaturesStillInLane[x].Owner._PlayerNumber == this._PlayerNumber ? true : false;
        int testcount = 0;
        for (int i = 0; i < creaturesStillInLane.Count; i++) {
            testcount = i;
            if (TestIfMine (i)) {
                break;
            }
            if (i == creaturesStillInLane.Count && !TestIfMine (i)) {
                return null;
            }
        }

        return Tuple.Create (creaturesStillInLane[testcount], creaturesStillInLane[testcount].LaneProgress - OpponentCreature.LaneProgress);
    }
    // returns your nearest creature and how far away it is from the other xreature
    // retuns null if no creature in the lane
    public Tuple<CreatureBase, int> GetNearestCreatureToNearestEnemy (LaneManager[] Board) {
        Tuple<CreatureBase, int> NearestCreatureToNearestEnemy = GetMyNearestCreature (GetNearestEnemy (Board), Board);
        return NearestCreatureToNearestEnemy;
    }

    public void Auto_Nearest (LaneManager[] Board) {
        Tuple<CreatureBase, int> nearestAndRange = GetNearestCreatureToNearestEnemy (Board);
        if (nearestAndRange != null) {
            LogStack.Log ("I have a nearby unit", Logging.LogLevel.Color);
            if (InRange (nearestAndRange)) {
                if (!AIResponse.Attack (nearestAndRange.Item1)) {
                    LogStack.Log ("Attack Validation check failed", Logging.LogLevel.Color);
                } else LogStack.Log ("Nearby Unit Attacking", Logging.LogLevel.Color);
            } else {
                if (!AIResponse.Move (nearestAndRange.Item1)) {
                    LogStack.Log ("Move Validation check failed", Logging.LogLevel.Color);
                } else LogStack.Log ("Nearby Unit Moving", Logging.LogLevel.Stack);
            }
        } else if (Opponent._Creatures.Count > 0) {
            if (!AIResponse.Spawn (GetNearestEnemyNodesAway (Board) > 3 ? Spawnable.Unicorn : Spawnable.Bunny, GetNearestEnemyLane (Board) < 0 ? 1 : GetNearestEnemyLane (Board))) {
                LogStack.Log ("Spawn Validation check failed", Logging.LogLevel.Stack);
            }
        } else if (Opponent._Creatures.Count == 0) {
            LogStack.Log ("Wait till opponent does something", Logging.LogLevel.Stack);
        }
    }

    public bool InRange (Spawnable CreatureType, int range) {
        return CreatureType == Spawnable.Unicorn ? range >= 3 : range == 1;
    }
    public bool InRange (Tuple<CreatureBase, int> TypeAndRange) {
        return TypeAndRange.Item1.CreatureType == Spawnable.Unicorn ? TypeAndRange.Item2 >= 3 : TypeAndRange.Item2 == 1;
    }

    //Func<CreatureBase, bool> TestIfMine = (x) => x.Owner._PlayerNumber == _PlayerNumber ? true : false;
    public LogicBase Opponent {
        get {
            return this == TournamentManager._instance.P1 ? TournamentManager._instance.P2 : TournamentManager._instance.P1;
        }
    }
}

[CreateAssetMenu (fileName = "Pattern_", menuName = "LaneControl/LanePattern", order = 0)]
public class LanePattern : ScriptableObject {
    Spawnable[] PatternDefinition;
}