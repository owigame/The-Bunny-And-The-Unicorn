using System;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "ThreadBear", menuName = "AI/ThreadBear", order = 0)]
public class ThreadBear : ThreadBare {
    public LanePattern LanePattern;
    public int Threshhold = 3;
    public override void OnTick (IBoardState[] data) {
        boardState = (LaneManager[]) data;

        for (int i = 1; i < (TournamentManager._instance.tokensPerRound==1?3:6); i++) {
            FormPattern (LanePattern, i, boardState);
        }
        //for (int i = 0; i < 3; i++)
        //{
        //    AIResponse.Attack(boardState[i].GetFriendliesInLane(this).Count>0? boardState[i].GetFriendliesInLane(this)[0]:null);
        //}

        Auto_Nearest (boardState);

        CreatureBase nearest = null;
        for (int i = 0; i < 2; i++) {
            if (boardState[i].GetFriendliesInLane (this).Count != 0) {
                if (nearest != null) {
                    if (boardState[i].GetFriendliesInLane (this) [0].LaneProgress > nearest.LaneProgress) {
                        nearest = boardState[i].GetFriendliesInLane (this) [0];
                    }
                } else nearest = boardState[i].GetFriendliesInLane (this) [0];
            }
        }

        if (AIResponse.Tokens > Threshhold)
            AIResponse.Move (nearest, AIResponse.Tokens - Threshhold);
        else
            AIResponse.Move (nearest, 1);

        for (int i = 0; i < AIResponse.Tokens; i++) {
            Auto_Nearest (boardState);
        }

        AIResponse.FinalizeResponse ();
    }

    protected void FormPattern (LanePattern pattern, int lane, LaneManager[] Board) {
        LaneManager TheLane = Board[lane - 1];

        CreatureBase[] FriendlyCreatures = TheLane.GetFriendliesInLane (this).ToArray ();
        if (FriendlyCreatures.Length == 0) {
            foreach (Spawnable spawntype in pattern.PatternDefinition) {
                AIResponse.Spawn (spawntype, lane);
            }
            return;
        }
        LanePattern lanePattern = new LanePattern (FriendlyCreatures);
        if (LanePattern.Equals (lanePattern)) return;

        for (int i = 0; i < LanePattern.PatternDefinition.Length; i++) {
            if (lanePattern.PatternDefinition.Length <= i || LanePattern.PatternDefinition[i] != lanePattern.PatternDefinition[i])
                if (!AIResponse.Spawn (LanePattern.PatternDefinition[i], lane)) {
                    Debug.Log ("~~~~ Failed Spawn in Pattern");
                } else {
                    Debug.Log ("~~~~ Success Spawn in Pattern");
                }
        }
    }
}

[CreateAssetMenu (fileName = "Pattern_", menuName = "LaneControl/LanePattern", order = 0)]

public class LanePattern : ScriptableObject, IEquatable<LanePattern> {
    public Spawnable[] PatternDefinition;
    public LanePattern (Spawnable[] CreatureType) {
        PatternDefinition = CreatureType;
    }
    public LanePattern (CreatureBase[] CreatureType) {
        List<Spawnable> CreatureSpawnable = new List<Spawnable> ();
        foreach (var item in CreatureType) {
            CreatureSpawnable.Add (item.CreatureType);
        }
        PatternDefinition = CreatureSpawnable.ToArray ();
    }
    public bool Equals (LanePattern other) {
        if (other.PatternDefinition != PatternDefinition) return false;
        else return true;
    }
}