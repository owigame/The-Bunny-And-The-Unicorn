using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Human", menuName = "AI/Human", order = 0)]
public class Human : LogicBase {

    GameObject _ui;
    RectTransform canvasRect;
    RectTransform rect;
    Camera _cam;

    Spawnable lastSpawnable;

    public override void OnTick (IBoardState data) {
        if (!AIResponse.Spawn (Spawnable.Unicorn, 1)) {

        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

    void Start () {
        //Assign variables
        _cam = Camera.main;

        //Spawn UI prefab for Human control
        _ui = Instantiate (Resources.Load ("UI/UIHumanControl")) as GameObject;
        _ui.GetComponent<UIHumanInput> ().humanLogicBase = this;
        _ui.GetComponent<UIHumanInput> ().Init (TournamentManager._instance.P1 == this as LogicBase ? true : false);
        _ui.SetActive (false);
    }

    void Update () {

    }

    public void Spawn (Spawnable _spawnable) {
        lastSpawnable = _spawnable;
    }

    public void SelectLane (int _laneNumber) {
        if (!AIResponse.Spawn (Spawnable.Unicorn, _laneNumber)) {
            Debug.Log ("Could not spawn " + lastSpawnable + " in lane " + _laneNumber);
        } else {
            Debug.Log ("Spawn " + lastSpawnable + " in lane " + _laneNumber);
        }
    }

    public void SkipTurn () {
        //Skip turn add cost

    }

    void OpenUI () {
        //UI
        _ui.SetActive (true);

    }

    void CloseUI () {
        _ui.SetActive (false);
    }

}