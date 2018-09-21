using System.Collections;
using EasyButtons;
using UnityEngine;

public class HumanActions : MonoBehaviour {
    Human human;

    public void Init (Human _human) {
        human = _human;
    }

    [Button]
    public void Move () {
        StartCoroutine (human.MoveConfirm ());
    }

    [Button]
    public void Attack () {
        StartCoroutine (human.AttackConfirm ());
    }

}