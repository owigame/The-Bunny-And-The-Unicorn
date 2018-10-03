using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "ThreadBare", menuName = "AI/ThreadBare", order = 0)]
public class ThreadBare : LogicBase
{
    LaneManager[] mng;
    public override void OnTick(IBoardState[] board)
    {
        if (board.GetType() == typeof(LaneManager))
            mng = board as LaneManager[];

        //if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        //{

        //}
        ////IResponse[] responses = AIResponse.QueryResponse();
        //AIResponse.FinalizeResponse();
    }

    public CreatureBase getNearestEnemy(LaneManager[] Board)
    {
        foreach (LaneManager lane in Board)
        {

        }

        return null;
    }

}