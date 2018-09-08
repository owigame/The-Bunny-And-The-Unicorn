using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILane : MonoBehaviour {

	public TextMesh laneNumber;

	void Start () {
		if (laneNumber != null)
			laneNumber.text = GetComponent<LanesNodes>().LaneNumber.ToString ();
	}

	void Update () {

	}
}