using System.Collections.Generic;

public class AIResponseManagemer {

	public AIResponseManagemer()
    {
		Response = new ResponseEvent();
		Response.AddListener(TournamentManager._instance.OnResponse);
		ResponseChain = new List<IResponse>();
    }
	
	public ResponseEvent Response;

	public List<IResponse> ResponseChain;

	public void onTick(IBoardState data)
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
		IResponse response = new ActionResponse(spawnable,lane);
		/* fail the Spawn */
		if(cost<0)
		{
			return false;
		}else{
			ResponseChain.Add(response);
			return true;
		}
	}

	public bool finalizeResponse()
	{
		/* fail the finalize */
		//if(false)
		//{
		//	return false;
		//}else{
			Response.Invoke(ResponseChain.ToArray());
			ResponseChain.Clear();
			return true;
		//}
	}
	public IResponse[] QueryResponse()
	{
		return ResponseChain.ToArray();
	}

}
