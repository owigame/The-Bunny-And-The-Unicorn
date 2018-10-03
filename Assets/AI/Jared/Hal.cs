using System.Collections.Generic;
using AI;
using Logging;
using UnityEngine;

[CreateAssetMenu (fileName = "Hal", menuName = "AI/Hal", order = 0)]
public class Hal : LogicBase {
    public override void OnTick (IBoardState[] data) {
        AIResponse.onTick (null);

        //Spend all tokens
        //Spawn in each lane otherwise attack or move
        int maxCycles = 99;

        while (AIResponse.Tokens > 0 && maxCycles > 0) { //Spend all tokens
            maxCycles--;
            if (Random.Range (0, 2) == 0 || _Creatures.Count == 0) {
                int randomLane = Random.Range (1, TournamentManager._instance.lanes.Count + 1);
                // LogStack.Log ("Random Spawn Lane " + randomLane, LogLevel.System);
                if (!AIResponse.Spawn (Random.Range (0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, randomLane)) {
                    CreatureBase randomCreature = _Creatures[Random.Range (0, _Creatures.Count)];
                    AttemptMoveAttack (randomCreature);
                }
            } else if (_Creatures.Count > 0) {
                CreatureBase randomCreature = _Creatures[Random.Range (0, _Creatures.Count)];
                AttemptMoveAttack (randomCreature);
            }
        }

        AIResponse.FinalizeResponse ();
    }

    void AttemptMoveAttack (CreatureBase creature) {
        if (creature != null) {
            List<CreatureBase> searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange ((int) creature.Range, creature.ActiveLaneNode, this);
            bool foundAttackTarget = false;
            foreach (CreatureBase _creature in searchTargetCreatures) {
                if (_creature.Owner != creature.Owner) { //Found enemy creature in range
                    foundAttackTarget = true;
                    AIResponse.Attack (creature);
                }
            }
            if (!foundAttackTarget) {
                int moveSpaces = creature.ActiveLaneNode.laneManager.GetOpenNodes (creature.ActiveLaneNode, _RightFacing);
                if (moveSpaces > AIResponse.Tokens){
                    moveSpaces = AIResponse.Tokens;
                }
                AIResponse.Move (creature, moveSpaces);
            }
        }
    }
}