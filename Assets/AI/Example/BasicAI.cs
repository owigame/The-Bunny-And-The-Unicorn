using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "BasicAI", menuName = "AI/BasicAI", order = 0)]
public class BasicAI : LogicBase
{
    public override void OnTick(IBoardState[] data)
    {
        if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        {
            AIResponse.Attack(_Creatures.Count >0? _Creatures[0]:null);
        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

}