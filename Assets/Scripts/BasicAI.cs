using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AI_Base", menuName = "AI_Base", order = 0)]
public class BasicAI : AI_Base {
    public override void OnTick(IData data)
    {
        //example
		if (!AIResponse.spawn(Spawnable.Unicorn,1))
        {

        }
        IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.finalizeResponse();
    }

}
