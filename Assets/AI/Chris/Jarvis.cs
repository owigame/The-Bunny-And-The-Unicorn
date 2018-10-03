using UnityEngine;
using AI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Jarvis", menuName = "AI/Jarvis", order = 0)]
public class Jarvis : LogicBase
{

    LogicBase player;
    public override void OnTick(IBoardState data)
    {
        int maxCycles = 99;

        while (AIResponse.Tokens > 0 && maxCycles > 0)
        {
            if(_Creatures.Count == 0 || Random.Range(0,2) == 1)
            {
                int laneToSpawn = Random.Range(1, TournamentManager._instance.lanes.Count + 1);
                if(Random.Range(0,2) != 1)
                {
                    if (!AIResponse.Spawn(Spawnable.Unicorn, laneToSpawn))
                    {
                        CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                        MoveAtk(randomCreature);
                    }
                }
                else
                {
                    if (!AIResponse.Spawn(Spawnable.Bunny, laneToSpawn))
                    {
                        CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                        MoveAtk(randomCreature);
                    }
                }

            }
            else
            if(_Creatures.Count >= 0)
            {
                foreach(CreatureBase creat in _Creatures)
                {
                    if (Random.Range(0, 2) == 1)
                        MoveAtk(creat);
                        
                }


            }
        }

        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
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