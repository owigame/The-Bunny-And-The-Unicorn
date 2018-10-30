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
	public Text playe1Name;
	public Text playe2Name;
	public Text roundCountText;
	public Button btnNextPlayers;

	public void Winner (string name, bool tie = false) {
		if (winText != null) {
			winText.transform.parent.gameObject.SetActive (true);
			winText.text = name + (!tie ? " wins!" : "Tie");
		}
	}
	public void WinnerReset () {
		if (winText != null) {
			winText.transform.parent.gameObject.SetActive (false);
			UpdateScore();
		}
	}

	public void EndGame () {
		Debug.LogWarning ("$$$$ GAME OVER $$$$");
		btnNextPlayers.gameObject.SetActive (false);
	}

	public void NextRound () {
		playe1Name.text = TournamentManager._instance.P1.name;
		playe2Name.text = TournamentManager._instance.P2.name;
	}

	void Awake () {
		_instance = this;
	}

	private void Start () {
		TournamentManager._instance.OnTick.AddListener (OnTick);
	}

	public void OnTick (IBoardState[] data) {
		if (roundCountText != null) roundCountText.text = TournamentManager._instance.roundCount.ToString ();
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