﻿using UnityEngine;
using AI;

[CreateAssetMenu(fileName = "BasicAI", menuName = "AI/BasicAI", order = 0)]
public class BasicAI : LogicBase
{
    public override void OnTick(IBoardState data)
    {
		if (!AIResponse.spawn(Spawnable.Unicorn,1))
        {

        }
        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.finalizeResponse();
    }

}