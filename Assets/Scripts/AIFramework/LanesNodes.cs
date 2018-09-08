using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
public class LanesNodes : MonoBehaviour, IBoardState {
    public LaneEvent OnLaneReady = new LaneEvent ();
    public Transform startNode, endNode;
    public Transform[] middleNodes;
    public List<CreatureBase> creatures = new List<CreatureBase> ();

    private int laneNumber = 0;
    public int LaneNumber {
        get {
            return laneNumber;
        }
    }

    private void Awake () {

        laneNumber = transform.GetSiblingIndex () + 1;
    }

    public void AssignToLane (CreatureBase creature, bool player1) {
        Transform spawnNode = player1 ? startNode : endNode;

        creature.transform.position = spawnNode.transform.position;
        creature.transform.rotation = spawnNode.transform.rotation;

        creatures.Add (creature);
    }

    public void OffTick () {
        StartCoroutine (TickTockExecute ());
    }

    IEnumerator TickTockExecute () {
        //Move
        yield return new WaitForSeconds (TournamentManager._instance.moveWait);
        foreach (CreatureBase creature in creatures) {
            creature.Move ();
        }

        //Attack
        yield return new WaitForSeconds (TournamentManager._instance.attackWait);
        foreach (CreatureBase creature in creatures) {
            creature.Attack ();
        }

        //Die
        yield return new WaitForSeconds (TournamentManager._instance.dieWait);
        foreach (CreatureBase creature in creatures) {
            creature.Die ();
        }
        for (int i = 0; i < creatures.Count; i++) {
            if (creatures[i].isDead){
                creatures.Remove(creatures[i]);
                i--;
            }
        }

        //Lane Complete
        yield return new WaitForSeconds (TournamentManager._instance.laneReadyWait);
        OnLaneReady.Invoke ();
    }

}