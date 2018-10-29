using AI;
using UnityEngine;

[CreateAssetMenu (fileName = "BasicAI", menuName = "AI/BasicAI", order = 0)]
public class BasicAI : LogicBase {
    public override void OnTick (IBoardState[] data) {
        if (!AIResponse.Spawn (Random.Range (0, 2) == 0 ? Spawnable.Bunny : Spawnable.Unicorn, Random.Range (1, 4))) {
            if (_Creatures.Count > 0) AIResponse.Attack (_Creatures[0]);
        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse ();
    }

}