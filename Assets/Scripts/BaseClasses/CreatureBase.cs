using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

public class CreatureBase : MonoBehaviour {

	private bool init = false;
	private LogicBase owner;
	private LanesNodes lane;
	private Spawnable creatureType;
	private int range;
	private int laneProgress = 0;
	private bool rightFacing = false;
	private Animator animator;
	private bool dead = false;

	public bool isDead { get { return dead; } }
	public Spawnable CreatureType { get { return creatureType; } }
	public int LaneProgress { get { return laneProgress; } }
	public int Range { get { return range; } }

	private void Awake () {
		animator = GetComponent<Animator> ();
	}

	public void Init (LogicBase _owner, LanesNodes _lane, Spawnable _type) {
		if (init == false) {
			owner = _owner;
			lane = _lane;
			creatureType = _type;
			range = _type == Spawnable.Bunny ? TournamentManager._instance.bunnyRange : TournamentManager._instance.unicornRange;
			init = true;
			rightFacing = _owner == TournamentManager._instance.P1 ? true : false;
			if (!rightFacing) laneProgress = lane.middleNodes.Length - 1;
			Debug.Log ("Creature owned by " + owner);
		}
	}

	public LogicBase Owner {
		get {
			return owner;
		}
	}

	public void onTick (IBoardState data) {
		animator.SetBool ("Attack", false);
	}

	public void Attack () {
		animator.SetBool ("Attack", true);
		animator.SetTrigger ("AttackInit");
	}

	public void StopAttack () {
		animator.SetBool ("Attack", false);
	}

	public void Die () {
		if (dead) {
			// lane.creatures.Remove (this);
			animator.SetBool ("Die", true);
		} else {
			if (rightFacing) {
				laneProgress++;
			} else {
				laneProgress--;
			}
		}
	}

	public void Move () {
		Debug.Log ("Moving Creature");
		if ((laneProgress < lane.middleNodes.Length && rightFacing) || (laneProgress >= 0 && !rightFacing)) {
			//Progress down lane
			transform.position = lane.middleNodes[laneProgress].position;
		} else if ((laneProgress >= lane.middleNodes.Length && rightFacing) || (laneProgress < 0 && !rightFacing)) {
			//Made it to the end
			transform.position = rightFacing ? lane.endNode.position : lane.startNode.position;
			Debug.Log (gameObject.name + " made it to the end. 1 point to " + owner.name);
		}
	}

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.layer == 10 && !dead) {
			CreatureBase otherCreature = other.attachedRigidbody.GetComponent<CreatureBase> ();
			if (otherCreature.owner != owner) {
				if (Mathf.Abs (otherCreature.LaneProgress - laneProgress) <= otherCreature.Range) {
					Debug.Log (transform.name + " died by Player " + (otherCreature.owner == TournamentManager._instance.P1 ? "1" : "2") + "'s " + otherCreature.CreatureType);
					dead = true;
					otherCreature.StopAttack ();
				}
			}
		}
	}

}