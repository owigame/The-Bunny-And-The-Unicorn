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

        int cnt = 0;
        while (cnt < 2)
            for (int i = 0; i < 2; i++)
            {
                if (TournamentManager._instance.lanes[i].GetEnemiesInLane(this).Count > 0)
                {
                    saving = false;
                }

                cnt++;
            }

        if (saving)
            if (tokens > 30)
                saving = false;

        if (!saving)
        {
            if (AIResponse.Tokens > 2)
            {
                while (AIResponse.Tokens > 0 && maxCycles > 0)
                {
                    LogStack.Log("Cycle count: " + (99 - maxCycles), Logging.LogLevel.Debug);
                    if (_Creatures.Count > 3)
                    {
                        foreach (CreatureBase creatur in _Creatures)
                        {
                            if (creatur != null && creatur.ActiveLaneNode.laneManager.SearchRange((int)creatur.Range, creatur.ActiveLaneNode, this).Count > 0)
                                MoveAtk(creatur);
                        }
                    }

                    cnt = 0;
                    while (cnt < 2)
                        for (int i = 0; i < 2; i++)
                        {
                            if (TournamentManager._instance.lanes[i].GetFriendliesInLane(this).Count == 0)
                            {
                                DoSpawn(i);
                            }
                            cnt++;
                        }


                    DoSpawn(randomlane: true);

                    maxCycles--;
                }
            }
        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

    //Do spawn is zero indexed but uses spawing like it needsit
    public void DoSpawn(int lane = 0, bool randomlane = false)
    {
        if (randomlane)
            lane = Random.Range(1, TournamentManager._instance.lanes.Count);

        LogStack.Log("Do Spawn. Lane: " + lane + ", Is Random: " + randomlane.ToString(), Logging.LogLevel.Debug);

        if (Random.Range(0, 3) != 1)
        {
            LogStack.Log("Attempting spawn: Unicorn", Logging.LogLevel.Debug);
            if (!AIResponse.Spawn(Spawnable.Unicorn, lane + 1))
            {
                LogStack.Log("Couldn't spawn Unicorn in Lane " + lane, Logging.LogLevel.Debug);
                if (_Creatures.Count > 0)
                {
                    LogStack.Log("Choosing random creature to move or attack", Logging.LogLevel.Debug);
                    CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count - 1)];
                    MoveAtk(randomCreature);
                }
            }
        }
        else
        {
            LogStack.Log("Attempting spawn: Bunny", Logging.LogLevel.Debug);
            if (!AIResponse.Spawn(Spawnable.Bunny, lane + 1))
            {
                LogStack.Log("Couldn't spawn Bunny in Lane " + lane, Logging.LogLevel.Debug);
                if (_Creatures.Count > 0)
                {
                    LogStack.Log("Choosing random creature to move or attack", Logging.LogLevel.Debug);
                    CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                    MoveAtk(randomCreature);
                }
            }
        }
    }

    public void MoveAtk(CreatureBase creat)
    {

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