using System.Collections.Generic;
using UnityEngine;

namespace AI.DanR
{
    [CreateAssetMenu(fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
    public class Hyena : LogicBase
    {
        private Spawnable _lastSpawned;
        private int _lanetospawn = 1;
        private bool set = false;

        private List<CreatureBase> friendliesWithEnemiesInRange;

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

                    List<CreatureBase> friendliesInLane1, friendliesInLane2, friendliesInLane3;

                    FindAllFriendliesInLane(out friendliesInLane1, out friendliesInLane2, out friendliesInLane3);

                    FindAllEnemiesInlanes();

                    FriendliesThatcanAttack(friendliesInLane1, friendliesInLane2, friendliesInLane3);


                    //TEMP
                    List<CreatureBase> friendlies = _Creatures;
                    MoveOrAttack(friendlies);


                    // Find if friendly is close to the end


                    // If no Target in range && freindly not close to end : pick a lane with the least amount of enemies to move in.

                    // If Target in range && friendly not close to end : Attack the closest enemy in the lane with the most enemies.

                    // If friend close to end && no target in range : Move forward

                    maxCycles--;
                }
            }

            AIResponse.FinalizeResponse();
        }

        private void Initialize()
        {
            if (!set)
            {
                _lanetospawn = 1;
                set = true;
            }
            
        }

        private void FindAllFriendliesInLane(out List<CreatureBase> friendliesInLane1, out List<CreatureBase> friendliesInLane2, out List<CreatureBase> friendliesInLane3)
        {
            // Find Friendlies in lane for all lanes and store each as its own list : RETURN A LIST
            friendliesInLane1 = TournamentManager._instance.lanes[0].GetFriendliesInLane(this);
            friendliesInLane2 = TournamentManager._instance.lanes[1].GetFriendliesInLane(this);
            friendliesInLane3 = TournamentManager._instance.lanes[2].GetFriendliesInLane(this);
        }

        private void FindAllEnemiesInlanes()
        {
            // Find Enemies in lane for all lanes and store each as its own list : RETURN A LIST
            var enemiesInLane1 = TournamentManager._instance.lanes[0].GetEnemiesInLane(this);
            var enemiesInLane2 = TournamentManager._instance.lanes[1].GetEnemiesInLane(this);
            var enemiesInLane3 = TournamentManager._instance.lanes[2].GetEnemiesInLane(this);
        }

        private void FriendliesThatcanAttack(List<CreatureBase> friendliesInLane1, List<CreatureBase> friendliesInLane2, List<CreatureBase> friendliesInLane3)
        {
            // Find  enemies in attacking range : RETURN A LIST
            if (friendliesInLane1.Count > 0)
            {
                foreach (CreatureBase creature in friendliesInLane1)
                {
                    if (creature != null && creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this).Count > 0)
                    {
                        friendliesWithEnemiesInRange.Add(creature);
                    }
                }
            }

            if (friendliesInLane2.Count > 0)
            {
                foreach (CreatureBase creature in friendliesInLane2)
                {
                    if (creature != null && creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this).Count > 0)
                    {
                        friendliesWithEnemiesInRange.Add(creature);
                    }
                }
            }

            if (friendliesInLane3.Count > 0)
            {
                foreach (CreatureBase creature in friendliesInLane3)
                {
                    if (creature != null && creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this).Count > 0)
                    {
                        friendliesWithEnemiesInRange.Add(creature);
                    }
                }
            }
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

        CreatureBase closestCreature;

        public void MoveOrAttack(List<CreatureBase> friendlyCreatures)
        {
            if(friendliesWithEnemiesInRange != null && friendliesWithEnemiesInRange.Count > 0)
            {
                AIResponse.Attack(friendliesWithEnemiesInRange[0]);
            }
            else
            {
                foreach(CreatureBase creature in friendlyCreatures)
                {
                    if(closestCreature == null)
                    {
                        closestCreature = creature;
                    }
                    if(creature.LaneProgress > closestCreature.LaneProgress && closestCreature != null)
                    {
                        closestCreature = creature;
                    }                   
                }
               
                if(closestCreature != null)
                {
                    Debug.Log(closestCreature);
                    AIResponse.Move(closestCreature);
                }
                
            }
        }
    }

}