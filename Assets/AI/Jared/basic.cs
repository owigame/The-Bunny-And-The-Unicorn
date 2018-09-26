using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "basic", menuName = "AI/basic", order = 0)]
public class basic : LogicBase
{
    public override void OnTick(IBoardState data)
    {
		if (!AIResponse.Spawn(Spawnable.Unicorn,1))
        {

        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }
}