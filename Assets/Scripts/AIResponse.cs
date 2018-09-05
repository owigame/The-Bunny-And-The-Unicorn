using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Response : UnityEvent<IResponse[]>{}
public class AIResponse {
	public AIResponse()
    {
		Response = new Response();
		Response.AddListener(Manager._Instance.OnResponse);
		ResponseChain = new List<IResponse>();
    }
	
	public Response Response;

	public List<IResponse> ResponseChain;
	public void onTick(IData data)
	{
		cost++;
	}
	public int Cost{
		get {
			return cost;
			}
		}
	private int cost;
    public bool spawn(Spawnable spawnable,int lane)
	{
		IResponse response ;
		/* fail the Spawn */
		if(false)
		{
			return false;
		}else{
			return true;
			ResponseChain.Add(response);
		}
	}
	public bool finalizeResponse()
	{
		/* fail the finalize */
		if(false)
		{
			return false;
		}else{
			Response.Invoke(ResponseChain.ToArray());
			ResponseChain.Clear();
			return true;
		}
	}
	public IResponse[] QueryResponse()
	{
		return ResponseChain.ToArray();
	}

}
public enum Spawnable
{
	Unicorn,
	Bunny
}