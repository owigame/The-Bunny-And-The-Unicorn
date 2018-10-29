/*
This code runs extreamly slow in the method FindFrendlyWithClosestEnemies need an alternative thos this method.
*/


using System.Collections.Generic;
using UnityEngine;

namespace AI.DanR
{
    [CreateAssetMenu(fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
    public class Hyena : LogicBase
    {
        private Spawnable _lastSpawned;
        private int _lanetospawn = 1;
        private bool _set;


        private int _lanetocheck = 1;

        private void Initialize()
        {
            if (!_set)
            {
                _lanetospawn = 1;
                _set = true;
            }
        }

        public override void OnTick(IBoardState[] data)
        {
            if (!AIResponse.Spawn(Spawnable.Unicorn, 1))
            {
                AIResponse.onTick(null);

                var maxCycles = 99;

                while (AIResponse.Tokens > 0 && maxCycles > 0)
                {
                    Initialize();

                    SpawnEnemy();

                    var lane1Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[0]);
                    var lane2Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[1]);
                    var lane3Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[2]);


                    CheckIfClosestenemyIsInAttackingrange(FindFrendlyWithClosestEnemies(lane1Frienlies, lane2Frienlies,
                        lane3Frienlies));

                    maxCycles--;
                }
            }

            AIResponse.FinalizeResponse();
        }

        private void SpawnEnemy()
        {
            // Round robin spawning
            if (_lanetospawn > 3)
                _lanetospawn = 1;

            if (_lastSpawned == Spawnable.Unicorn)
            {
                if (!AIResponse.Spawn(Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }
            else if (_lastSpawned == Spawnable.Bunny)
            {
                if (!AIResponse.Spawn(Spawnable.Unicorn, _lanetospawn))
                    _lastSpawned = Spawnable.Unicorn;
            }
            else
            {
                if (!AIResponse.Spawn(Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }

            _lanetospawn++;
        }

        private List<CreatureBase> FindAllFriendlies(LaneManager lane)
        {
            return lane.GetFriendliesInLane(this);
        }

        private CreatureBase FindFrendlyWithClosestEnemies(List<CreatureBase> lane1Frienlies,
            List<CreatureBase> lane2Frienlies, List<CreatureBase> lane3Frienlies)
        {
            var nearestDistance = 100;

            CreatureBase allyWithEnemyClostest = null;

            if (_lanetocheck == 1)
            {
                foreach (var creature in lane1Frienlies)
                {
                    if (GetNearestEnemyNodesAwayFrom(creature) >= nearestDistance) continue;

                    nearestDistance = GetNearestEnemyNodesAwayFrom(creature);
                    allyWithEnemyClostest = creature;
                    //  Debug.Log("Lane1");
                }
            }
            else if (_lanetocheck == 2)
            {
                foreach (var creature in lane2Frienlies)
                {
                    if (GetNearestEnemyNodesAwayFrom(creature) < nearestDistance)
                    {
                        nearestDistance = GetNearestEnemyNodesAwayFrom(creature);
                        allyWithEnemyClostest = creature;
                        //  Debug.Log("Lane2");
                    }
                }
            }
            else if (_lanetocheck == 3)
            {
                foreach (var creature in lane3Frienlies)
                {
                    if (GetNearestEnemyNodesAwayFrom(creature) >= nearestDistance) continue;

                    nearestDistance = GetNearestEnemyNodesAwayFrom(creature);
                    allyWithEnemyClostest = creature;
                    Debug.Log("Lane3");
                }
            }

            _lanetocheck++;

            if (_lanetocheck >= 3)
                _lanetocheck = 1;

            return allyWithEnemyClostest;
        }

        private void CheckIfClosestenemyIsInAttackingrange(CreatureBase AllyWithClosestEnemy)
        {
            if (AllyWithClosestEnemy == null) return;

            if (GetNearestEnemyNodesAwayFrom(AllyWithClosestEnemy) <= AllyWithClosestEnemy.Range)
            {
                Debug.Log("Attack");

                if (!AIResponse.Attack(AllyWithClosestEnemy))
                {
                    AIResponse.Move(AllyWithClosestEnemy);
                }
            }
        }
    }
}

/*
 *
 *
 *
 *
 *
 *
 *
 *
 *
 * 
 */


namespace AI.DanR
{
    [CreateAssetMenu(fileName = "Hyena_Test", menuName = "AI/Hyena_Test", order = 0)]
    public class Hyena2 : LogicBase
    {
        private Spawnable _lastSpawned;
        private int _lanetospawn = 1;
        private bool _hasBeenInitialized;


        private int _lanetocheck = 1;

        private void Initialize()
        {
            if (_hasBeenInitialized) return;

            _lanetospawn = 1;
            _hasBeenInitialized = true;
        }

        
        public override void OnTick(IBoardState[] data)
        {
            if (!AIResponse.Spawn(Spawnable.Unicorn, 1))
            {
                AIResponse.onTick(null);

                var maxCycles = 99;

                while (AIResponse.Tokens > 0 && maxCycles > 0)
                {
                    Initialize();

                    SpawnEnemy();

                    var lane1Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[0]);
                    var lane2Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[1]);
                    var lane3Frienlies = FindAllFriendlies(TournamentManager._instance.lanes[2]);


                    var farthsetAlong = 0;
                    CreatureBase furthestCreature = null;

                    if (_lanetospawn == 1)
                    {
                        foreach (var frienly in lane1Frienlies)
                        {
                            if (frienly.LaneProgress >= farthsetAlong || furthestCreature == null)
                            {
                                farthsetAlong = frienly.LaneProgress;
                                furthestCreature = frienly;
                            }
                        }

                        CheckIfClosestenemyIsInAttackingrange(furthestCreature);
                    }
                    else if(_lanetospawn == 2)
                    {
                        foreach (var frienly in lane2Frienlies)
                        {
                            if (frienly.LaneProgress >= farthsetAlong || furthestCreature == null)
                            {
                                farthsetAlong = frienly.LaneProgress;
                                furthestCreature = frienly;
                            }
                        }

                        CheckIfClosestenemyIsInAttackingrange(furthestCreature);
                    }
                    else 
                    {
                        foreach (var frienly in lane3Frienlies)
                        {
                            if (frienly.LaneProgress >= farthsetAlong || furthestCreature == null)
                            {
                                farthsetAlong = frienly.LaneProgress;
                                furthestCreature = frienly;
                            }
                        }

                        CheckIfClosestenemyIsInAttackingrange(furthestCreature);
                    }



                    maxCycles--;
                }
            }

            AIResponse.FinalizeResponse();
        }

        private void SpawnEnemy()
        {
            // Round robin spawning
            if (_lanetospawn > 3)
                _lanetospawn = 1;

            if (_lastSpawned == Spawnable.Unicorn)
            {
                if (!AIResponse.Spawn(Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }
            else if (_lastSpawned == Spawnable.Bunny)
            {
                if (!AIResponse.Spawn(Spawnable.Unicorn, _lanetospawn))
                    _lastSpawned = Spawnable.Unicorn;
            }
            else
            {
                if (!AIResponse.Spawn(Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }

            _lanetospawn++;
        }

        private List<CreatureBase> FindAllFriendlies(LaneManager lane)
        {
            return lane.GetFriendliesInLane(this);
        }

        CreatureBase _hitEnemy;

        private void CheckIfClosestenemyIsInAttackingrange(CreatureBase allyWithMostProgress)
        {
            if (allyWithMostProgress == null) return;

            List<CreatureBase> searchTargetCreatures;
            searchTargetCreatures = allyWithMostProgress.ActiveLaneNode.laneManager.SearchRange(
                (int) allyWithMostProgress.Range,
                allyWithMostProgress.ActiveLaneNode, this);

            var foundAttackTarget = false;

            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                if (_creature.Owner != allyWithMostProgress.Owner)
                {
                    //Found enemy creature in range
                    foundAttackTarget = true;
                    AIResponse.Attack(allyWithMostProgress);
                }
            }


            if (!foundAttackTarget)
            {
                int moveSpaces =
                    allyWithMostProgress.ActiveLaneNode.laneManager.GetOpenNodes(allyWithMostProgress.ActiveLaneNode,
                        _RightFacing);
                if (moveSpaces > AIResponse.Tokens)
                {
                    moveSpaces = AIResponse.Tokens;
                }

                if (AIResponse.Move(allyWithMostProgress, moveSpaces))
                {
                }
            }

            /*
            if (allyWithMostProgress == null) return;

            Debug.Log(allyWithMostProgress.Range);
            
            RaycastHit[] hits;

            hits = Physics.RaycastAll(allyWithMostProgress.gameObject.transform.position,
                allyWithMostProgress.gameObject.transform.forward, allyWithMostProgress.Range + 3);


            foreach (var variable in hits)
            {
                _hitEnemy = variable.collider.gameObject.GetComponent<CreatureBase>();
                Debug.Log("Did hit");
            }

            if (_hitEnemy == null) return;

            if (_hitEnemy.Owner != this)
            {
                Debug.Log("Here");

                if (AIResponse.Attack(allyWithMostProgress, 1) == false)
                {
                    AIResponse.Move(allyWithMostProgress, 2);
                }                
            }
            
            */
        }
    }
}
