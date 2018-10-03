using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "AI_Jared", menuName = "AI/AI_Jared", order = 0)]
public class AI_Jared : LogicBase
{
    public override void OnTick(IBoardState[] data)
    {
        if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        {

        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

}