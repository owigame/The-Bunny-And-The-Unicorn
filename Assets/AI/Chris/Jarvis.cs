using UnityEngine;
using AI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Jarvis", menuName = "AI/Jarvis", order = 0)]
public class Jarvis : LogicBase
{

    LogicBase player;
    public override void OnTick(IBoardState[] data)
    {
        int maxCycles = 99;
        int tokens = AIResponse.Tokens;
        bool saving = true;
        bool fresh = true;

        int cnt = 0;

        if (saving)
            if (tokens > 30)
            {
                saving = false;
                fresh = false;
            }
              
        foreach(LaneManager lane in TournamentManager._instance.lanes)
        {
            if(lane.GetEnemiesInLane(this).Count > 0)
                saving = false;
        }

        if (!saving)
        {
            while (AIResponse.Tokens > 0 && maxCycles > 0)
            {
                DoSpawn();

                maxCycles--;
            }
            
        }
        //IResponse[] responses = AIResponse.QueryResponse();


        AIResponse.FinalizeResponse();
    }

    //Do spawn is zero indexed but uses spawing like it needsit
    public void DoSpawn(int lane = 0, bool randomlane = false)
    {
        var enemiesinlane = 0;
        var friendliesinlane = 0;
        int cnt = 0;
        LaneManager mostImportant = null;
        foreach (LaneManager l in TournamentManager._instance.lanes)
        {
            enemiesinlane = l.GetEnemiesInLane(this).Count;
            friendliesinlane = l.GetFriendliesInLane(this).Count;

            bool mostImportantNeedFriendlies = (mostImportant != null && mostImportant.GetEnemiesInLane(this).Count > 0 && mostImportant.GetFriendliesInLane(this).Count == 0);
            bool mostImportantCanPush = (mostImportant != null && mostImportant.GetEnemiesInLane(this).Count == 0 && mostImportant.GetFriendliesInLane(this).Count > 0);
            bool mostImportantIsEmpty = (mostImportant != null && mostImportant.GetEnemiesInLane(this).Count == 0 && mostImportant.GetFriendliesInLane(this).Count == 0);

            if(mostImportant != null)
                Debug.Log("Lane " + mostImportant.LaneNumber + " needs friendlies: " + mostImportantNeedFriendlies.ToString() + " and Can push: " + mostImportantCanPush.ToString() + " and is empty is: " + mostImportantIsEmpty.ToString());

            CreatureBase highestprog = null;
            foreach (CreatureBase creat in l.GetEnemiesInLane(this))
            {
                if (highestprog == null)
                    highestprog = creat;
                else
                if (creat.LaneProgress > highestprog.LaneProgress)
                {
                    highestprog = creat;
                }

                if (highestprog.LaneProgress < 7)
                    highestprog = null;
                    
            }

            if(highestprog != null)
            {
                mostImportant = highestprog.ActiveLaneNode.laneManager;
            }
            else
            if (!mostImportantNeedFriendlies && (enemiesinlane > 0 && friendliesinlane == 0))
            {
                mostImportant = l;
            }
            else
            if (!(mostImportantCanPush && mostImportantNeedFriendlies) && (enemiesinlane == 0 && friendliesinlane > 0))
            {
                mostImportant = l;
            }
            else
            if (!(mostImportantCanPush && mostImportantNeedFriendlies && mostImportantIsEmpty) && (enemiesinlane == 0 && friendliesinlane == 0))
            {
                mostImportant = l;
            }
            else
            if (mostImportant == null)
                mostImportant = l;
                        
                        
            
            cnt++;
        }

        Debug.Log("Most important lane is: " + mostImportant.LaneNumber);

        if (mostImportant != null)
        {
            enemiesinlane = mostImportant.GetEnemiesInLane(this).Count;
            friendliesinlane = mostImportant.GetFriendliesInLane(this).Count;
            cnt = mostImportant.LaneNumber - 1;

            if (enemiesinlane == 0 && friendliesinlane == 0)
            {
                if (Random.Range(0, 3) >= 1)
                {
                    AIResponse.Spawn(Spawnable.Unicorn, cnt + 1);
                }
                else
                {
                    AIResponse.Spawn(Spawnable.Bunny, cnt + 1);
                }
            }
            else
            if(friendliesinlane == 0)
            {
                if (Random.Range(0, 3) >= 1)
                {
                    AIResponse.Spawn(Spawnable.Unicorn, cnt + 1);
                }
                else
                {
                    AIResponse.Spawn(Spawnable.Bunny, cnt + 1);
                }
            }
            else
            {
                CreatureBase highestprog = null;
                foreach (CreatureBase creat in mostImportant.GetFriendliesInLane(this))
                {
                    if (highestprog == null)
                        highestprog = creat;
                    else
                    if (creat.LaneProgress > highestprog.LaneProgress)
                    {
                        highestprog = creat;
                    }
                }

                if(highestprog != null)
                    MoveAtk(highestprog);
            }
        }

    }

    public void MoveAtk(CreatureBase creat)
    {
        Debug.Log("Moving or attacking: " + creat.name + ", in lane: " + creat.ActiveLaneNode.laneManager.LaneNumber.ToString());
        if (creat != null)
        {
            bool TargetInRange = false;

            List<CreatureBase> Targets = creat.ActiveLaneNode.laneManager.SearchRange((int)creat.Range, creat.ActiveLaneNode, this);

            foreach (CreatureBase creature in Targets)
            {
                if (creature.Owner != creat.Owner)
                {
                    TargetInRange = true;
                    AIResponse.Attack(creat);
                }
            }

            if (!TargetInRange)
            {
                int spaces = creat.ActiveLaneNode.laneManager.GetOpenNodes(creat.ActiveLaneNode, creat.RightFacing);
                if (spaces > AIResponse.Tokens)
                {
                    spaces = AIResponse.Tokens;
                }

                if (spaces > 10)
                    spaces = 10;

                AIResponse.Move(creat, spaces);
            }
        }
    }
}