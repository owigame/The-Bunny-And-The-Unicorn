using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

public class UIHumanInput : MonoBehaviour {

	public Human humanLogicBase;
	RectTransform _child;

	void Start () {
		_child = transform.GetChild (0).GetComponent<RectTransform> ();
	}

	public void Init (bool player1) {
		if (!player1) {
			_child.pivot = new Vector2 (1, 0);
			_child.anchorMin = new Vector2 (1, 0);
			_child.anchorMax = new Vector2 (1, 0);
			_child.anchoredPosition = new Vector2 (-30f, 30);
			_child.localScale = new Vector3 (-1, 1, 1);
		}
	}

	public void OnSpawnClick (int _spawnable) {
		humanLogicBase.Spawn ((Spawnable)_spawnable);
	}

	public void OnSelectLane (int _laneNumber) {
		humanLogicBase.SelectLane (_laneNumber);
	}

	public void OnSkipTurnClick () {
		humanLogicBase.SkipTurn ();
	}
}