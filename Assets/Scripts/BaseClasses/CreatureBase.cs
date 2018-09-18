using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Logging;

public class CreatureBase : MonoBehaviour {

	private bool init = false;
	private LogicBase owner;
	private LaneManager lane;
	private LaneNode activeLaneNode;
	[SerializeField] private Spawnable creatureType;
	private float range;
	private int laneProgress = 0;
	private bool rightFacing = false;
	private Animator animator;
	private UIHealth uiHealth;
	private bool dead = false;
	private float damageAmount;
	private float health = 3;

	public bool isDead { get { return dead; } }
	public LogicBase Owner { get { return owner; } }
	public Spawnable CreatureType { get { return creatureType; } }
	public int LaneProgress { get { return laneProgress; } }
	public float Range { get { return range; } }

	private void Awake () {
		animator = GetComponent<Animator> ();
	}

	public void Init (LogicBase _owner, LaneManager _lane, Spawnable _type, UIHealth _healthUI, float _damageAmount) {
		if (init == false) {
			owner = _owner;
			lane = _lane;
			creatureType = _type;
			uiHealth = _healthUI;
			damageAmount = _damageAmount;
			range = _type == Spawnable.Bunny ? TournamentManager._instance.bunnyRange : TournamentManager._instance.unicornRange;
			init = true;
			rightFacing = _owner == TournamentManager._instance.P1 ? true : false;
			activeLaneNode = lane.GetFirstLaneNode (rightFacing);
			activeLaneNode.activeCreatures.Add (this);
			// Debug.Log ("Creature owned by " + owner);
		}
	}

	public void onTick (IBoardState data) {
		animator.SetBool ("Attack", false);
	}

	public void Attack () {
		if (!dead) {
			LogStack.Log(this.name + " attacking", LogLevel.Stack);
			animator.SetBool ("Attack", true);
			animator.SetTrigger ("AttackInit");
			CameraShake._CameraShake.DoCameraShake (0.1f, rightFacing ? 0.5f : -0.5f);
			KillInRange ();
		}
	}

	public void StopAttack () {
		animator.SetBool ("Attack", false);
	}

	public void KillInRange () {
		Debug.Log ("--- " + owner + "searchInRange ---");
		foreach (var creature in lane.SearchRange ((int) range, activeLaneNode)) {
			if (creature.Owner != owner) {
				Debug.Log (creature + " in range. Kill it.");
				creature.Damage (this, damageAmount);
				break;
			}
		}
	}

	public void Damage (CreatureBase killer, float amount) {
		Debug.Log (transform.name + " killed by Player " + (killer == TournamentManager._instance.P1 ? "1" : "2") + "'s " + killer.CreatureType);
		health -= amount;
		uiHealth.SetHealth (health);

		if (health <= 0) {
			dead = true;
			Die ();
		}
	}

	public void Die () {
		if (dead) {
			activeLaneNode.activeCreatures.Remove (this);
			animator.SetBool ("Die", true);
			uiHealth.gameObject.SetActive (false);
		}
	}

	public void Win () {
		if ((activeLaneNode.transform == lane.endNode && rightFacing) || (activeLaneNode.transform == lane.startNode && !rightFacing)) {
			//Made it to the end
			Debug.Log (gameObject.name + " made it to the end. 1 point to " + owner.name);
			TournamentManager._instance.ScoreUpdate (this);
			animator.SetBool ("Win", true);
			dead = true;
			activeLaneNode.activeCreatures.Remove (this);
			uiHealth.gameObject.SetActive (false);
		}
	}

	public void Move () {
		Debug.Log ("Moving Creature");
		CameraShake._CameraShake.DoCameraShake (0.2f, rightFacing ? 0.2f : -0.2f);
		activeLaneNode.activeCreatures.Remove (this);
		activeLaneNode = lane.GetNextLaneNode (activeLaneNode, rightFacing, range);
		activeLaneNode.activeCreatures.Add (this);

		transform.position = activeLaneNode.transform.position;
	}

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.layer == 10 && !dead) {
			CreatureBase otherCreature = other.attachedRigidbody.GetComponent<CreatureBase> ();
			if (otherCreature.owner != owner) {
				otherCreature.StopAttack ();
			}
		}
	}

}