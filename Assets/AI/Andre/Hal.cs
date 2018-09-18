using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "Hal", menuName = "AI/Hal", order = 0)]
public class Hal : LogicBase {
    public override void OnTick (IBoardState data) {
        AIResponse.onTick (null);

        for (int i = 0; i < AIResponse.Tokens; i++) {
            if (!AIResponse.Spawn (Spawnable.Bunny, 1)) {
                AIResponse.Move (TournamentManager._instance.lanes[0].creatures[0]);
            }

            // {

            // }
            // //IResponse[] responses = AIResponse.QueryResponse();
        }
        
        AIResponse.FinalizeResponse ();
    }
}