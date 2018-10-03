using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Kitty", menuName = "AI/Kitty", order = 0)]
public class Kitty : LogicBase
{
    CreatureBase _targetCreature;
    public override void OnTick (IBoardState[] data)
    {
        //--list of closest creature in each lane--
        List<CreatureBase> closestCreatures = new List<CreatureBase> ();
        //--lanes with enemies...
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {

            int enemyCount = lane.GetEnemiesInLane (this).Count;
            int friendlyCount = lane.GetFriendliesInLane (this).Count;

            int highestLaneProgress = 0;
            CreatureBase closestEnemy = null;

            if (enemyCount > 0)
            {
                //--each enemy in lane--
                foreach (CreatureBase creature in lane.GetEnemiesInLane (this))
                {
                    if (creature.LaneProgress > highestLaneProgress)
                    {
                        closestEnemy = creature;
                        highestLaneProgress = closestEnemy.LaneProgress;
                    }
                    if (closestEnemy != null)
                    {
                        if (!AIResponse.Spawn (Random.Range (0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, TournamentManager._instance.lanes.IndexOf (closestEnemy.ActiveLaneNode.laneManager) + 1))
                        {
                            //Could not Spawn
                        }
                    }
                }
                closestCreatures.Add (closestEnemy);
            }
        }

        if (closestCreatures.Count > 0)
        {
            foreach (CreatureBase enemycreature in closestCreatures)
            {
                int highestLaneProgress = 0;
                if (enemycreature != null)
                {
                    if (enemycreature.LaneProgress > highestLaneProgress)
                    {
                        _targetCreature = enemycreature;
                        highestLaneProgress = enemycreature.LaneProgress;
                    }
                    AttemptMoveAttack(_targetCreature);
                }
            }
        }
        else
        {
            int randomLane = Random.Range (1, TournamentManager._instance.lanes.Count + 1);
            //--attampt spawn in lane---
            if (!AIResponse.Spawn (Random.Range (0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane))
            {

            }
        }

        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

    void AttemptMoveAttack (CreatureBase creature)
    {
        //Find friendly in same lane as _targetCreature
        CreatureBase closestFriendly = creature.ActiveLaneNode.laneManager.GetFriendliesInLane (this) [0];
        if (closestFriendly == null)
        {
            //No friendly
        }
        int openNodes = _targetCreature.ActiveLaneNode.laneManager.GetOpenNodes (closestFriendly.ActiveLaneNode, _RightFacing);

        if (openNodes < closestFriendly.Range)
        {
            AIResponse.Attack (closestFriendly);
        }
        else
        {
            AIResponse.Move (closestFriendly, openNodes);
        }
    }
}