using System.Collections.Generic;
using Logging;

// Handels responses from the AI and makes sure the AI follows the basic rules defined
public class AIResponseManager {

	// Initialise the ResponseManager by listening to the events and creating the variables
	public AIResponseManager (AI.LogicBase _logicBase) {
		Response = new ResponseEvent ();
		Response.AddListener (TickManager._instance.OnResponse);
		ResponseChain = new List<IResponse> ();
		logicBase = _logicBase;
	}

	// Event that communicates the AI response to the manager.
	private ResponseEvent Response;

	// Local chain of responses before sending them off.
	private List<IResponse> ResponseChain;

	private AI.LogicBase logicBase;
	private List<int> lanesTaken = new List<int> ();

	// The tokens increase module. 
	// The code really should be compiled into a dll so this doesn't appear in the drop-down list [attribte hides it]
	// Possible OOP rejuggling required
	[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Never)]
	public void onTick (IBoardState data) {
		tokens += TournamentManager._instance.tokensPerRound;
		UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
	}

	// Limits the cost to read only.
	public int Tokens {
		get {
			return tokens;
		}
	}
	private int tokens;

	/// <summary>
	/// Spawns a creature // detailed instructions
	/// </summary>
	/// <param name="spawnable">the creature</param>
	public bool Spawn (Spawnable creature, int lane) {
		IResponse response = new ActionResponse (creature == Spawnable.Bunny ? TournamentManager._instance.bunnyPrefab.GetComponent<CreatureBase> () : TournamentManager._instance.unicornPrefab.GetComponent<CreatureBase> (), lane, logicBase, ResponseActionType.Spawn);
		/* fail the Spawn */
		LogStack.Log ("Tokens: " + tokens, LogLevel.Debug);
		LaneNode node = logicBase == TournamentManager._instance.P1 ? TournamentManager._instance.lanes[lane - 1].startNode : TournamentManager._instance.lanes[lane - 1].endNode;
		LogStack.Log ("Node Creature Count: " + node.activeCreatures.Count, LogLevel.System);
		if (tokens <= 0 || lane > TournamentManager._instance.lanes.Count || lanesTaken.Contains (lane) || node.activeCreatures.Count > 0) {
			LogStack.Log ("Response | Spawn Failed Lane: " + lane, LogLevel.Stack);
			return false;
		} else {
			SpendToken ();
			lanesTaken.Add (lane);
			ResponseChain.Add (response);
			return true;
		}
	}

	public bool Move (CreatureBase creature) {
		SpendToken ();
		LogStack.Log ("Response | Move", LogLevel.Stack);
		IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Move);
		ResponseChain.Add (response);
		return true;
	}

	public bool Attack (CreatureBase creature) {
		SpendToken ();
		LogStack.Log ("Response | Attack", LogLevel.Stack);
		IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Attack);
		ResponseChain.Add (response);
		return true;
	}

	void SpendToken () {
		tokens--;
		UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
	}

	public bool FinalizeResponse () {
		/* fail the finalize */
		//if(false)
		//{
		//	return false;
		//}else{
		lanesTaken.Clear ();
		Response.Invoke (ResponseChain.ToArray ());
		ResponseChain.Clear ();
		return true;
		//}
	}
	public IResponse[] QueryResponse () {
		return ResponseChain.ToArray ();
	}

}