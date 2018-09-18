using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

public class UIHumanInput : MonoBehaviour {

	public Transform[] flipForP2;

	Human humanLogicBase;
	RectTransform _child;
	public GameObject[] laneNumbers;

	void Awake () {
		_child = transform.GetChild (0).GetComponent<RectTransform> ();
		for (int i = 0; i < TournamentManager._instance.lanes.Count; i++){
			laneNumbers[i].SetActive(true);
		}
	}

	public void Init (bool player1) {
		if (!player1) {
			_child.localScale = new Vector3 (-1, 1, 1);
			foreach (var item in flipForP2) {
				item.localScale = new Vector3 (-1, 1, 1);
			}
		}
	}

	public void OnSpawnClick (int _spawnable) {
		humanLogicBase.Spawn ((Spawnable) _spawnable);
	}

	public void OnSpawnOptions () {
		humanLogicBase.SpawnOptions();
	}

	public void OnSelectLane (int _laneNumber) {
		humanLogicBase.SelectLane (_laneNumber);
	}

	public void OnSkipTurnClick () {
		humanLogicBase.SkipTurn (true);
	}

	public void OnBackClick () {
		humanLogicBase.SkipTurn (false);
	}
	public void OnAttackClick () {
		humanLogicBase.humanActions.Attack();
	}
	public void OnMoveClick () {
		humanLogicBase.humanActions.Move();
	}

	public void SetLogicBase (Human humanLogicBase) {
		this.humanLogicBase = humanLogicBase;
	}
}