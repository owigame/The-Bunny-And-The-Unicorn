using System.Collections;
using System.Collections.Generic;
using AI;
using Logging;
using UnityEngine;

public class CreatureBase : MonoBehaviour {

	private bool init = false;
	private LogicBase owner;
	private LaneManager lane;
	[SerializeField] private LaneNode activeLaneNode;
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
	public bool RightFacing { get { return rightFacing; } }
	public LogicBase Owner { get { return owner; } }
	public Spawnable CreatureType { get { return creatureType; } }
	public LaneNode ActiveLaneNode { get { return activeLaneNode; } }
	//TODO: include rightfacing in LaneProgress
	public int LaneProgress { get { return ActiveLaneNode.laneManager.allNodes.IndexOf (ActiveLaneNode); } }
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
			activeLaneNode = lane.GetFirstLaneNode (owner);
			activeLaneNode.activeCreature = this;
			owner._Creatures.Add (this);
			rightFacing = owner._RightFacing;
			// Debug.Log ("Creature owned by " + owner);
		}
	}

	public void onTick (IBoardState[] data) {
		animator.SetBool ("Attack", false);
	}

	public void Attack () {
		if (!dead) {
			LogStack.Log (this.name + " attacking", LogLevel.Stack);
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
		LogStack.Log ("--- " + owner + "searchInRange ---", LogLevel.Stack);
		foreach (var creature in lane.SearchRange ((int) range, activeLaneNode, owner)) {
			if (creature.Owner != owner) {
				LogStack.Log (creature + " in range. Kill it.", LogLevel.Stack);
				creature.Damage (this, damageAmount);
				break;
			}
		}
	}

	public void Damage (CreatureBase killer, float amount) {
		LogStack.Log (transform.name + " killed by Player " + (killer == TournamentManager._instance.P1 ? "1" : "2") + "'s " + killer.CreatureType, LogLevel.Stack);
		health -= amount;
		uiHealth.SetHealth (health);

		if (health <= 0) {
			Die ();
		}
	}

	public void Die () {
		dead = true;
		activeLaneNode.activeCreature = null;
		owner._Creatures.Remove (this);
		lane.creatures.Remove(this);
		animator.SetBool ("Die", true);
		uiHealth.gameObject.SetActive (false);
		if (TournamentManager.OnCreatureDead != null) TournamentManager.OnCreatureDead (this);
	}

	public void Win () {
		if ((activeLaneNode == lane.endNode && rightFacing) || (activeLaneNode == lane.startNode && !rightFacing) && dead != true) {
			//Made it to the end
			LogStack.Log (gameObject.name + " made it to the end. 1 point to " + owner.name, LogLevel.Stack);
			TournamentManager._instance.ScoreUpdate (this);
			animator.SetBool ("Win", true);
			Die ();
		}
	}

	public void Move (LaneNode nextNode) {
		LogStack.Log ("Moving Creature", LogLevel.Stack);
		CameraShake._CameraShake.DoCameraShake (0.2f, rightFacing ? 0.2f : -0.2f);
		if (activeLaneNode.activeCreature == this) activeLaneNode.activeCreature = null;
		LogStack.Log ("MOVING: Current Node - " + activeLaneNode.name, LogLevel.System);
		activeLaneNode = nextNode;
		LogStack.Log ("MOVING: New Node - " + activeLaneNode.name, LogLevel.System);
		activeLaneNode.activeCreature = this;

		transform.position = activeLaneNode.transform.position;
		Win ();
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