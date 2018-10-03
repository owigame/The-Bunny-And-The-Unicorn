using System.Collections;
using System.Collections.Generic;
using AI;
using EasyButtons;
using Logging;
using UnityEngine;

[CreateAssetMenu (fileName = "Human", menuName = "AI/Human", order = 0)]
public class Human : LogicBase {

    GameObject _ui;
    Camera _cam;

    Spawnable lastSpawnable;
    Animator _uiAnimator;
    public HumanActions humanActions;

    public LayerMask layerMask;

    public override void OnTick (IBoardState[] data) {
        //   Debug.Log (this.name + " OnTick()");
        OpenUI ();

    }

    void Finalise () {
        IResponse[] responses = AIResponse.QueryResponse ();
        AIResponse.FinalizeResponse ();
        Debug.Log (this.name + " cost: " + AIResponse.Tokens);
    }

    public void Start () {
        //Debug.Log (this.name + " Start()");

        //Assign variables
        _cam = Camera.main;

        humanActions = TournamentManager._instance.gameObject.AddComponent<HumanActions> ();
        humanActions.Init (this);

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

    public void SpawnOptions () {
        _uiAnimator.SetBool ("Spawn", true);
        _uiAnimator.SetBool ("Actions", false);
    }

    public void SelectLane (int _laneNumber) {
        if (!AIResponse.Spawn (lastSpawnable, _laneNumber)) {
            Debug.Log ("Could not spawn " + lastSpawnable + " in lane " + _laneNumber);
            SkipTurn (false);
        } else if (AIResponse.Tokens > 0) {
            OpenUI ();
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
            _uiAnimator.SetBool ("Actions", true);
            _uiAnimator.SetBool ("Spawn", false);
        }
    }

    public IEnumerator MoveConfirm () {
        LogStack.Log ("MoveConfirm () " + this.name, LogLevel.Stack);
        bool foundCreature = false;
        while (!foundCreature) {
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;

            Debug.DrawLine (ray.origin, ray.direction * 500, Color.yellow, 1);
            if (Physics.Raycast (ray, out hit, 500, layerMask, QueryTriggerInteraction.Collide)) {
                CreatureBase creature = hit.transform.gameObject.GetComponent<CreatureBase> ();
                LogStack.Log ("MoveConfirm () " + this.name + " | " + hit.transform.name, LogLevel.Stack);

                if (Input.GetMouseButtonDown (0) && creature?.Owner == this) {
                    foundCreature = true;
                    if (!AIResponse.Move (creature)) {
                        LogStack.Log ("Could Not Move Creature", LogLevel.Stack);
                        OpenUI ();
                    } else if (AIResponse.Tokens > 0) {
                        LogStack.Log ("Creature Moved", LogLevel.Stack);
                        _uiAnimator.SetBool ("Actions", false);
                        yield return null;
                        OpenUI ();
                    } else {
                        LogStack.Log ("Creature Moved", LogLevel.Stack);
                        Finalise ();
                        CloseUI ();
                    }
                } else if (Input.GetMouseButtonDown (1)) {
                    OpenUI ();
                    foundCreature = true;
                }
            }
            yield return null;
        }
    }

    public IEnumerator AttackConfirm () {
        LogStack.Log ("AttackConfirm () " + this.name, LogLevel.Stack);
        bool foundCreature = false;
        while (!foundCreature) {
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;

            Debug.DrawLine (ray.origin, ray.direction * 500, Color.yellow, 1);
            if (Physics.Raycast (ray, out hit, 500, layerMask, QueryTriggerInteraction.Collide)) {
                CreatureBase creature = hit.transform.gameObject.GetComponent<CreatureBase> ();
                LogStack.Log ("AttackConfirm () " + this.name + " | " + hit.transform.name, LogLevel.Stack);

                if (Input.GetMouseButtonDown (0) && creature?.Owner == this) {
                    foundCreature = true;
                    if (!AIResponse.Attack (creature)) {
                        LogStack.Log ("Could Not Attack Creature", LogLevel.Stack);
                        OpenUI ();
                    } else if (AIResponse.Tokens > 0) {
                        LogStack.Log ("Creature Attacked", LogLevel.Stack);
                        _uiAnimator.SetBool ("Actions", false);
                        yield return null;
                        OpenUI ();
                    } else {
                        LogStack.Log ("Creature Attacked", LogLevel.Stack);
                        Finalise ();
                        CloseUI ();
                    }
                } else if (Input.GetMouseButtonDown (1)) {
                    OpenUI ();
                    foundCreature = true;
                }
            }
            yield return null;
        }
    }

    void OpenUI () {
        _uiAnimator.SetBool ("Actions", true);
        _uiAnimator.SetBool ("Spawn", false);
        _uiAnimator.SetBool ("Lane", false);
    }

    void CloseUI () {
        _uiAnimator.SetBool ("Actions", false);
        _uiAnimator.SetBool ("Spawn", false);
        _uiAnimator.SetBool ("Lane", false);
    }

}