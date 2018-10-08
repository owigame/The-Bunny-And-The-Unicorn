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

        while (AIResponse.Tokens > 0 && maxCycles > 0)
        {
            if (_Creatures.Count > 3)
            {
                foreach (CreatureBase creatur in _Creatures)
                {
                    if (creatur != null && creatur.ActiveLaneNode.laneManager.SearchRange((int)creatur.Range, creatur.ActiveLaneNode, this).Count > 0)
                        MoveAtk(creatur);
                }
            }

            int cnt = 0;
            while(cnt < 2)
            {
                if (TournamentManager._instance.lanes[cnt].GetFriendliesInLane(this).Count == 0)
                {
                    DoSpawn(cnt + 1);
                }

                cnt++;
            }

            DoSpawn(0, true);

            CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];

            if(randomCreature != null)
                MoveAtk(randomCreature);

            maxCycles--;
        }

        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

    public void DoSpawn(int lane, bool randomlane = false)
    {
        if(randomlane)
            lane = Random.Range(1, TournamentManager._instance.lanes.Count + 1);

        if (Random.Range(0, 3) != 1)
        {
            if (!AIResponse.Spawn(Spawnable.Unicorn, lane))
            {
                CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                MoveAtk(randomCreature);
            }
        }
        else
        {
            if (!AIResponse.Spawn(Spawnable.Bunny, lane))
            {
                CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                MoveAtk(randomCreature);
            }
        }
    }

    public void MoveAtk(CreatureBase creat){
        
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

                AIResponse.Move(creat, spaces);
            }
        }
    }
}