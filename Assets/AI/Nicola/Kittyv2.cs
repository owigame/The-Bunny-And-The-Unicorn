using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    LaneManager[] boardState;

    int enemyCount;
    CreatureBase targetCreature, closestEnemy;

    public List<AttackingPair> attackingPairs = new List<AttackingPair> ();
    public override void OnTick (IBoardState[] data)
    {
        if (_Start)
        {
            _Start = false;
            attackingPairs = new List<AttackingPair> ();
        }

        boardState = (LaneManager[]) data;

        //--if no pairs, add empty pairs--

        if (attackingPairs.Count <= 0)
        {
            for (int i = 1; i < 4; i++)
            {
                attackingPairs.Add (new AttackingPair (i, this));
            }
        }

        //--spawn for each pair--
        foreach (var pair in attackingPairs)
        {
            pair.SpawnPair (AIResponse);
        }

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int playerInLane = lane.GetFriendliesInLane (this).Count;
            if (playerInLane <= 0)
            {
                attackingPairs.Add (new AttackingPair (lane.LaneNumber, this));
                foreach (var pair in attackingPairs)
                {
                    if (pair == null) pair.SpawnPair (AIResponse);
                }
            }
        }
        // foreach (var pair in attackingPairs)
        // {
        //     pair.MovePair(AIResponse);
        // }foreach (var pair in attackingPairs)
        // {
        //     pair.AttackAsPair(AIResponse);
        // }

        #region More than 20
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
                    //find pair that contains friendlyToMove
                    foreach (var pair in attackingPairs)
                    {
                        if (pair.Contains (_friendlyToMove))
                        {
                            pair.MovePair (AIResponse);
                            break;
                        }
                    }
                    // AIResponse.Move (_friendlyToMove, _openNodes);
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
                foreach (CreatureBase _enemy in _lane.GetEnemiesInLane (this))
                {
                    //--Move to enemy, attack, then move to end of Lane--    
                    int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                    //find pair that contains friendlyToMove
                    foreach (var pair in attackingPairs)
                    {
                        if (pair.Contains (_friendlyToMove))
                        {
                            if (!pair.MovePair (AIResponse))
                            {
                                pair.AttackAsPair (AIResponse);
                            }
                            pair.AttackAsPair (AIResponse);
                            break;
                        }
                    }

                    // if (!AIResponse.Move (_friendlyToMove, _openNodes))
                    // {
                    //     AIResponse.Attack (_friendlyToMove);
                    // }
                    // AIResponse.Attack (_friendlyToMove);

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
        #endregion
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

                List<CreatureBase> searchTargetCreatures = toMove.ActiveLaneNode.laneManager.SearchRange ((int) toMove.Range, toMove.ActiveLaneNode, this);
                bool foundAttackTarget = false;
                foreach (CreatureBase _creature in searchTargetCreatures)
                {
                    if (_creature.Owner != toMove.Owner)
                    {
                        //Found enemy creature in range
                        foundAttackTarget = true;
                        AIResponse.Attack (toMove);
                        //find pair that contains friendlyToMove
                        foreach (var pair in attackingPairs)
                        {
                            if (pair.Contains (toMove))
                            {
                                pair.AttackAsPair (AIResponse);
                                break;
                            }
                        }
                    }
                }
                if (!foundAttackTarget)
                {
                    int moveSpaces = toMove.ActiveLaneNode.laneManager.GetOpenNodes (toMove.ActiveLaneNode, _RightFacing);
                    if (moveSpaces > AIResponse.Tokens)
                    {
                        moveSpaces = AIResponse.Tokens;
                    }
                    foreach (var pair in attackingPairs)
                    {
                        if (pair.Contains (toMove))
                        {
                            pair.MovePair (AIResponse);
                            break;
                        }
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
                // AIResponse.Attack (closestFriendly);
                foreach (var pair in attackingPairs)
                {
                    if (pair.Contains (closestFriendly))
                    {
                        pair.AttackAsPair (AIResponse);
                        break;
                    }
                }
            }
            //--Else move...
            else
            {
                //AIResponse.Move (closestFriendly, openNodes);
                foreach (var pair in attackingPairs)
                {
                    if (pair.Contains (closestFriendly))
                    {
                        pair.MovePair (AIResponse);
                        break;
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class AttackingPair
{
    public AttackingPair (int thelane, LogicBase Owner)
    {
        Debug.Log ("-----Create Pair-----");
        Lane = thelane;
        owner = Owner;
    }
    LogicBase owner;
    public int Lane;
    public int spawnProg = 0;
    public CreatureBase[] creatures = new CreatureBase[2] { null, null };

    public Spawnable[] pairTypes = new Spawnable[2] { Spawnable.Bunny, Spawnable.Unicorn };

    public void SpawnPair (AIResponseManager responseManager)
    {
        bool success = false;

        Debug.Log ("----SpawnPair--------");
        if (spawnProg == 0)
        {
            success = responseManager.Spawn (pairTypes[0], Lane);
        }

        if (spawnProg == 1)
        {
            creatures[0] = TournamentManager._instance.lanes[Lane - 1].GetFirstLaneNode (owner).activeCreature;
            success = responseManager.Move (creatures[0]);
        }

        Debug.Log ("2nd Creature: " + creatures[1]);
        if (spawnProg == 2)
        {
            success = responseManager.Spawn (pairTypes[1], Lane);
        }

        if (spawnProg == 3)
        {
            creatures[1] = TournamentManager._instance.lanes[Lane - 1].GetFirstLaneNode (owner).activeCreature;
        }

        if (success) spawnProg++;
    }
    public bool MovePair (AIResponseManager responseManager)
    {
        Debug.Log ("-----MOVE PAIR----");
        if (responseManager.Tokens > 2)
            return false;

        bool temp = true;
        if (creatures[0] != null)
            if (!responseManager.Move (creatures[0]))
                temp = false;

        if (creatures[1] != null)
            if (!responseManager.Move (creatures[1]))
                temp = false;
        return temp;
    }
    public bool AttackAsPair (AIResponseManager responseManager)
    {
        Debug.Log ("------ATTACK PAIR----");
        if (creatures[0] == null && creatures[1] == null)
            return false;
        if (creatures[0] != null)
        {
            if (!responseManager.Attack (creatures[0], creatures[0].CreatureType == Spawnable.Bunny ? 1 : 3))
            {
                if (creatures[1] != null)
                    return responseManager.Attack (creatures[1], creatures[1].CreatureType == Spawnable.Bunny ? 1 : 3);
            }
        }
        else if (creatures[1] != null)
            return responseManager.Attack (creatures[1], creatures[1].CreatureType == Spawnable.Bunny ? 1 : 3);

        return false;
    }
    public bool Contains (CreatureBase creaturetoTest)
    {
        for (int i = 0; i < creatures.Length; i++)
        {
            if (creatures[i] == creaturetoTest)
                return true;
        }
        return false;
    }
}