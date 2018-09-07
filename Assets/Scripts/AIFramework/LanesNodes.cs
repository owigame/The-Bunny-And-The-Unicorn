using System.Collections.Generic;
using AI;
using UnityEngine;
public class LanesNodes : MonoBehaviour, IBoardState {
    public Transform startNode, endNode;
    public List<CreatureBase> creatures = new List<CreatureBase> ();

    public void AssignToLane (CreatureBase creature, bool player1) {
        Transform spawnNode = player1 ? startNode : endNode;

        creature.transform.position = spawnNode.transform.position;
        creature.transform.rotation = spawnNode.transform.rotation;

        creatures.Add (creature);
    }

    public void onTick (IBoardState data) {
        foreach (CreatureBase creature in creatures) {
            creature.Move();
        }
    }

}