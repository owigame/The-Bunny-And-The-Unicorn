﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public static UIManager _instance;

	[Header ("References")]
	public Text player1ScoreText;
	public Text player2ScoreText;
	public Text player1TokenText;
	public Text player2TokenText;
	public Text winText;
	public Text playe1Name;
	public Text playe2Name;
	public Text roundCountText;
	int roundCount = 0;

	public void Winner (string name) {
		if (winText != null) {
			winText.transform.parent.gameObject.SetActive (true);
			winText.text = name + " wins!";
		}
	}

	void Awake () {
		_instance = this;
		playe1Name.text = TournamentManager._instance.P1.name;
		playe2Name.text = TournamentManager._instance.P2.name;
	}

	private void Start () {
		TournamentManager._instance.OnTick.AddListener (OnTick);
	}

	public void OnTick (IBoardState[] data) {
		roundCount++;
		if (roundCountText != null) roundCountText.text = roundCount.ToString();
	}

	public void UpdateScore () {
		player1ScoreText.text = TournamentManager._instance.player1Score.ToString ();
		player2ScoreText.text = TournamentManager._instance.player2Score.ToString ();
	}

	public void UpdateToken (bool player1, float amount) {
		if (player1) {
			player1TokenText.text = amount.ToString ();
		} else {
			player2TokenText.text = amount.ToString ();
		}
	}
}