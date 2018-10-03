using System.Collections.Generic;
using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "Test", menuName = "AI/Test", order = 0)]
public class Test : LogicBase
{
    public override void OnTick(IBoardState data)
    {
        //--have at least three creatures in play..spawn on lanes with enemies first... --HOW?
        if (_Creatures.Count == 0)
        {

            int randomLane = Random.Range(1, TournamentManager._instance.lanes.Count + 1);
            //--attampt spawn in lane---
            if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane))
            {
                //--get a random creature to move/attack with... --HOW do I choose a type of creature?
                CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                AttemptAttack(randomCreature);
            }
        }
        else if (_Creatures.Count > 0)
        {
            //--get a random creature to move/attack with...
            CreatureBase randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
            AttemptAttack(randomCreature);
        }


        //IResponse[] responses = AIResponse.QueryResponse();
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
                }
            }
            if (!foundAttackTarget)
            {
                //Can't attack...
                float wait = Random.Range(0, 25);
                if (wait % 2 == 0)
                {
                    AttemptMove(creature);
                }
            }
        }
    }

    void AttemptMove(CreatureBase creature)
    {
        int moveSpaces = creature.ActiveLaneNode.laneManager.GetOpenNodes(creature.ActiveLaneNode, creature.RightFacing);
        if (moveSpaces > AIResponse.Tokens)
        {
            moveSpaces = AIResponse.Tokens;
        }
        AIResponse.Move(creature, moveSpaces);
    }

    void AttemptSpawn(int laneToSpawn)
    {
        AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, laneToSpawn);
    }
}