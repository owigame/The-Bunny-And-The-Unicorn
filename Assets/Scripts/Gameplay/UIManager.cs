using System.Collections;
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

	public void Winner (string name) {
		if (winText != null) {
			winText.transform.parent.gameObject.SetActive(true);
			winText.text = name + " wins!";
		}
	}

	void Awake () {
		_instance = this;
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