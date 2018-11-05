/*
Best Version so far
*/


using System.Collections.Generic;
using UnityEngine;

namespace AI.DanR
{
    namespace AI.DanR
    {
        [CreateAssetMenu(fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
        public class Hyena : LogicBase
        {
            private Spawnable _lastSpawned;
            private int _lanetospawn = 1;
            private bool _hasBeenInitialized;


            private int _lanetocheck = 1;

            private void Initialize()
            {
                if (!_Start) return;

                _lanetospawn = 1;
                _Start = false;
                friendlyBases = new List<CreatureBase>();
            }


            List<CreatureBase> friendlyBases = new List<CreatureBase>();

            public override void OnTick(IBoardState[] data)
            {
                AIResponse.onTick(null);

                var maxCycles = 99;
                Initialize();

                while (AIResponse.Tokens > 0 && maxCycles > 0)
                {

//                    if (CheckIfAlliesAreInAttackingDistance())
//                    {
//                        continue;
//                    }
                    
                    
                    
                    
                    SpawnEnemy();


                    CreatureBase FarthestLane1Ally = null;
                    CreatureBase FarthestLane2Ally = null;
                    CreatureBase FarthestLane3Ally = null;

                    if (TournamentManager._instance.lanes[0].GetFriendliesInLane(this).Count > 0)
                    {
                        FarthestLane1Ally = TournamentManager._instance.lanes[0].GetFriendliesInLane(this)[0];
                        if (FarthestLane1Ally != null && !friendlyBases.Contains(FarthestLane1Ally))
                            friendlyBases.Add(FarthestLane1Ally);
                    }

                    if (TournamentManager._instance.lanes[1].GetFriendliesInLane(this).Count > 0)
                    {
                        FarthestLane2Ally = TournamentManager._instance.lanes[1].GetFriendliesInLane(this)[0];
                        if (FarthestLane2Ally != null && !friendlyBases.Contains(FarthestLane2Ally))
                            friendlyBases.Add(FarthestLane2Ally);
                    }

                    if (TournamentManager._instance.lanes[2].GetFriendliesInLane(this).Count > 0)
                    {
                        FarthestLane3Ally = TournamentManager._instance.lanes[2].GetFriendliesInLane(this)[0];
                        if (FarthestLane3Ally != null && !friendlyBases.Contains(FarthestLane3Ally))
                            friendlyBases.Add(FarthestLane3Ally);
                    }


                    var farthsetAlong = 0;
                    CreatureBase withMostProgress = null;


                    foreach (var creatureBase in friendlyBases)
                    {
                        if (creatureBase != null && creatureBase.LaneProgress > farthsetAlong ||
                            withMostProgress.isDead)
                        {
                            farthsetAlong = creatureBase.LaneProgress;

                            withMostProgress = creatureBase;
                        }
                    }

                    Debug.Log("Further: " +
                              (withMostProgress != null ? withMostProgress.GetInstanceID().ToString() : " NULL"));


                    CreatureBase canFinish = null;

                    foreach (var creature in _Creatures)
                    {
                        int remaining = creature.LaneProgress - 10;

                        if (remaining <= 10)
                        {
                            canFinish = creature;
                        }
                    }


                    if (CheckIfCreatureCanFinish(canFinish))
                    {
                        Debug.Log("Finish");
                    }
                    else
                    {
                        CheckIfClosestenemyIsInAttackingrange(withMostProgress);
                    }
                    


                    maxCycles--;
                }


                AIResponse.FinalizeResponse();
            }

            private bool CheckIfAlliesAreInAttackingDistance()
            {
                foreach (var creature in _Creatures)
                {
                    if (creature.ActiveLaneNode.laneManager.SearchRange(0, creature.ActiveLaneNode, this) != null && creature != null)
                    {
                        AIResponse.Attack(creature);
                        return true;
                    }                
                }

                return false;
            }

            private void SpawnEnemy()
            {
                // Round robin spawning
                if (_lanetospawn > 3)
                    _lanetospawn = 1;

                if (_lastSpawned == Spawnable.Unicorn)
                {
                    if (AIResponse.Spawn(Spawnable.Bunny, _lanetospawn))
                        _lastSpawned = Spawnable.Bunny;
                }
                else if (_lastSpawned == Spawnable.Bunny)
                {
                    if (AIResponse.Spawn(Spawnable.Unicorn, _lanetospawn))
                        _lastSpawned = Spawnable.Unicorn;
                }

                _lanetospawn++;
            }


            private bool CheckIfCreatureCanFinish(CreatureBase creature)
            {
                if (creature == null) return false;

                int remaining = creature.LaneProgress - 10;

                if (remaining > AIResponse.Tokens)
                {
                    if (AIResponse.Move(creature, remaining))
                    {
                        return true;
                    }
                }

                return false;
            }

            private CreatureBase _hitEnemy;

            private void CheckIfClosestenemyIsInAttackingrange(CreatureBase allyWithMostProgress)
            {
                if (allyWithMostProgress == null) return;

                List<CreatureBase> searchTargetCreatures;
                searchTargetCreatures = allyWithMostProgress.ActiveLaneNode.laneManager.SearchRange(
                    (int) allyWithMostProgress.Range,
                    allyWithMostProgress.ActiveLaneNode, this);

                var hasTraget = false;

                foreach (CreatureBase creature in searchTargetCreatures)
                {
                    if (creature.Owner != allyWithMostProgress.Owner)
                    {
                        //Found enemy creature in range
                        hasTraget = true;
                        AIResponse.Attack(allyWithMostProgress);
                    }
                }


                if (hasTraget) return;

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
                else if(AIResponse.Move(_Creatures[_Creatures.Count -1], 2))
                {
                    
                }
            }
        }
    }
}
