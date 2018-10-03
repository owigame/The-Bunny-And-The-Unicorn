using System.Collections.Generic;
using UnityEngine;
using AI;
using Logging;

[CreateAssetMenu(fileName = "Test", menuName = "AI/Test", order = 0)]
public class Test : LogicBase
{
    public override void OnTick(IBoardState[] data)
    {
        int maxLoop = 99;
            while (_Creatures.Count < 5 && maxLoop > 0)
            {
                maxLoop --;
                int randomLane = Random.Range(1, TournamentManager._instance.lanes.Count + 1);
                LogStack.Log("Random Lane: " + randomLane,LogLevel.Debug);
                //--attampt spawn in lane---
                if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane))
                 {
                     //--get a random creature to move/attack with... --HOW do I choose a type of creature?
                    if (_Creatures.Count > 0){
                        CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                        AttemptAttack(randomCreature);
                    }
                }   
            }
            if (_Creatures.Count >= 5)
            {
                CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                AttemptAttack(randomCreature);
            }


        //IResponse[] responses = AIResponse.QueryResponse();
        //AIResponse.responses.clear();
        AIResponse.FinalizeResponse();
    }

    void AttemptAttack(CreatureBase creature)
    {
        if (creature != null)
        {
            List<CreatureBase> searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this);
            bool foundAttackTarget = false;
            //---is target in attack range?
            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                if (_creature.Owner != creature.Owner)
                {
                    //Found enemy creature in range
                    foundAttackTarget = true;
                    AIResponse.Attack(creature);
                    AttemptMove(creature);
                }
            }
            if (!foundAttackTarget)
            {
                AttemptMove(creature);
            }
        }
    }

    void AttemptMove(CreatureBase creature)
    {
        if(creature != null)
        {
            int moveSpaces = creature.ActiveLaneNode.laneManager.GetOpenNodes(creature.ActiveLaneNode, creature.RightFacing);
            if (moveSpaces > AIResponse.Tokens)
            {
                moveSpaces = AIResponse.Tokens;
            }
            AIResponse.Move(creature, moveSpaces);
        }
    }

    void AttemptSpawn(int laneToSpawn)
    {
        AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, laneToSpawn);
    }
}