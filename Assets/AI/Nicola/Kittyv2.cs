using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu(fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    int enemyCount, creatureCount;
    CreatureBase targetCreature, closestEnemy;
    bool alternateTick = false;

    struct AttackingPair
    {
        public CreatureBase creature1;
        public CreatureBase creature2;
    }

    List<AttackingPair> attackingPairs = new List<AttackingPair>();

    public override void OnTick(IBoardState[] data)
    {
        alternateTick = !alternateTick;

        if (AIResponse.Tokens >= 20)
        {
            Debug.Log("Find most progressed creature or open lane and move till end");
        }
        else
        {
            //--Have at least one creature in each lane at all times...
            foreach (LaneManager lane in TournamentManager._instance.lanes)
            {
                int playerInLane = lane.GetFriendliesInLane(this).Count;
                int enemyInLane = lane.GetEnemiesInLane(this).Count;
                if (playerInLane <= 0)
                {
                    AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber);
                }
                if (enemyInLane > playerInLane)
                {
                    //  ManagePairs(lane);
                }
            }

            //--Get Enemy Count--
            GetCount();

            //--If there are Enemies, find the closest--
            if (enemyCount > 0)
            {
                FindNearest();
                //--if neareast found, move towards and attack--
                if (targetCreature != null)
                {
                    Debug.Log("move to nearest");
                    AttemptAttackMoveTarget(targetCreature);
                }
            }
            //--else random move or attack--
            else
            {
                Debug.Log("move random");
                AttemptMoveAttack();
            }
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

        //--Find the nearest from the list
        foreach (CreatureBase creature in closestCreatures)
        {
            int highestLaneProgress = 0;
            if (creature != null)
            {
                if (creature.LaneProgress >= highestLaneProgress)
                {
                    highestLaneProgress = creature.LaneProgress;
                    targetCreature = creature;
                }
            }
        }
    }

    //--Random attack or move--
    void AttemptMoveAttack()
    {
        if (_Creatures.Count > 0)
        {
            CreatureBase toMove = _Creatures[Random.Range(0, _Creatures.Count)];

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
        List<CreatureBase> friendlies = creature.ActiveLaneNode.laneManager.GetFriendliesInLane(this);
        if (friendlies.Count > 0)
        {
            CreatureBase closestFriendly = friendlies[0];
            //--Attempt to attack...
            int openNodes = closestFriendly.ActiveLaneNode.laneManager.GetOpenNodes(closestFriendly.ActiveLaneNode, _RightFacing);
            LogStack.Log("$$$$ OpenNodes - " + closestFriendly.GetInstanceID() + ": " + openNodes, Logging.LogLevel.System);
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

    IEnumerator ManagePairs(LaneManager _lane)
    {
        /* 
        Find imbalance of enemies vs friendlies
        Initiate pair spawning - new AttackPair(Creature1, Creature2)
        Wait for alternate tick for 2nd creature spawn

        bool currentTick = alternateTick;
        1. Spawn first creature
        while (alternateTick == currentTick){
            yield return null;
        }
        2. Spawn second creature
        3. attackingPairs.Add(new AttackingPair{creature1 = _creature1, creature2 = _creature2})
        */

        CreatureBase _creature1, _creature2;

        bool currentTick = alternateTick;
        //--Spawn First Creature on first tick--
        AIResponse.Spawn(Spawnable.Bunny, _lane.LaneNumber);
        while (alternateTick == currentTick)
        {
            yield return null;
        }
        //--Spawn Second Creature on second tick--
        AIResponse.Spawn(Spawnable.Unicorn, _lane.LaneNumber);

        //--Add to list of pairs--
        attackingPairs.Add(new AttackingPair { creature1 = _creature1, creature2 = _creature2 });
        yield return null;
    }
}