using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "BasicAI", menuName = "AI/BasicAI", order = 0)]
public class BasicAI : LogicBase
{
    public override void OnTick(IBoardState[] data)
    {
        if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        {
            AIResponse.Attack(_Creatures[0]);
        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

}