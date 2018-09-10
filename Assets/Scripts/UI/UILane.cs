using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILane : MonoBehaviour {

	public TextMesh laneNumber;

	void Start () {
		if (laneNumber != null)
			laneNumber.text = GetComponent<LaneManager>().LaneNumber.ToString ();
	}

	void Update () {

	}
}