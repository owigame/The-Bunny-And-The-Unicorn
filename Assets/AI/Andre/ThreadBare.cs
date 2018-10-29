using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Utilities;
using UnityEngine;

[CreateAssetMenu (fileName = "ThreadBare", menuName = "AI/ThreadBare", order = 0)]
public class ThreadBare : LogicBase {
   protected LaneManager[] boardState;
    public override void OnTick (IBoardState[] board) {
        boardState = (LaneManager[]) board;

        Debug.Log (boardState.Length);
        LogStack.Log ("Run Auto Response Nearest for:" + AIResponse.Tokens, Logging.LogLevel.Color);
        //for (int i = 0; i < AIResponse.Tokens; i++)
        //{
        //    Auto_Nearest(boardState);
        //}
        Auto_Nearest (boardState);
        Auto_Nearest (boardState);
        Auto_Nearest (boardState);

        //}
        ////IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

    public void Auto_Nearest (LaneManager[] Board) {
        Tuple<CreatureBase, int> nearestAndRange = GetNearestCreatureToNearestEnemy (Board);
        if (nearestAndRange != null && _Creatures.Count > 0) {
            LogStack.Log ("I have a nearby unit", Logging.LogLevel.Color);

            List<CreatureBase> inRange = nearestAndRange.Item1.ActiveLaneNode.laneManager.SearchRange ((int) nearestAndRange.Item1.Range, nearestAndRange.Item1.ActiveLaneNode, nearestAndRange.Item1.Owner);
            if (inRange.GetEnemies (nearestAndRange.Item1.Owner).Count > 0) {
                // if (AIResponse.Attack (nearestAndRange.Item1)) {
                if (!AIResponse.Attack (nearestAndRange.Item1, nearestAndRange.Item1.CreatureType == Spawnable.Bunny ? 1 : (int) GetNearestEnemy (Board).Health)) {
                    // LogStack.Log ("Attack Validation check failed", Logging.LogLevel.System);
                } else
                    LogStack.Log ("Nearby Unit Attacking", Logging.LogLevel.Color);
            } else {
                LogStack.Log ("Try Move " + nearestAndRange.Item1.GetInstanceID (), Logging.LogLevel.System);

                if (!AIResponse.Move (nearestAndRange.Item1)) {
                    LogStack.Log ("Move validation check failed", Logging.LogLevel.Color);
                } else LogStack.Log ("Nearby Unit Moving", Logging.LogLevel.Stack);
            }
        } else if (Opponent._Creatures.Count > 0) {
            CreatureBase nearestCreature = GetNearestEnemy (Board);
            int nearestLane = 1;
            Spawnable spawnCreature = Spawnable.Bunny;
            if (nearestCreature != null) {
                spawnCreature = nearestCreature.LaneProgress < 6 ? Spawnable.Unicorn : Spawnable.Bunny;
                nearestLane = nearestCreature.ActiveLaneNode.laneManager.LaneNumber;
            }
            if (!AIResponse.Spawn (spawnCreature, nearestLane)) {
                LogStack.Log ("Spawn Validation check failed", Logging.LogLevel.Stack);
            }
        } else if (Opponent._Creatures.Count == 0) {
            LogStack.Log ("Wait till opponent does something", Logging.LogLevel.Stack);
        }
    }

}

