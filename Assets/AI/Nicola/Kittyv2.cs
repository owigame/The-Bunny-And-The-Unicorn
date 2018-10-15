using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    int enemyCount, creatureCount;
    CreatureBase targetCreature, closestEnemy;
    public bool alternateTick = false;
    bool runOnce = true;
    // KittyHelper _helper;

    public struct AttackingPair
    {
        public CreatureBase creature1;
        public CreatureBase creature2;
    }

    public List<AttackingPair> attackingPairs = new List<AttackingPair> ();

    public void Init ()
    {
        // _helper = TournamentManager._instance.SpawnHelper(KittyHelper);
    }

    public override void OnTick (IBoardState[] data)
    {
        // if (runOnce)
        // {
        //     runOnce = false;
        //     Init ();
        // }
        alternateTick = !alternateTick;

        if (AIResponse.Tokens >= 20)
        {
            LaneManager _lane;
            CreatureBase _friendlyToMove = null;
            int previousEnemies = 0;
            bool _moveAttack = true;

            //--check each lane--
            foreach (LaneManager lane in TournamentManager._instance.lanes)
            {
                int playerInLane = lane.GetFriendliesInLane (this).Count;
                int enemyInLane = lane.GetEnemiesInLane (this).Count;
                //--Find Open lane--
                if (enemyInLane == 0 && playerInLane > 0)
                {
                    _lane = lane;
                    _friendlyToMove = _lane.GetFriendliesInLane (this) [0];
                    //--move the creature to end of lane---
                    int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                    AIResponse.Move (_friendlyToMove, _openNodes);
                    _moveAttack = false;
                    break;
                }
                //--ELSE Find lane with least amount of enemies--
                else if (enemyInLane > 0 && playerInLane > 0)
                {
                    if (enemyInLane > previousEnemies)
                    {
                        previousEnemies = enemyInLane;
                        _lane = lane;
                        _friendlyToMove = _lane.GetFriendliesInLane (this) [0];
                    }
                }
            }

            if (_moveAttack)
            {
                //--Move to enemy, attack, then move to end of Lane--    
                // _enemyToAttack = _lane.GetEnemiesInLane (this) [0];
                int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                AIResponse.Move (_friendlyToMove, _openNodes);
                AIResponse.Attack (_friendlyToMove);
                _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                AIResponse.Move (_friendlyToMove, _openNodes);
            }
        }
        else
        {
            //--Have at least one creature in each lane at all times...
            foreach (LaneManager lane in TournamentManager._instance.lanes)
            {
                int playerInLane = lane.GetFriendliesInLane (this).Count;
                int enemyInLane = lane.GetEnemiesInLane (this).Count;
                if (playerInLane <= 0 || enemyInLane < playerInLane)
                {
                    // AIResponse.Spawn (Random.Range (0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber);
                    SpawnNewPair (lane);
                }
                else if (enemyInLane > playerInLane)
                {
                    //  ManagePairs(lane);
                    // if (_helper != null){
                    SpawnNewPair (lane);
                    // } else {
                    //     Debug.LogError("No Helper Spawned");
                    //     Init();
                    // }
                }
            }

            //--Get Enemy Count--
            GetCount ();

            //--If there are Enemies, find the closest--
            if (enemyCount > 0)
            {
                FindNearest ();
                //--if neareast found, move towards and attack--
                if (targetCreature != null)
                {
                    Debug.Log ("move to nearest");
                    AttemptAttackMoveTarget (targetCreature);
                }
            }
            //--else random move or attack--
            else
            {
                Debug.Log ("move random");
                AttemptMoveAttack ();
            }
        }
        AIResponse.FinalizeResponse ();
    }

    //--Get total enemies in play--
    void GetCount ()
    {
        //--reset count each round...
        enemyCount = 0;
        // creatureCount = 0;

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            enemyCount += lane.GetEnemiesInLane (this).Count;
            // creatureCount += lane.GetFriendliesInLane(this).Count;
        }
    }

    //--Find enemy nearest player--
    void FindNearest ()
    {
        List<CreatureBase> closestCreatures = new List<CreatureBase> ();

        //--foreach lane
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            //--foreach creature in that lane
            foreach (CreatureBase creature in lane.GetEnemiesInLane (this))
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
            closestCreatures.Add (closestEnemy);
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
    void AttemptMoveAttack ()
    {
        if (_Creatures.Count > 0)
        {
            CreatureBase toMove = _Creatures[Random.Range (0, _Creatures.Count)];

            if (toMove != null)
            {

                bool foundInPairs = false;
                foreach (AttackingPair pair in attackingPairs)
                {

                    if (pair.creature1 == toMove || pair.creature2 == toMove)
                    {
                        foundInPairs = true;
                        //Move Pair
                    }
                }

                List<CreatureBase> searchTargetCreatures = toMove.ActiveLaneNode.laneManager.SearchRange ((int) toMove.Range, toMove.ActiveLaneNode, this);
                bool foundAttackTarget = false;
                foreach (CreatureBase _creature in searchTargetCreatures)
                {
                    if (_creature.Owner != toMove.Owner)
                    { //Found enemy creature in range
                        foundAttackTarget = true;
                        AIResponse.Attack (toMove);
                    }
                }
                if (!foundAttackTarget)
                {
                    int moveSpaces = toMove.ActiveLaneNode.laneManager.GetOpenNodes (toMove.ActiveLaneNode, _RightFacing);
                    if (moveSpaces > AIResponse.Tokens)
                    {
                        moveSpaces = AIResponse.Tokens;
                    }
                    AIResponse.Move (toMove, moveSpaces);
                }
            }
        }
    }

    void AttemptAttackMoveTarget (CreatureBase creature)
    {
        //Find friendly in same lane as _targetCreature
        List<CreatureBase> friendlies = creature.ActiveLaneNode.laneManager.GetFriendliesInLane (this);
        if (friendlies.Count > 0)
        {
            CreatureBase closestFriendly = friendlies[0];
            //--Attempt to attack...
            int openNodes = closestFriendly.ActiveLaneNode.laneManager.GetOpenNodes (closestFriendly.ActiveLaneNode, _RightFacing);
            LogStack.Log ("$$$$ OpenNodes - " + closestFriendly.GetInstanceID () + ": " + openNodes, Logging.LogLevel.System);
            //--is enemy in attack range? Attack...
            if (openNodes < closestFriendly.Range)
            {
                AIResponse.Attack (closestFriendly);
            }
            //--Else move...
            else
            {
                AIResponse.Move (closestFriendly, openNodes);
            }
        }
    }

    bool tickChanged = false;
    bool currentTick;
    bool spawnTwo = false;
    public Kittyv2 owner;
    bool spawningPairs = false;

    IEnumerator managedPairs;

    public void SpawnNewPair (LaneManager _lane)
    {
        if (!spawningPairs){
            spawningPairs = true;
        managedPairs = ManagePairs(_lane);
        Debug.Log("SPAWNNEWPAIR() " + TournamentManager._instance);
        TournamentManager._instance.StartCoroutine (managedPairs);
        }
    }

    IEnumerator ManagePairs (LaneManager _lane)
    {

        // LaneManager _lane = null;

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
    
        Validate spawn 1 then move on to spawn 2 and validate
    
    
        */

        currentTick = alternateTick;
        CreatureBase _creature1 = null, _creature2 = null;

        //--Spawn First Creature on first tick--

        if (!SpawnOne (Spawnable.Bunny, _lane.LaneNumber))
        {
            TournamentManager._instance.StartCoroutine (WaitForTickChange ());
            while (!tickChanged)
            {
                yield return null;
            }
            tickChanged = false;
            TournamentManager._instance.StartCoroutine (ManagePairs (_lane));
        }

        TournamentManager._instance.StartCoroutine (WaitForTickChange ());
        _creature1 = _lane.GetFirstLaneNode(this).activeCreature;
        AIResponse.Move (_creature1, _lane.LaneNumber);

        TournamentManager._instance.StartCoroutine (SpawnTwo (_lane.LaneNumber));

        while (spawnTwo == false){
            yield return null;
        }

        _creature2 = _lane.GetFirstLaneNode(this).activeCreature;


        //--Add to list of pairs--
        attackingPairs.Add (new Kittyv2.AttackingPair { creature1 = _creature1, creature2 = _creature2 });
        spawningPairs = false;
        yield return null;
    }

    bool SpawnOne (Spawnable type, int lane)
    {
        if (AIResponse.Spawn (type, lane))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator SpawnTwo (int laneNumber)
    {
        //--Spawn Second Creature on second tick--
        if (!AIResponse.Spawn (Spawnable.Unicorn, laneNumber))
        {
            TournamentManager._instance.StartCoroutine (WaitForTickChange ());
            while (!tickChanged)
            {
                yield return null;
            }
            tickChanged = false;
            TournamentManager._instance.StartCoroutine (SpawnTwo (laneNumber));
        } else {
            TournamentManager._instance.StartCoroutine (WaitForTickChange ());
            spawnTwo = true;
        }
    }

    IEnumerator WaitForTickChange ()
    {
        currentTick = alternateTick;
        while (alternateTick == currentTick)
        {
            yield return null;
        }
        tickChanged = true;
        currentTick = alternateTick;
    }

}