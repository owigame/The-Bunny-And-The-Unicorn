
using UnityEngine;
using AI;

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