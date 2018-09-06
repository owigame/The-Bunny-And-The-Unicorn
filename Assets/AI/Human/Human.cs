using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Human", menuName = "AI/Human", order = 0)]
public class Human : LogicBase {

    GameObject _ui;
    Camera _cam;

    Spawnable lastSpawnable;
    Animator _uiAnimator;

    public override void OnTick (IBoardState data) {
        Debug.Log (this.name + " OnTick()");
        OpenUI ();

    }

    void Finalise () {
        IResponse[] responses = AIResponse.QueryResponse ();
        AIResponse.FinalizeResponse ();
        Debug.Log(this.name + " cost: " + AIResponse.Cost);
    }

    public void Start () {
        Debug.Log (this.name + " Start()");

        //Assign variables
        _cam = Camera.main;

        //Spawn UI prefab for Human control
        _ui = Instantiate (Resources.Load ("UI/UIHumanControl")) as GameObject;
        if (_ui != null) {
            _ui.GetComponent<UIHumanInput> ().SetLogicBase (this);
            _ui.GetComponent<UIHumanInput> ().Init (TournamentManager._instance.P1 == this as LogicBase ? true : false);
            _uiAnimator = _ui.GetComponent<Animator> ();
        } else {
            Debug.LogWarning ("Could not spawn UI");
        }
    }

    public void Spawn (Spawnable _spawnable) {
        lastSpawnable = _spawnable;
        _uiAnimator.SetBool ("Lane", true);
    }

    public void SelectLane (int _laneNumber) {
        if (!AIResponse.Spawn (Spawnable.Unicorn, _laneNumber, this)) {
            Debug.Log ("Could not spawn " + lastSpawnable + " in lane " + _laneNumber);
            SkipTurn (false);
        } else {
            Finalise ();
            CloseUI ();
        }
    }

    public void SkipTurn (bool skip) {
        if (skip) {
            //Skip turn add cost
            Finalise ();
            CloseUI ();
        } else {
            _uiAnimator.SetBool ("Lane", false);
        }
    }

    void OpenUI () {
        _uiAnimator.SetBool ("Spawn", true);
        _uiAnimator.SetBool ("Lane", false);
    }

    void CloseUI () {
        _uiAnimator.SetBool ("Spawn", false);
        _uiAnimator.SetBool ("Lane", false);
    }

}