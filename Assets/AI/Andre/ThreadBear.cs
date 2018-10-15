
using UnityEngine;
using AI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ThreadBear", menuName = "AI/ThreadBear", order = 0)]
public class ThreadBear :  ThreadBare
{
    public LanePattern lanePattern1;
    public override void OnTick(IBoardState[] data)
    {
        boardState = (LaneManager[])data;
    
        for (int i = 1; i < 3; i++)
        {
            FormPattern(lanePattern1,i,boardState);
        }
        for (int i = 0; i < 3; i++)
        {
            AIResponse.Attack(boardState[i].GetFriendliesInLane(this).Count>0? boardState[i].GetFriendliesInLane(this)[0]:null);
        }

        Auto_Nearest(boardState);

        CreatureBase nearest = null;
        for (int i = 0; i < 2; i++)
        {
            if (boardState[i].GetFriendliesInLane(this).Count != 0 )
            {
                if (nearest != null)
                {
                    if (boardState[i].GetFriendliesInLane(this)[0].LaneProgress > nearest.LaneProgress)
                    {
                        nearest = boardState[i].GetFriendliesInLane(this)[0];
                    }
                }
                else nearest = boardState[i].GetFriendliesInLane(this)[0];
            }
        }
        AIResponse.Move(nearest);

        AIResponse.FinalizeResponse();
    }
    protected void FormPattern(LanePattern pattern,int lane, LaneManager[] Board)
    {
        LaneManager TheLane = Board[lane - 1];

        CreatureBase[] FriendlyCreatures = TheLane.GetFriendliesInLane(this).ToArray();
        if (FriendlyCreatures.Length == 0)
        {
            foreach (var spawntype in pattern.PatternDefinition)
            {
                AIResponse.Spawn(spawntype, lane);
            }
            return;
        }
        LanePattern lanePattern = new LanePattern(FriendlyCreatures);
        if (lanePattern1.Equals(lanePattern)) return;
        for (int i = 0; i < lanePattern1.PatternDefinition.Length; i++)
        {
            if (lanePattern.PatternDefinition.Length <= i) AIResponse.Spawn(lanePattern1.PatternDefinition[i], lane);
        }
    }
}