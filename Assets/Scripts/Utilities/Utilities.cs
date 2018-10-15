using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI.Utilities {

	public static class Utilities {

		public static List<CreatureBase> GetEnemies (this List<CreatureBase> creatures, LogicBase owner) {
			List<CreatureBase> enemies = new List<CreatureBase> ();
			foreach (CreatureBase creature in creatures)
			{
				if (creature.Owner != owner){
					enemies.Add(creature);
				}
			}
			return enemies;
		}

	}
}