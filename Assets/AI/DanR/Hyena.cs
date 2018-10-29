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
                    if (FarthestLane1Ally != null && !friendlyBases.Contains(FarthestLane2Ally))
                        friendlyBases.Add(FarthestLane2Ally);
                }

                if (TournamentManager._instance.lanes[2].GetFriendliesInLane(this).Count > 0)
                {
                    FarthestLane3Ally = TournamentManager._instance.lanes[2].GetFriendliesInLane(this)[0];
                    if (FarthestLane1Ally != null && !friendlyBases.Contains(FarthestLane3Ally))
                        friendlyBases.Add(FarthestLane3Ally);
                }


                var farthsetAlong = 0;
                CreatureBase withMostProgress = null;


                foreach (var creatureBase in friendlyBases)
                {
                    if (creatureBase != null && creatureBase.LaneProgress > farthsetAlong || withMostProgress.isDead)
                    {
                        farthsetAlong = creatureBase.LaneProgress;

                        withMostProgress = creatureBase;
                    }
                }
                Debug.Log("Further: " + (withMostProgress != null ? withMostProgress.GetInstanceID().ToString() : " NULL"));


                CheckIfClosestenemyIsInAttackingrange(withMostProgress);


                maxCycles--;
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

        private List<CreatureBase> FindAllFriendlies(LaneManager lane)
        {
            return lane.GetFriendliesInLane(this);
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
        }
    }
}
