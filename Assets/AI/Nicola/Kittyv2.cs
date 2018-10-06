using UnityEngine;
using System.Collections.Generic;
using AI;

[CreateAssetMenu(fileName = "Kittyv2", menuName = "AI/Kittyv2", order = 0)]
public class Kittyv2 : LogicBase
{
    int enemyCount, creatureCount;
    CreatureBase targetCreature,closestEnemy;


    public override void OnTick(IBoardState[] data)
    {
        //--Have at least one creature in each lane at all times...
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            int playerInLane = lane.GetFriendliesInLane(this).Count;
            if (playerInLane <= 0)
            {
                AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, lane.LaneNumber);
            }
        }

        //--Get Enemy Count--
        GetCount();

        //--If there are Enemies, find the closest--
        if (enemyCount != 0)
        {
            FindNearest();
            Debug.Log(targetCreature.name + " | " + targetCreature.ActiveLaneNode);
        }

        AIResponse.FinalizeResponse();
    }

    void GetCount()
    {
        //--reset count each round...
        enemyCount = 0;
       // creatureCount = 0;

        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            enemyCount += lane.GetEnemiesInLane(this).Count;
           // creatureCount += lane.GetFriendliesInLane(this).Count;
        }
    }

    void FindNearest()
    {
        List<CreatureBase> closestCreatures = new List<CreatureBase>();

        //--foreach lane
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            //--foreach creature in that lane
            foreach (CreatureBase creature in lane.GetEnemiesInLane(this))
            {
                //--Get the nearest
                int highestLaneProgress = 0;
                if (creature.LaneProgress >= highestLaneProgress)
                {
                    highestLaneProgress = creature.LaneProgress;
                    closestEnemy = creature;
                }
            }
            //--add to list
            closestCreatures.Add(closestEnemy);
        }
        Debug.Log("Closest Creature count: " + closestCreatures.Count);

        //--Find the nearest from the list
        foreach (CreatureBase creature in closestCreatures)
        {
            int highestLaneProgress = 0;
           // Debug.Log("creature lane progress: " + creature.LaneProgress);
            if (creature != null)
            {
                if (creature.LaneProgress >= highestLaneProgress)
                {
                    highestLaneProgress = creature.LaneProgress;
                    targetCreature = creature;
                }
            }
        }
    }
}