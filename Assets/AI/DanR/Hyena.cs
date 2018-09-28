using System.Collections.Generic;
using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
public class Hyena : LogicBase
{
    public override void OnTick(IBoardState data)
    {
        AIResponse.onTick(null);

        //Spend all tokens
        //Spawn in each lane otherwise attack or move
        var maxCycles = 99;

        while (AIResponse.Tokens > 0 && maxCycles > 0)
        {
            //Spend all tokens
            maxCycles--;
            if (Random.Range(0, 2) == 0 || _Creatures.Count == 0)
            {
                var randomLane = Random.Range(1, TournamentManager._instance.lanes.Count + 1);
                // LogStack.Log ("Random Spawn Lane " + randomLane, LogLevel.System);
                if (!AIResponse.Spawn(Random.Range(0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane))
                {
                    var randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                    AttemptMoveAttack(randomCreature);
                }
            }
            else if (_Creatures.Count > 0)
            {
                var randomCreature = _Creatures[Random.Range(0, _Creatures.Count)];
                AttemptMoveAttack(randomCreature);
            }
        }

        AIResponse.FinalizeResponse();
    }

    private void AttemptMoveAttack(CreatureBase creature)
    {
        if (creature == null) return;
        
        var searchTargetCreatures =
            creature.ActiveLaneNode.laneManager.SearchRange((int) creature.Range, creature.ActiveLaneNode);
        
        var foundAttackTarget = false;

        foreach (var thisCreature in searchTargetCreatures)
        {
            if (thisCreature.Owner == creature.Owner) continue;

            //Found enemy creature in range
            foundAttackTarget = true;
            AIResponse.Attack(creature);
        }

        if (!foundAttackTarget)
        {
            var moveSpaces =
                creature.ActiveLaneNode.laneManager.GetOpenNodes(creature.ActiveLaneNode, creature.RightFacing);

            if (moveSpaces > AIResponse.Tokens)
                moveSpaces = AIResponse.Tokens;

            AIResponse.Move(creature, moveSpaces);
        }
    }
}