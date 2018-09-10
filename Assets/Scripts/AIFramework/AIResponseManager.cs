using System.Collections.Generic;

// Handels responses from the AI and makes sure the AI follows the basic rules defined
public class AIResponseManager {

	// Initialise the ResponseManager by listening to the events and creating the variables
	public AIResponseManager (AI.LogicBase _logicBase) {
		Response = new ResponseEvent ();
		Response.AddListener (TournamentManager._instance.OnResponse);
		ResponseChain = new List<IResponse> ();
		logicBase = _logicBase;
	}

	// Event that communicates the AI response to the manager.
	private ResponseEvent Response;

	// Local chain of responses before sending them off.
	private List<IResponse> ResponseChain;

	private AI.LogicBase logicBase;
	private List<int> lanesTaken = new List<int> ();

	// The cost increase module. 
	// The code really should be compiled into a dll so this doesn't appear in the drop-down list [attribte hides it]
	// Possible OOP rejuggling required
	[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Never)]
	public void onTick (IBoardState data) {
		tokens++;
		tokens = UnityEngine.Mathf.Clamp(tokens, 0, TournamentManager._instance.lanes.Count);
		UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
	}

	// Limits the cost to read only.
	public int Tokens {
		get {
			return tokens;
		}
	}
	private int tokens;

	public bool Spawn (Spawnable spawnable, int lane) {
		IResponse response = new ActionResponse (spawnable, lane, logicBase);
		/* fail the Spawn */
		if (tokens <= 0 || lane > TournamentManager._instance.lanes.Count || lanesTaken.Contains(lane)) {
			return false;
		} else {
			tokens--;
			lanesTaken.Add(lane);
			UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
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
		lanesTaken.Clear();
		Response.Invoke (ResponseChain.ToArray ());
		ResponseChain.Clear ();
		return true;
		//}
	}
	public IResponse[] QueryResponse () {
		return ResponseChain.ToArray ();
	}

}