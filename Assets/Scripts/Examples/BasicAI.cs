using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "AI_Base", menuName = "AI_Base", order = 0)]
public class BasicAI : LogicBase
{
    public override void OnTick(IBoardState data)
    {
        //example
		if (!AIResponse.spawn(Spawnable.Unicorn,1))
        {

        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.finalizeResponse();
    }

}
