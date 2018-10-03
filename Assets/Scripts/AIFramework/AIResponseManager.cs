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

	// The tokens increase module. 
	// The code really should be compiled into a dll so this doesn't appear in the drop-down list [attribte hides it]
	// Possible OOP rejuggling required
	[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Never)]
	public void onTick (IBoardState[] data) {
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

	List<LaneNode> spawnNodesTaken = new List<LaneNode> ();

	/// <summary>
	/// Spawns a creature // detailed instructions
	/// </summary>
	/// <param name="spawnable">the creature</param>
	public bool Spawn (Spawnable creature, int lane) {
		LaneNode node = logicBase == TournamentManager._instance.P1 ? TournamentManager._instance.lanes[lane - 1].startNode : TournamentManager._instance.lanes[lane - 1].endNode;
		IResponse response = new ActionResponse (creature == Spawnable.Bunny ? TournamentManager._instance.bunnyPrefab.GetComponent<CreatureBase> () : TournamentManager._instance.unicornPrefab.GetComponent<CreatureBase> (), lane, logicBase, ResponseActionType.Spawn, node);
		/* fail the Spawn */
		// LogStack.Log ("Tokens: " + tokens, LogLevel.Debug);
		// LogStack.Log ("Node Creature Count: " + (node.activeCreature != null ? 1 : 0), LogLevel.System);
		if (!SpendToken () || lane > TournamentManager._instance.lanes.Count || node.activeCreature != null || spawnNodesTaken.Contains (node)) {
			// LogStack.Log ("Response | Spawn Failed Lane: " + lane, LogLevel.Stack);
			return false;
		} else {
			// LogStack.Log ("Response | Spawn Success Lane: " + lane, LogLevel.Stack);
			spawnNodesTaken.Add (node);
			ResponseChain.Add (response);
			return true;
		}
	}

	public bool Move (CreatureBase creature, int range = 1) {
		LaneNode nextNode = creature.ActiveLaneNode.laneManager.GetNextLaneNode (creature.ActiveLaneNode, creature.RightFacing, range);

		if (SpendToken (range) && creature != null && nextNode != null && nextNode.activeCreature == null) {
			// LogStack.Log ("Response | Move", LogLevel.Stack);
			IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Move, nextNode);
			ResponseChain.Add (response);
			return true;
		} else {
			return false;
		}
	}

	public bool Attack (CreatureBase creature) {
		if (!SpendToken ()) return false;

		// LogStack.Log ("Response | Attack", LogLevel.Stack);
		IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Attack, creature.ActiveLaneNode);
		ResponseChain.Add (response);
		return true;
	}

	bool SpendToken (int amount = 1) {
		if (tokens + amount >= 0) {
			tokens -= amount;
			UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
			return true;
		}
		return false;
	}

	public bool FinalizeResponse () {
		/* fail the finalize */
		//if(false)
		//{
		//	return false;
		//}else{
		Response.Invoke (ResponseChain.ToArray ());
		ResponseChain.Clear ();
		spawnNodesTaken.Clear ();
		return true;
		//}
	}
	public IResponse[] QueryResponse () {
		return ResponseChain.ToArray ();
	}

}