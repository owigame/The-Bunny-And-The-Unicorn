using System.Collections;
using UnityEngine;

[CreateAssetMenu (fileName = "ThreatBear", menuName = "AI/ThreatBear", order = 0)]
public class ThreatBaer : ThreadBear {
    public override void OnTick (IBoardState[] data) {
        boardState = (LaneManager[]) data;

        for (int i = 1; i < 3; i++) {
            FormPattern (LanePattern, i, boardState);
        }

        //Auto_Nearest(boardState);

        //if (AIResponse.Tokens > 1)
        //    AIResponse.Move(MostProgressed(boardState), 1);

        AIResponse.FinalizeResponse ();
    }
    public CreatureBase MostProgressed (LaneManager[] Board) {
        CreatureBase nearest = null;
        for (int i = 0; i < 2; i++) {
            if (Board[i].GetFriendliesInLane (this).Count != 0) {
                if (nearest != null) {
                    if (Board[i].GetFriendliesInLane (this) [0].LaneProgress > nearest.LaneProgress) {
                        nearest = Board[i].GetFriendliesInLane (this) [0];
                    }
                } else nearest = Board[i].GetFriendliesInLane (this) [0];
            }
        }
        return nearest;
    }
}