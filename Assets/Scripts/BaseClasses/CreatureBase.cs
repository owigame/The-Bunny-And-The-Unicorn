using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

public class CreatureBase : MonoBehaviour {

	private bool init = false;
	private LogicBase owner;

	public void Init (LogicBase _owner) {
		if (init == false) {
			owner = _owner;
			init = true;
			Debug.Log("Creature owned by " + owner);
		}
	}

	public LogicBase Owner {
		get {
			return owner;
		}
	}

	public void Attack () {

	}

	public void Move () {
		Debug.Log("Moving Creature");
	}

}