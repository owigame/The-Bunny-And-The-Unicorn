using System.Collections.Generic;
using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "Kitty", menuName = "AI/Kitty", order = 0)]
public class Kitty : LogicBase
{
    CreatureBase _targetCreature;
    public override void OnTick(IBoardState[] data)
    {
        //--list of closest creature in each lane--
        List<CreatureBase> closestCreatures = new List<CreatureBase>();
		//--lanes with enemies...
        foreach (LaneManager lane in TournamentManager._instance.lanes){
            int highestLaneProgress = 0;
            CreatureBase closestEnemy = null;
            //--each enemy in lane--
            foreach(CreatureBase creature in lane.GetEnemiesInLane(this)){
                if (creature.LaneProgress > highestLaneProgress){
                    closestEnemy = creature;
                    highestLaneProgress = closestEnemy.LaneProgress;
                }
            }
            closestCreatures.Add(closestEnemy);
            
        }

        foreach(CreatureBase enemycreature in closestCreatures)
        {
            int highestLaneProgress = 0;
            if (enemycreature.LaneProgress > highestLaneProgress)
            {
                _targetCreature = enemycreature;
                highestLaneProgress = enemycreature.LaneProgress;
            }
        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }
}