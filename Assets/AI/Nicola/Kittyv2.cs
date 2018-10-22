using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    LaneManager[] boardState;

    int enemyCount, creatureCount;
    CreatureBase targetCreature, closestEnemy;
    public bool alternateTick = false;

    protected bool pairsReady1, pairsReady2, pairsReady3;


    public List<AttackingPair> attackingPairs = new List<AttackingPair> ();

    IEnumerator SpawnOne(Kittyv2 kitta)
    {
        yield return kitta.AIResponse.Spawn(Spawnable.Unicorn,1);
    }
    public override void OnTick (IBoardState[] data)
    {
        boardState = (LaneManager[]) data;
        #region dont use
        ////TournamentManager._instance.StartCoroutine(SpawnOne(this) );
        //if (_Start)
        //{
        //    spawningPairs1 = false;
        //    spawningPairs2 = false;
        //    spawningPairs3 = false;
        //    pairsReady1 = true;
        //    pairsReady2 = true;
        //    pairsReady3 = true;

        //    // Debug.Break();
        //    _Start = false;
        //}
        #endregion
        alternateTick = !alternateTick;

        //--Have at least one creature in each lane at all times...
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int playerInLane = lane.GetFriendliesInLane(this).Count;
            //int enemyInLane = lane.GetEnemiesInLane(this).Count;
            if (playerInLane <= 0 )
            {
                AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber);
            }
           
        }

        if (AIResponse.Tokens >= 20)
        {
            LaneManager _lane = null;
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

           // CreatureBase _enemies[] = _lane.GetEnemiesInLane(this);

            if (_moveAttack && _friendlyToMove != null)
            {
                //attemp movetowards and attack each enemy in lane, then move to end of lane---
                Debug.Log ("move the creature...");
                foreach (CreatureBase _enemy in _lane.GetEnemiesInLane(this))
                {
                    //--Move to enemy, attack, then move to end of Lane--    
                    int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                    if (!AIResponse.Move (_friendlyToMove, _openNodes))
                    {
                        AIResponse.Attack (_friendlyToMove);
                    }
                        AIResponse.Attack (_friendlyToMove);

                } 
                    
                // _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                // AIResponse.Move (_friendlyToMove, _openNodes);
            }
        }
        else
        {

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
        // TournamentManager._instance.StartCoroutine(AllReady());
        AIResponse.FinalizeResponse();

    }

    IEnumerator AllReady(){
        while (pairsReady1 == false || pairsReady2 == false || pairsReady3 == false){
            Debug.Log("Waiting for Ready");
            yield return null;
        }
        Debug.Log("***** ALL READY *****");
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
                if (!foundInPairs)
                {
                    List<CreatureBase> searchTargetCreatures = toMove.ActiveLaneNode.laneManager.SearchRange ((int) toMove.Range, toMove.ActiveLaneNode, this);
                    bool foundAttackTarget = false;
                    foreach (CreatureBase _creature in searchTargetCreatures)
                    {
                        if (_creature.Owner != toMove.Owner)
                        {
                            //Found enemy creature in range
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

    #region dont use
    //bool tickChanged = false;
    //bool currentTick;
    //bool spawnTwo = false;
    //public Kittyv2 owner;
    //bool spawningPairs1 = false;
    //bool spawningPairs2 = false;
    //bool spawningPairs3 = false;

    //IEnumerator managedPairs;

    //public void SpawnNewPair (LaneManager _lane)
    //{
    //    if ((!spawningPairs1 && _lane.LaneNumber == 1) || (!spawningPairs2 && _lane.LaneNumber == 2) || (!spawningPairs3 && _lane.LaneNumber == 3))
    //    {
    //        // managedPairs = ManagePairs (_lane);
    //        Debug.Log ("SPAWNNEWPAIR() " + TournamentManager._instance);
    //        TournamentManager._instance.StartCoroutine (ManagePairs (_lane,this));
    //        if (_lane.LaneNumber == 1){
    //            spawningPairs1 = true;
    //            pairsReady1 = false;
    //        }
    //        if (_lane.LaneNumber == 2){
    //            spawningPairs2 = true;
    //            pairsReady2 = false;
    //        }
    //        if (_lane.LaneNumber == 3){
    //            spawningPairs3 = true;
    //            pairsReady3 = false;
    //        }
    //    }
    //}

    //IEnumerator ManagePairs (LaneManager _lane,Kittyv2 thebase)
    //{

    //    // LaneManager _lane = null;

    //    /* 
    //    Find imbalance of enemies vs friendlies
    //    Initiate pair spawning - new AttackPair(Creature1, Creature2)
    //    Wait for alternate tick for 2nd creature spawn

    //    bool currentTick = alternateTick;
    //    1. Spawn first creature
    //    while (alternateTick == currentTick){
    //        yield return null;
    //    }
    //    2. Spawn second creature
    //    3. attackingPairs.Add(new AttackingPair{creature1 = _creature1, creature2 = _creature2})
    
    //    Validate spawn 1 then move on to spawn 2 and validate
    
    
    //    */

    //    currentTick = alternateTick;
    //    CreatureBase _creature1 = null, _creature2 = null;

    //    //--Spawn First Creature on first tick--
    //    Debug.Log("PAIRS LANE " + _lane.LaneNumber + " Pre-Spawn");

    //    if (!thebase.AIResponse.Spawn (Spawnable.Bunny, _lane.LaneNumber))
    //    {
    //        Debug.Log("PAIRS LANE " + _lane.LaneNumber + " Failed Spawn");
    //         if (_lane.LaneNumber == 1)  thebase.pairsReady1 = true;
    //         if (_lane.LaneNumber == 2)  thebase.pairsReady2 = true;
    //         if (_lane.LaneNumber == 3)  thebase.pairsReady3 = true;

    //        if (_lane.LaneNumber == 1) spawningPairs1 = false;
    //        if (_lane.LaneNumber == 2) spawningPairs2 = false;
    //        if (_lane.LaneNumber == 3) spawningPairs3 = false;

    //        TournamentManager._instance.StartCoroutine (WaitForTickChange (_lane.LaneNumber));
    //        while (!tickChanged)
    //        {
    //            Debug.Log("Waiting for tick change | 1:" + pairsReady1 + " 2:" + pairsReady2 + " 3:" + pairsReady3 +
    //                " Current lane == "+ _lane.LaneNumber +" Pairs Readyone == "+ pairsReady1 );
    //            yield return null;
    //        }
    //        tickChanged = false;


    //        // SpawnNewPair (_lane);
    //    }
    //    else
    //    {
    //        Debug.Log("PAIRS LANE " + _lane.LaneNumber + " Spawned");
    //        if (_lane.LaneNumber == 1) thebase.pairsReady1 = true;
    //        if (_lane.LaneNumber == 2) thebase.pairsReady2 = true;
    //        if (_lane.LaneNumber == 3) thebase.pairsReady3 = true;

    //        TournamentManager._instance.StartCoroutine (WaitForTickChange (_lane.LaneNumber));
    //        _creature1 = _lane.GetFirstLaneNode (this).activeCreature;
    //        if (_creature1 != null)
    //        {
    //            Debug.Log("PAIRS LANE " + _lane.LaneNumber + " Got Creature1");
    //            thebase.AIResponse.Move (_creature1, _lane.LaneNumber);

    //            TournamentManager._instance.StartCoroutine (SpawnTwo (_lane.LaneNumber,this));

    //            while (spawnTwo == false)
    //            {
    //                yield return null;
    //            }

    //            _creature2 = _lane.GetFirstLaneNode (this).activeCreature;

    //            //--Add to list of pairs--
    //            attackingPairs.Add (new AttackingPair { creature1 = _creature1, creature2 = _creature2 });

    //            if (_lane.LaneNumber == 1) spawningPairs1 = false;
    //            if (_lane.LaneNumber == 2) spawningPairs2 = false;
    //            if (_lane.LaneNumber == 3) spawningPairs3 = false;
    //            yield return null;
    //        }
    //        else
    //        {
    //            Debug.Log("PAIRS LANE " + _lane.LaneNumber + " Could Not Find Creature1");

    //            if (_lane.LaneNumber == 1) spawningPairs1 = false;
    //            if (_lane.LaneNumber == 2) spawningPairs2 = false;
    //            if (_lane.LaneNumber == 3) spawningPairs3 = false;
    //            // SpawnNewPair (_lane);
    //        }
    //    }
    //}

    //bool SpawnOne (Spawnable type, int lane)
    //{
    //    if (AIResponse.Spawn (type, lane))
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //IEnumerator SpawnTwo (int laneNumber,Kittyv2 thebase)
    //{
    //    //--Spawn Second Creature on second tick--
    //    if (!thebase.AIResponse.Spawn (Spawnable.Unicorn, laneNumber))
    //    {
    //        TournamentManager._instance.StartCoroutine (WaitForTickChange (laneNumber));
    //        while (!tickChanged)
    //        {
    //            yield return null;
    //        }
    //        tickChanged = false;
    //        TournamentManager._instance.StartCoroutine (SpawnTwo (laneNumber,thebase));
    //    }
    //    else
    //    {
    //        if (laneNumber == 1) pairsReady1 = true;
    //        if (laneNumber == 2) pairsReady2 = true;
    //        if (laneNumber == 3) pairsReady3 = true;
    //        TournamentManager._instance.StartCoroutine (WaitForTickChange (laneNumber));
    //        spawnTwo = true;
    //    }
    //}

    //IEnumerator WaitForTickChange (int laneNumber)
    //{
    //    currentTick = alternateTick;
    //    while (alternateTick == currentTick)
    //    {
    //        Debug.Log("WaitForTickChange()");
    //        yield return null;
    //    }
    //    tickChanged = true;
    //    currentTick = alternateTick;
    //    // if (laneNumber == 1) pairsReady1 = false;
    //    // if (laneNumber == 2) pairsReady2 = false;
    //    // if (laneNumber == 3) pairsReady3 = false;
    //}
    #endregion
}

[System.Serializable]
    public struct AttackingPair
    {
        public CreatureBase creature1;
        public CreatureBase creature2;
    }
