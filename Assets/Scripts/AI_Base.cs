using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class AI_Base  :ScriptableObject{
	public abstract void OnTick(IData data);
	private AIResponse response;
    protected AIResponse AIResponse
    {
        get
        {
            return response;
        }
    }
	public void init()
    {
		response = new AIResponse();
        Manager.OnTick.AddListener(response.onTick);
    }
    
}
