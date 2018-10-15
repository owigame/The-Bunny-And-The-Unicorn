using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public abstract class LogicBase : ScriptableObject {
        private AIResponseManager _AIResponder;
        public List<CreatureBase> _Creatures = new List<CreatureBase> ();
        public int _PlayerNumber = 0;
        public bool _RightFacing = false;

        public abstract void OnTick (IBoardState[] Board);
        // public abstract void OnValidateFail(IBoardState data,IResponse[] chain);

        protected AIResponseManager AIResponse {
            get {
                return _AIResponder;
            }
        }

        public void init () {
            _Creatures.Clear ();
            if (this is Human) {
                (this as Human).Start ();
            }

            _PlayerNumber = TournamentManager._instance.P1 == this ? 1 : 2;
            _RightFacing = _PlayerNumber == 1;

            _AIResponder = new AIResponseManager (this);
            TournamentManager._instance.OnTick.AddListener (_AIResponder.onTick);
        }

        public CreatureBase GetNearestEnemy (LaneManager[] Board) {
            CreatureBase[] NearestEnemies = GetNearestEnemies (Board);
            if (NearestEnemies.Length > 0) {
                LogStack.Log ("GetNearestEnemy()" + NearestEnemies[0], Logging.LogLevel.Color);
                return NearestEnemies[0];
            } else {
                LogStack.Log ("GetNearestEnemy()" + null, Logging.LogLevel.Color);
                return null;
            }
        }
        public CreatureBase GetNearestEnemyTo (CreatureBase myCreature) {
            List<CreatureBase> enemyCreatures = myCreature.ActiveLaneNode.laneManager.GetEnemiesInLane (this);
            if (enemyCreatures.Count > 0) {
                LogStack.Log ("GetNearestEnemy()" + enemyCreatures[0], Logging.LogLevel.Color);
                return enemyCreatures[0];
            } else {
                LogStack.Log ("GetNearestEnemy()" + null, Logging.LogLevel.Color);
                return null;
            }
        }
        public int GetNearestEnemyNodesAway (LaneManager[] Board) {
            LogStack.Log ("GetNearestEnemyNodesAway()", Logging.LogLevel.Color);
            CreatureBase creature = GetNearestEnemy (Board);
            if (creature != null) {
                return creature.ActiveLaneNode.laneManager.GetNodeCount - creature.LaneProgress;
            } else return -1;
        }
        public int GetNearestEnemyNodesAwayFrom (CreatureBase myCreature) {
            LogStack.Log ("GetNearestEnemyNodesAwayFrom()", Logging.LogLevel.Color);
            CreatureBase creature = GetNearestEnemyTo (myCreature);
            if (creature != null) {
                return Mathf.Abs (myCreature.LaneIndex - creature.LaneIndex);
            } else return -1;
        }
        public int GetNearestEnemyLane (LaneManager[] Board) {
            LogStack.Log ("GetNearestEnemyLane()", Logging.LogLevel.Color);
            CreatureBase creature = GetNearestEnemy (Board);
            if (creature != null) {
                return TournamentManager._instance.lanes.IndexOf (creature.ActiveLaneNode.laneManager);
            } else return -1;

        }
        public CreatureBase[] GetNearestEnemies (LaneManager[] Board) {
            LogStack.Log ("GetNearestEnemies()", Logging.LogLevel.Color);
            List<CreatureBase> nearestCreatures = new List<CreatureBase> ();
            foreach (LaneManager lane in Board) {
                LaneNode startNode = lane.GetFirstLaneNode (this);
                List<CreatureBase> tempCreatures = lane.GetEnemiesInLane (this);
                if (tempCreatures.Count > 0) {
                    nearestCreatures.Add (tempCreatures[0]);
                }
            }
            LogStack.Log ("Three nearest enemies: " + nearestCreatures.Count, Logging.LogLevel.Debug);
            // Func<int, int, bool> FindNearestOfTwoDependatOnPlayerSide = (x, y) => _PlayerNumber == 1 ? x > y : x < y;

            if (_PlayerNumber == 1)
                nearestCreatures.Sort ((a, b) => a.LaneIndex.CompareTo (b.LaneIndex)); // ascending sort
            else
                nearestCreatures.Sort ((a, b) => -1 * a.LaneIndex.CompareTo (b.LaneIndex)); // descending sort

            return nearestCreatures.ToArray ();
        }
        //returns your nearest creature and how far away it is
        // retuns null if no creature in the lane
        public Tuple<CreatureBase, int> GetMyNearestCreature (CreatureBase OpponentCreature, LaneManager[] Board) {
            if (OpponentCreature == null) return null;
            List<CreatureBase> CreaturesInLane = OpponentCreature.ActiveLaneNode.laneManager.GetFriendliesInLane (this);
            CreatureBase NearestCreature = null;
            if (CreaturesInLane.Count > 0) {
                NearestCreature = OpponentCreature.ActiveLaneNode.laneManager.GetFriendliesInLane (this) [0];
            } else NearestCreature = null;

            LogStack.Log ("GetMyNearestCreature() " + NearestCreature, Logging.LogLevel.System);
            if (NearestCreature == null) {
                return null;
            }
            return Tuple.Create (NearestCreature, Mathf.Abs (NearestCreature.LaneIndex - OpponentCreature.LaneIndex));
        }
        // returns your nearest creature and how far away it is from the other xreature
        // retuns null if no creature in the lane
        public Tuple<CreatureBase, int> GetNearestCreatureToNearestEnemy (LaneManager[] Board) {
            CreatureBase creature = GetNearestEnemy (Board);
            Tuple<CreatureBase, int> NearestCreatureToNearestEnemy = GetMyNearestCreature (creature, Board);
            if (creature != null) {
                // LogStack.Log ("GetNearestCreatureToNearestEnemy() " + NearestCreatureToNearestEnemy == null?null : NearestCreatureToNearestEnemy.Item1.ToString (), Logging.LogLevel.System);
            }
            return NearestCreatureToNearestEnemy;
        }

        public bool InRange (Spawnable CreatureType, int range) {
            return CreatureType == Spawnable.Unicorn ? range >= 3 : range == 1;
        }
        public bool InRange (Tuple<CreatureBase, int> TypeAndRange) {
            return TypeAndRange.Item1.CreatureType == Spawnable.Unicorn ? TypeAndRange.Item2 >= 3 : TypeAndRange.Item2 == 1;
        }

        //Func<CreatureBase, bool> TestIfMine = (x) => x.Owner._PlayerNumber == _PlayerNumber ? true : false;
        public LogicBase Opponent {
            get {
                return this == TournamentManager._instance.P1 ? TournamentManager._instance.P2 : TournamentManager._instance.P1;
            }
        }

    }
}