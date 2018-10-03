using System.Collections.Generic;
using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Hyena", menuName = "AI/Hyena", order = 0)]
public class Hyena : LogicBase
{
    public int lanetospawn = 1;
    private Spawnable LastSpawned;

    public override void OnTick (IBoardState data)
    {
        AIResponse.onTick (null);

        int maxCycles = 99;

        while (AIResponse.Tokens > 0 && maxCycles > 0)
        {
            if (lanetospawn > 3)
                lanetospawn = 1;

            if (LastSpawned == Spawnable.Unicorn)
            {

                if (!AIResponse.Spawn (Spawnable.Bunny, lanetospawn))
                    MoveOrAttack (_Creatures[Random.Range (0, _Creatures.Count)]);

                else
                    LastSpawned = Spawnable.Bunny;

            }
            else if (LastSpawned == Spawnable.Bunny)
            {

                if (!AIResponse.Spawn (Spawnable.Unicorn, lanetospawn))
                    MoveOrAttack (_Creatures[Random.Range (0, _Creatures.Count)]);
                else
                    LastSpawned = Spawnable.Unicorn;
            }
            else
            {

                if (!AIResponse.Spawn (Spawnable.Unicorn, lanetospawn))
                {

                    MoveOrAttack (_Creatures[Random.Range (0, _Creatures.Count)]);
                }
                else
                    LastSpawned = Spawnable.Unicorn;
            }

            lanetospawn++;

            maxCycles--;
        }

        AIResponse.FinalizeResponse ();
    }

    private void MoveOrAttack (CreatureBase creature)
    {
        if (creature == null) return;

        var searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange ((int) creature.Range, creature.ActiveLaneNode);

        var foundAttackTarget = false;

        foreach (var thisCreature in searchTargetCreatures)
        {
            if (thisCreature.Owner == creature.Owner) continue;

            var CreatureTarget = creature.ActiveLaneNode.laneManager.SearchRange ((int) 0, creature.ActiveLaneNode);

            if (CreatureTarget != null)
            {
                //Found enemy creature in range
                foundAttackTarget = true;
            }
        }

        if (!foundAttackTarget)
        {
            var moveSpaces =
                creature.ActiveLaneNode.laneManager.GetOpenNodes (creature.ActiveLaneNode, creature.RightFacing);

            if (moveSpaces > AIResponse.Tokens)
                moveSpaces = AIResponse.Tokens;

            AIResponse.Move (creature, moveSpaces);
        }
        else
        {
            AIResponse.Attack (creature);
        }
    }
}