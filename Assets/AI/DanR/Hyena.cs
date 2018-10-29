using System.Collections.Generic;
using UnityEngine;

namespace AI.DanR
{
    [CreateAssetMenu (fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
    public class Hyena : LogicBase
    {
        private Spawnable _lastSpawned;
        private int _lanetospawn = 1;
        private bool set = false;

        private void Initialize ()
        {
            if (!set)
            {
                _lanetospawn = 1;
                set = true;
            }
        }

        public override void OnTick (IBoardState[] data)
        {
            if (!AIResponse.Spawn (Spawnable.Unicorn, 1))
            {
                AIResponse.onTick (null);

                var maxCycles = 99;

                while (AIResponse.Tokens > 0 && maxCycles > 0)
                {
                    Initialize ();

                    SpawnEnemy ();

                    List<CreatureBase> lane1Frienlies = FindAllFriendlies (TournamentManager._instance.lanes[0]);
                    List<CreatureBase> lane2Frienlies = FindAllFriendlies (TournamentManager._instance.lanes[1]);
                    List<CreatureBase> lane3Frienlies = FindAllFriendlies (TournamentManager._instance.lanes[2]);

                    CheckIfClosestenemyIsInAttackingrange (FindFrendlyWithClosestEnemies (lane1Frienlies, lane2Frienlies, lane3Frienlies));

                    maxCycles--;
                }
            }

            AIResponse.FinalizeResponse ();
        }

        private void SpawnEnemy ()
        {

            // Round robin spawning
            if (_lanetospawn > 3)
                _lanetospawn = 1;

            if (_lastSpawned == Spawnable.Unicorn)
            {
                if (!AIResponse.Spawn (Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }
            else if (_lastSpawned == Spawnable.Bunny)
            {
                if (!AIResponse.Spawn (Spawnable.Unicorn, _lanetospawn))
                    _lastSpawned = Spawnable.Unicorn;
            }
            else
            {
                if (!AIResponse.Spawn (Spawnable.Bunny, _lanetospawn))
                    _lastSpawned = Spawnable.Bunny;
            }

            _lanetospawn++;
        }

        private List<CreatureBase> FindAllFriendlies (LaneManager lane)
        {
            return lane.GetFriendliesInLane (this);
        }

        private CreatureBase FindFrendlyWithClosestEnemies (List<CreatureBase> lane1Frienlies, List<CreatureBase> lane2Frienlies, List<CreatureBase> lane3Frienlies)
        {
            int nearestDistance = 100;
            CreatureBase allyWithEnemyClostest = null;

            foreach (CreatureBase creature in lane1Frienlies)
            {
                if (GetNearestEnemyNodesAwayFrom (creature) < nearestDistance)
                {
                    nearestDistance = GetNearestEnemyNodesAwayFrom (creature);
                    allyWithEnemyClostest = creature;
                }
            }

            foreach (CreatureBase creature in lane2Frienlies)
            {
                if (GetNearestEnemyNodesAwayFrom (creature) < nearestDistance)
                {
                    nearestDistance = GetNearestEnemyNodesAwayFrom (creature);
                    allyWithEnemyClostest = creature;
                }
            }

            foreach (CreatureBase creature in lane3Frienlies)
            {
                if (GetNearestEnemyNodesAwayFrom (creature) < nearestDistance)
                {
                    nearestDistance = GetNearestEnemyNodesAwayFrom (creature);
                    allyWithEnemyClostest = creature;
                }
            }

            return allyWithEnemyClostest;
        }

        private void CheckIfClosestenemyIsInAttackingrange (CreatureBase AllyWithClosestEnemy)
        {
            if (AllyWithClosestEnemy == null) return;

            if (GetNearestEnemyNodesAwayFrom (AllyWithClosestEnemy) <= AllyWithClosestEnemy.Range)
            {
                Debug.Log ("Attack");

                if (!AIResponse.Attack (AllyWithClosestEnemy))
                {
                    AIResponse.Move (AllyWithClosestEnemy);
                }

            }

        }
    }

}
