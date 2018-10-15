using System;
using System.Collections.Generic;
using Logging;
using UnityEngine;

// Handels responses from the AI and makes sure the AI follows the basic rules defined
public class AIResponseManager
{

	// Initialise the ResponseManager by listening to the events and creating the variables
	public AIResponseManager (AI.LogicBase _logicBase)
	{
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
	public void onTick (IBoardState[] data)
	{
		tokens += TournamentManager._instance.tokensPerRound;
		UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
	}

	// Limits the cost to read only.
	public int Tokens
	{
		get
		{
			return tokens;
		}
	}
	private int tokens;

	List<LaneNode> spawnNodesTaken = new List<LaneNode> ();

	/// <summary>
	/// Spawns a creature // detailed instructions
	/// </summary>
	/// <param name="spawnable">the creature</param>
	public bool Spawn (Spawnable creature, int lane)
	{
		LaneNode node = logicBase == TournamentManager._instance.P1 ? TournamentManager._instance.lanes[lane - 1].startNode : TournamentManager._instance.lanes[lane - 1].endNode;
		IResponse response = new ActionResponse (creature == Spawnable.Bunny ? TournamentManager._instance.bunnyPrefab.GetComponent<CreatureBase> () : TournamentManager._instance.unicornPrefab.GetComponent<CreatureBase> (), lane, logicBase, ResponseActionType.Spawn, node);

		/* fail the Spawn */
		// LogStack.Log ("Tokens: " + tokens, LogLevel.Debug);
		// LogStack.Log ("Node Creature Count: " + (node.activeCreature != null ? 1 : 0), LogLevel.System);
		if (lane > TournamentManager._instance.lanes.Count || node.activeCreature != null || spawnNodesTaken.Contains (node))
		{
			// LogStack.Log ("Response | Spawn Failed Lane: " + lane, LogLevel.Stack);
			return false;
		}
		else
		{
			if (!SpendToken ()) return false;
			// LogStack.Log ("Response | Spawn Success Lane: " + lane, LogLevel.Stack);
			spawnNodesTaken.Add (node);
			if (!ResponseChain.Contains (response))
			{
				ResponseChain.Add (response);
			}
			else
			{
				LogStack.Log ("##### Duplicate Spawn Response", LogLevel.System);
				RefundToken ();
			}
			return true;
		}
	}

	public bool Move (CreatureBase creature, int range = 1)
	{
		LogStack.Log ("creature.ActiveLaneNode: " + creature.ActiveLaneNode, LogLevel.System);
		LogStack.Log ("creature.Owner._RightFacing: " + creature.Owner._RightFacing, LogLevel.System);

		LaneNode nextNode = creature.ActiveLaneNode.laneManager.GetNextLaneNode (creature.ActiveLaneNode, creature.Owner._RightFacing, Mathf.Abs (range));
		LogStack.Log ("nextNode: " + nextNode, LogLevel.System);

		if (creature != null && nextNode != null && nextNode.activeCreature == null)
		{
			LogStack.Log ("Next Node: " + nextNode.GetInstanceID (), LogLevel.System);
			if (SpendToken (range))
			{

				LogStack.Log ("Response | Move " + range, LogLevel.Stack);
				IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Move, nextNode);
				if (!ResponseChain.Contains (response))
				{
					ResponseChain.Add (response);
				}
				else
				{
					LogStack.Log ("##### Duplicate Move Response", LogLevel.System);
					RefundToken ();
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	public bool Attack (CreatureBase creature, int AmountOfTimes = 1) {
		bool allTrue = true;
		for (int i = 0; i < AmountOfTimes; i++) {
			if (!Attack(creature)){
				allTrue = false;
			}
		}
		return allTrue;
	}
	
	public bool Attack (CreatureBase creature) {
        if (creature == null) return false;
        List<CreatureBase> inRange = creature.ActiveLaneNode.laneManager.SearchRange ((int) creature.Range, creature.ActiveLaneNode, creature.Owner);
		if (inRange.GetEnemies (creature.Owner).Count > 0) {
			if (!SpendToken ()) return false;

		// LogStack.Log ("Response | Attack", LogLevel.Stack);
		IResponse response = new ActionResponse (creature, 0, logicBase, ResponseActionType.Attack, creature.ActiveLaneNode);
		if (!ResponseChain.Contains (response))
		{
			ResponseChain.Add (response);
		}
		else
		{
			LogStack.Log ("##### Duplicate Attack Response", LogLevel.System);
			RefundToken ();
		}
		return true;
	}

	bool SpendToken (int amount = 1)
	{
		if (tokens - amount >= 0)
		{
			tokens -= amount;
			UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
			return true;
		}
		return false;
	}

	void RefundToken (int amount = 1)
	{
		tokens += amount;
		UIManager._instance.UpdateToken (logicBase == TournamentManager._instance.P1, tokens);
	}

	public bool FinalizeResponse ()
	{
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
	public IResponse[] QueryResponse ()
	{
		return ResponseChain.ToArray ();
	}

}