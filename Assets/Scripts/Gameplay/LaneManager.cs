using System.Collections;
using System.Collections.Generic;
using AI;
using Logging;
using UnityEngine;
// lane validation
public class LaneManager : MonoBehaviour, IBoardState {

    private void Awake () {
        laneNumber = transform.GetSiblingIndex () + 1;
        //Add all LaneNode components to list
        allNodes.Add (startNode);
        foreach (LaneNode node in middleNodes) {
            allNodes.Add (node);
        }
        allNodes.Add (endNode);
    }

    void Start () {
        TournamentManager._instance.OnTick.AddListener (OnTick);
    }

    public LaneEvent OnLaneReady = new LaneEvent ();
    public LaneNode startNode, endNode;
    public LaneNode[] middleNodes;
    List<LaneNode> allNodes = new List<LaneNode> ();
    [SerializeField] List<LaneNode> lanesTakenThisRound = new List<LaneNode> ();
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

    public void OnTick (IBoardState data) {
        lanesTakenThisRound.Clear ();
    }

    public List<CreatureBase> SearchRange (int range, LaneNode currentNode, LogicBase player) {
        List<CreatureBase> creaturesInRange = new List<CreatureBase> ();

        for (int i = -range; i < range + 1; i++) {
            if (allNodes.IndexOf (currentNode) + i >= 0 && allNodes.IndexOf (currentNode) + i < allNodes.Count) {
                if (allNodes[allNodes.IndexOf (currentNode) + i].activeCreature != null) {
                    creaturesInRange.Add (allNodes[allNodes.IndexOf (currentNode) + i].activeCreature);
                }
            }
        }
        if (player._PlayerNumber == 2) {
            creaturesInRange.Reverse ();
        }

        return creaturesInRange;
    }

    public int GetOpenNodes (LaneNode currentNode, bool rightFacing) {
        int openNodes = 0;
        for (int i = 0; i < currentNode.laneManager.allNodes.Count; i++) {
            LaneNode nextNode = allNodes[Mathf.Clamp (allNodes.IndexOf (currentNode) + (rightFacing ? i : -i), 0, currentNode.laneManager.allNodes.Count - 1)];
            if (nextNode != null && nextNode.activeCreature == null) {
                openNodes++;
            }
        }
        return openNodes;
    }

    public LaneNode GetNextLaneNode (LaneNode currentNode, bool rightFacing, int range, bool forced = false) {
        //If the next block has a creature in it, fail the move
        LaneNode nextNode = allNodes[Mathf.Clamp (allNodes.IndexOf (currentNode) + (rightFacing ? range : -range), 0, currentNode.laneManager.allNodes.Count - 1)];

        if (nextNode != null && (!lanesTakenThisRound.Contains (nextNode) || forced)) {
            LogStack.Log ("nextNode not null and available", LogLevel.System);
            if (!forced) lanesTakenThisRound.Add (nextNode);
            return nextNode;
        } else {
            return null;
        }
    }

    public LaneNode GetFirstLaneNode (bool rightFacing) {
        return allNodes[rightFacing ? 0 : allNodes.Count - 1];
    }

    public List<CreatureBase> GetEnemiesInLane (LogicBase playerLogicBase) {
        List<CreatureBase> creaturesInRange = new List<CreatureBase> ();
        foreach (CreatureBase creature in creatures) {
            if (creature.Owner != playerLogicBase) {
                creaturesInRange.Add (creature);
            }
        }
        return creaturesInRange;
    }

    public List<CreatureBase> GetFriendliesInLane (LogicBase playerLogicBase) {
        List<CreatureBase> creaturesInRange = new List<CreatureBase> ();
        foreach (CreatureBase creature in creatures) {
            if (creature.Owner == playerLogicBase) {
                creaturesInRange.Add (creature);
            }
        }
        return creaturesInRange;
    }

}