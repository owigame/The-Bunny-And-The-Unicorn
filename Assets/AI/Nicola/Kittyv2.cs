using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

/*
    -------LOGIC--------

    Spawn, Attack, and Move in pairs of a bunny and a unicorn.
    Have one pair on each lane at all times.

    Progress in lanes:
        - Look for lane with most progressed enemies.
        - Look for open lane.
        - Look for lane with least amount of enemies.

 */

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
            //---Add empty pairs at start---
            for (int i = 1; i < 4; i++)
            {
                attackingPairs.Add (new AttackingPair (i, this));
            }
        }

        boardState = (LaneManager[]) data;

        //---spawn for each pair---
        foreach (var pair in attackingPairs)
        {
            pair.SpawnPair (AIResponse);
        }

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int playerInLane = lane.GetFriendliesInLane (this).Count;
            if (playerInLane <= 0)
            {
                foreach (var pair in attackingPairs)
                {
                    if (pair.spawnProg == 0)
                    {
                        pair.SpawnPair (AIResponse);
                    }
                }
            }
        }
        

        #region More than 20
        if (AIResponse.Tokens >= 20)
        {
            LaneManager _lane = null;
            CreatureBase _friendlyToMove = null;
            int previousEnemies = 0;
            bool _moveAttack = true;

            //---check each lane---
            foreach (LaneManager lane in TournamentManager._instance.lanes)
            {
                int playerInLane = lane.GetFriendliesInLane (this).Count;
                int enemyInLane = lane.GetEnemiesInLane (this).Count;
                //---Find Open lane---
                if (enemyInLane == 0 && playerInLane > 0)
                {
                    _lane = lane;
                    _friendlyToMove = _lane.GetFriendliesInLane (this) [0];
                    //---move the creature to end of lane---
                    int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                    //---find pair that contains friendlyToMove---
                    foreach (var pair in attackingPairs)
                    {
                        if (pair.ContainsCreature (_friendlyToMove))
                        {
                            pair.MovePair (AIResponse);
                            break;
                        }
                    }
                    _moveAttack = false;
                    break;
                }
                //---ELSE Find lane with least amount of enemies---
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

            Debug.Log ("Friendly to move: " + _friendlyToMove);
            Debug.Log ("MoveAttack :" + _moveAttack);
            if (_moveAttack && _friendlyToMove != null)
            {
                //---attemp movetowards and attack each enemy in lane, then move to end of lane---
                foreach (CreatureBase _enemy in _lane.GetEnemiesInLane (this))
                {
                    //---Move to enemy, attack, then move to end of Lane---    
                    int _openNodes = _friendlyToMove.ActiveLaneNode.laneManager.GetOpenNodes (_friendlyToMove.ActiveLaneNode, _RightFacing);
                    //---find pair that contains friendlyToMove---
                    foreach (var pair in attackingPairs)
                    {
                        if (pair.ContainsCreature (_friendlyToMove))
                        {
                            if (!pair.MovePair (AIResponse))
                            {
                                AttemptMoveAttack ();
                            }
                            break;
                        }
                    }
                }
            }
        }
        #endregion
        else
        {

            //---Get Enemy Count---
            GetCount ();

            //---If there are Enemies, find the closest---
            if (enemyCount > 0)
            {
                FindNearest ();
                //---if neareast found, move towards and attack---
                if (targetCreature != null)
                {
                    AttemptAttackMoveTarget (targetCreature);
                }
            }
            //--else random move or attack--
            else
            {
                AttemptMoveAttack ();
            }
        }
        AIResponse.FinalizeResponse ();

    }

    //---Get total enemies in play---
    void GetCount ()
    {
        //---reset count each round...
        enemyCount = 0;

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            enemyCount += lane.GetEnemiesInLane (this).Count;
        }
    }

    //---Find enemy nearest player---
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
        Debug.Log ("---Attempt Move/Attack---");
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
                        //---Found enemy creature in rang---
                        foundAttackTarget = true;
                        AIResponse.Attack (toMove);
                        //---find pair that contains friendlyToMove---
                        foreach (var pair in attackingPairs)
                        {
                            if (pair.ContainsCreature (toMove))
                            {
                                //---if first creature still exists---
                                if (pair.creatures[0] != null)
                                {
                                    pair.AttackAsPair (AIResponse);
                                }
                                else
                                {
                                    pair.AttackSingle(pair.creatures[1], AIResponse);
                                }
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
                        if (pair.ContainsCreature (toMove))
                        {
                            //---if first creature still exists---
                            if (pair.creatures[0] != null)
                            {
                                pair.MovePair (AIResponse);
                            }
                            else
                            {
                                pair.MoveSingle(pair.creatures[1], AIResponse, moveSpaces);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    void AttemptAttackMoveTarget (CreatureBase creature)
    {
        Debug.Log ("---Attempt Target---");

        //---Find friendly in same lane as _targetCreature---
        List<CreatureBase> friendlies = creature.ActiveLaneNode.laneManager.GetFriendliesInLane (this);
        if (friendlies.Count > 0)
        {
            CreatureBase closestFriendly = friendlies[0];
            //--Attempt to attack...
            int openNodes = closestFriendly.ActiveLaneNode.laneManager.GetOpenNodes (closestFriendly.ActiveLaneNode, _RightFacing);
            //--is enemy in attack range? Attack...
            if (openNodes < closestFriendly.Range)
            {
                // AIResponse.Attack (closestFriendly);
                foreach (var pair in attackingPairs)
                {
                    if (pair.ContainsCreature (closestFriendly))
                    {
                        //---if first creature still exists---
                        if (pair.creatures[0] != null)
                        {
                            if (!pair.AttackAsPair (AIResponse))
                            {
                                pair.MovePair (AIResponse);
                            }
                        }
                        else
                        {
                            //---------attempt to attack with second creature-----------
                            if (!pair.AttackSingle(pair.creatures[1], AIResponse))
                            {
                                //--------else attempt to move second creature------------
                                pair.MoveSingle(pair.creatures[1], AIResponse, openNodes);
                            }
                        }
                        break;
                    }
                }
            }
            //--Else move...
            else
            {
                foreach (var pair in attackingPairs)
                {
                    if (pair.ContainsCreature (closestFriendly))
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
        //  Debug.Log ("-----Create Pair-----");
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
            if (creatures[0] != null)
            {
                success = responseManager.Move (creatures[0]);
            }
        }

        if (spawnProg == 2)
        {
            success = responseManager.Spawn (pairTypes[1], Lane);
        }

        if (spawnProg == 3)
        {
            creatures[1] = TournamentManager._instance.lanes[Lane - 1].GetFirstLaneNode (owner).activeCreature;
            success = true;
        }

        if (spawnProg == 4)
        {
            if (creatures[0] == null || creatures[0].isDead)
            {
                spawnProg = 0;
                SpawnPair (responseManager);
            }
            if (creatures[1] == null || creatures[1].isDead)
            {
                spawnProg = 1;
                SpawnPair (responseManager);
            }
        }

        if (success) spawnProg++;
    }
    public bool MovePair (AIResponseManager responseManager)
    {
        Debug.Log ("-----MOVE PAIR----");
        if (responseManager.Tokens < 2)
        {
            return false;
        }

        bool temp = true;

        if (creatures[0] != null)
        {
            Debug.Log ("creature 1: " + creatures[0]);
            if (!responseManager.Move (creatures[0]))
            {
                temp = false;
            }
        }

        if (creatures[1] != null)
        {
            Debug.Log ("creature 2: " + creatures[1]);
            if (!responseManager.Move (creatures[1]))
            {
                temp = false;
            }
        }
        return temp;
    }
    public bool AttackAsPair (AIResponseManager responseManager)
    {
        Debug.Log ("------ATTACK PAIR----");
        if (creatures[0] == null && creatures[1] == null)
        {
            return false;
        }

        bool validAttack = false;
        if (creatures[0] != null)
        {
            if (!responseManager.Attack (creatures[0], 1))
            {
                responseManager.Move (creatures[0]);
            }
            else
            {
                validAttack = true;
            }
        }
        if (creatures[1] != null)
        {
            if (!responseManager.Attack (creatures[1], 1))
            {
                responseManager.Move (creatures[1]);
            }
            else
            {
                validAttack = true;
            }
        }

        return validAttack;
    }

    public bool AttackSingle(CreatureBase _creature, AIResponseManager responseManager)
    {
        if (responseManager.Attack(creatures[1], 1))
        {
            return true;           
        }
        else
        {
            return false;
        }
    }

    public bool MoveSingle(CreatureBase _creature, AIResponseManager responseManager, int _Range)
    {
        if (responseManager.Move(creatures[1], _Range))
        {
            return true;           
        }
        else
        {
            return false;
        }
    }

    public bool ContainsCreature (CreatureBase creaturetoTest)
    {
        for (int i = 0; i < creatures.Length; i++)
        {
            if (creatures[i] == creaturetoTest)
            {
                return true;
            }
        }
        return false;
    }
}