using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
// lane validation
public class LaneManager : MonoBehaviour, IBoardState {

    private void Awake()
    {
        laneNumber = transform.GetSiblingIndex() + 1;
        //Add all LaneNode components to list
        allNodes.Add(startNode);
        foreach (LaneNode node in middleNodes)
        {
            allNodes.Add(node);
        }
        allNodes.Add(endNode);
    }



    public LaneEvent OnLaneReady = new LaneEvent ();
    public LaneNode startNode, endNode;
    public LaneNode[] middleNodes;
    List<LaneNode> allNodes = new List<LaneNode> ();
    public List<CreatureBase> creatures = new List<CreatureBase> ();

    private int laneNumber = 0;
    public int LaneNumber {
        get {
            return laneNumber;
        }
    }

    public void AssignToLane (CreatureBase creature, bool player1) {
        Transform spawnNode = player1 ? startNode.transform : endNode.transform;

        creature.transform.position = spawnNode.transform.position;
        creature.transform.rotation = spawnNode.transform.rotation;

        creatures.Add (creature);
    }

    public void OffTick () {
        //PerformPhase ();
    }

    public List<CreatureBase> SearchRange (int range, LaneNode currentNode) {
        List<CreatureBase> creaturesInRange = new List<CreatureBase> ();

        for (int i = -range; i < range + 1; i++) {
            if (allNodes.IndexOf (currentNode) + i >= 0 && allNodes.IndexOf (currentNode) + i < allNodes.Count) {
                if (allNodes[allNodes.IndexOf (currentNode) + i].activeCreatures.Count > 0) {
                    creaturesInRange.AddRange (allNodes[allNodes.IndexOf (currentNode) + i].activeCreatures);
                }
            }
        }

        return creaturesInRange;
    }

    public LaneNode GetNextLaneNode (LaneNode currentNode, bool rightFacing, float range) {
        //If the next block has a creature in it, fail the move
        if (allNodes[allNodes.IndexOf (currentNode) + (rightFacing ? 1 : -1)].activeCreatures.Count > 0) {
            return allNodes[allNodes.IndexOf (currentNode)]; //Fail Move
        } else {
            return allNodes[allNodes.IndexOf (currentNode) + (rightFacing ? 1 : -1)];
        }
    }

    public LaneNode GetFirstLaneNode (bool rightFacing) {
        return allNodes[rightFacing ? 0 : allNodes.Count - 1];
    }

    //public void PerformPhase () {

    //    // rejigger for use on tokens
    //    //Move
    //    foreach (CreatureBase creature in creatures) {
    //        if (creature.CreatureType == Spawnable.Bunny) {
    //            creature.Move ();
    //        } else {
    //            creature.Attack ();
    //        }
    //    }

    //    //Attack
    //    foreach (CreatureBase creature in creatures) {
    //        if (creature.CreatureType == Spawnable.Bunny) {
    //            creature.Attack ();
    //        } else {
    //            creature.Move ();
    //        }
    //    }

    //    //Die/Win
    //    foreach (CreatureBase creature in creatures) {
    //        creature.Win ();
    //    }
    //    for (int i = 0; i < creatures.Count; i++) {
    //        if (creatures[i].isDead) {
    //            creatures.Remove (creatures[i]);
    //            i--;
    //        }
    //    }

    //    //Lane Complete
    //    OnLaneReady.Invoke ();
    //}

}