using System.Collections.Generic;

// Handels responses from the AI and makes sure the AI follows the basic rules defined
public class AIResponseManager {

	// Initialise the ResponseManager by listening to the events and creating the variables
	public AIResponseManager () {
		Response = new ResponseEvent ();
		Response.AddListener (TournamentManager._instance.OnResponse);
		ResponseChain = new List<IResponse> ();
	}

	// Event that communicates the AI response to the manager.
	private ResponseEvent Response;

	// Local chain of responses before sending them off.
	private List<IResponse> ResponseChain;

	// The cost increase module. 
	// The code really should be compiled into a dll so this doesn't appear in the drop-down list [attribte hides it]
	// Possible OOP rejuggling required
	[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Never)]
	public void onTick (IBoardState data) {
		cost++;
	}

	// Limits the cost to read only.
	public int Cost {
		get {
			return cost;
		}
	}
	private int cost;

	public bool Spawn (Spawnable spawnable, int lane) {
		IResponse response = new ActionResponse (spawnable, lane);
		/* fail the Spawn */
		if (cost <= 0) {
			return false;
		} else {
			cost --;
			ResponseChain.Add (response);
			return true;
		}
	}

	public bool FinalizeResponse () {
		/* fail the finalize */
		//if(false)
		//{
		//	return false;
		//}else{
		Response.Invoke (ResponseChain.ToArray ());
		ResponseChain.Clear ();
		return true;
		//}
	}
	public IResponse[] QueryResponse () {
		return ResponseChain.ToArray ();
	}

}