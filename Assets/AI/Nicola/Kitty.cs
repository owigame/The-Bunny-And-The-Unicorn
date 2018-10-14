using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Kitty", menuName = "AI/Kitty", order = 0)]
public class Kitty : LogicBase
{

    List<CreatureBase> closestCreatures = new List<CreatureBase>();
    CreatureBase _targetCreature, closestEnemy;

    int enemyCount, friendlyCount;
    public override void OnTick (IBoardState[] data)
    {
        int maxLoop = 99;
        while (maxLoop > 0)
        {
            maxLoop--;
            //--get availables---
            CheckForAvailables();
            if (friendlyCount <= 0 || enemyCount <= 0)
            {
                //--No friendlies inplay, so spawn or wait or move
                int randomLane = Random.Range(1, TournamentManager._instance.lanes.Count + 1);
                AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane);
            }
            //--spanw player in lanes 
            else
            {
                SpawnCreature();
            }

            if (_targetCreature != null)
            {
                SpawnToClosest();
                AttemptMoveAttack(_targetCreature);
            }
        }

        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

    //--Spawn creatures when no target enemy found...
    void SpawnCreature()
    {
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int enemyAmount = lane.GetEnemiesInLane(this).Count;
            int creatureAmount = lane.GetFriendliesInLane(this).Count;
            int toSpawn = Mathf.Abs(enemyAmount - creatureAmount);
            for (int i = 0; i < toSpawn; i++)
            {
                //--attampt spawn in lane---
                if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber))
                {
                    Debug.Log("Can't Spawn in " + lane.LaneNumber);
                }
            }
        }
    }

    //--Spawn creature to target enemy...
    void SpawnToClosest()
    {
        if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, TournamentManager._instance.lanes.IndexOf(closestEnemy.ActiveLaneNode.laneManager) + 1))
        {
            Debug.Log("Could not spawn to nearest enemy");
        }
    }

    //--Attempt to attack target creature...
    void AttemptMoveAttack (CreatureBase creature)
    {
        //Find friendly in same lane as _targetCreature
        CreatureBase closestFriendly = creature.ActiveLaneNode.laneManager.GetFriendliesInLane (this) [0];
        if (closestFriendly == null)
        {
            //No friendly, do something else...
            if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, creature.ActiveLaneNode.laneManager.LaneNumber))
            {
               //
            }
        }
        else
        {
            //--Attempt to attack...
            int openNodes = _targetCreature.ActiveLaneNode.laneManager.GetOpenNodes(closestFriendly.ActiveLaneNode, _RightFacing);
            //--is enemy in attack range? Attack...
            if (openNodes < closestFriendly.Range)
            {
                AIResponse.Attack(closestFriendly);
            }
            //--Else move...
            else
            {
                AIResponse.Move(closestFriendly, openNodes);
            }
        }
    }

    //--Check if creatures and enemies are in play...
    void CheckForAvailables()
    {
        //--lanes with enemies...
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            //--get available enemies...
            enemyCount = lane.GetEnemiesInLane(this).Count;
            //--get available self...
            friendlyCount = lane.GetFriendliesInLane(this).Count;
        }
    }

    //--Get enemy closest to creature...
    void GetNearest()
    {
        //--get nearest enemy in each lane--
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            foreach (CreatureBase creature in lane.GetEnemiesInLane(this))
            {
                int highestLaneProgress = 0;
                if (creature.LaneProgress > highestLaneProgress)
                {
                    closestEnemy = creature;
                    highestLaneProgress = closestEnemy.LaneProgress;
                }
            }
            closestCreatures.Add(closestEnemy);
        }

        //--get closest enemy from nearest in each lane--
        if (closestCreatures.Count > 0)
        {
            foreach (CreatureBase enemycreature in closestCreatures)
            {
                int highestLaneProgress = 0;

                if (enemycreature.LaneProgress > highestLaneProgress)
                {
                    _targetCreature = enemycreature;
                    highestLaneProgress = enemycreature.LaneProgress;
                }
            }
        }
    }
}