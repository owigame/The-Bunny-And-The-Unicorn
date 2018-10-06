using UnityEngine;
using System.Collections.Generic;
using AI;

[CreateAssetMenu(fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    int enemyCount, creatureCount;
    CreatureBase targetCreature,closestEnemy;


    public override void OnTick(IBoardState[] data)
    {
        //--Have at least one creature in each lane at all times...
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int playerInLane = lane.GetFriendliesInLane(this).Count;
            if (playerInLane <= 0)
            {
                AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber);
            }
        }

        //--Get Enemy Count--
        GetCount();

        //--If there are Enemies, find the closest--
        if (enemyCount != 0)
        {
            FindNearest();
        }

        //--if neareast found, move towards and attack--
        if (targetCreature != null)
        {
            AttemptAttackMoveTarget(targetCreature);
        }
        //--else random move or attack--
        else
        {
            AttemptMoveAttack();
        }

        AIResponse.FinalizeResponse();
    }

    //--Get total enemies in play--
    void GetCount()
    {
        //--reset count each round...
        enemyCount = 0;
       // creatureCount = 0;

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            enemyCount += lane.GetEnemiesInLane(this).Count;
           // creatureCount += lane.GetFriendliesInLane(this).Count;
        }
    }

    //--Find enemy nearest player--
    void FindNearest()
    {
        List<CreatureBase> closestCreatures = new List<CreatureBase>();

        //--foreach lane
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            //--foreach creature in that lane
            foreach (CreatureBase creature in lane.GetEnemiesInLane(this))
            {
                //--Get the nearest
                int highestLaneProgress = 0;
                if (creature.LaneProgress >= highestLaneProgress)
                {
                    highestLaneProgress = creature.LaneProgress;
                    closestEnemy = creature;
                }
            }
            //--add to list
            closestCreatures.Add(closestEnemy);
        }
       // Debug.Log("Closest Creature count: " + closestCreatures.Count);

        //--Find the nearest from the list
        foreach (CreatureBase creature in closestCreatures)
        {
            int highestLaneProgress = 0;
           // Debug.Log("creature lane progress: " + creature.LaneProgress);
            if (creature != null)
            {
                if (creature.LaneProgress >= highestLaneProgress)
                {
                    highestLaneProgress = creature.LaneProgress;
                    targetCreature = creature;
                }
                else
                {
                    targetCreature = null;
                }
            }
        }
    }

    //--Random attack or move--
    void AttemptMoveAttack()
    {
        if (_Creatures.Count > 0)
        {
        Debug.Log("Attack or Move");
            CreatureBase toMove = _Creatures[Random.Range(0, _Creatures.Count)];
            // int distance = toMove.ActiveLaneNode.laneManager.GetOpenNodes(toMove.ActiveLaneNode, _RightFacing);
            // AIResponse.Move(toMove, distance);

            if (toMove != null)
            {
                List<CreatureBase> searchTargetCreatures = toMove.ActiveLaneNode.laneManager.SearchRange((int)toMove.Range, toMove.ActiveLaneNode, this);
                bool foundAttackTarget = false;
                foreach (CreatureBase _creature in searchTargetCreatures)
                {
                    if (_creature.Owner != toMove.Owner)
                    { //Found enemy creature in range
                        foundAttackTarget = true;
                        AIResponse.Attack(toMove);
                    }
                }
                if (!foundAttackTarget)
                {
                    int moveSpaces = toMove.ActiveLaneNode.laneManager.GetOpenNodes(toMove.ActiveLaneNode, _RightFacing);
                    if (moveSpaces > AIResponse.Tokens)
                    {
                        moveSpaces = AIResponse.Tokens;
                    }
                    AIResponse.Move(toMove, moveSpaces);
                }
            }
        }
    }

    void AttemptAttackMoveTarget(CreatureBase creature)
    {
        //Find friendly in same lane as _targetCreature
        CreatureBase closestFriendly = creature.ActiveLaneNode.laneManager.GetFriendliesInLane(this)[0];
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
            int openNodes = targetCreature.ActiveLaneNode.laneManager.GetOpenNodes(closestFriendly.ActiveLaneNode, _RightFacing);
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
}