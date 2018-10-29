using System.Collections.Generic;
using UnityEngine;
using Logging;
using AI;

[CreateAssetMenu(fileName = "DanielG_Action", menuName = "AI/DanielG_Action", order = 0)]
public class DanielG_Action : LogicBase
{
    private List<int> LI_ActiveTroops = new List<int>();
    public bool B_Init = false;
    public bool B_StartLeft = true;
    public bool B_Bunnies = true;

    private List<int> LI_LaneAdvantage = new List<int>();
    private List<bool> LI_LaneTypes = new List<bool>();

    [SerializeField]
    private int iDefTokenThreshold = 3;
    [SerializeField]
    private int iAttackTokenThreshold = 7;
    [SerializeField]
    private int iOffensiveThreshold = 9;
    [SerializeField]
    private int iPointPushThreshold = 5;
    private int iFriendlyCount = 0;

    private int iResponseLane = -1; // Stores the lane pos being used by the advanced response system.
    private CreatureBase responseCreature = new CreatureBase();
    private int iSavingTokens = 0;
    private int iUnicornAttacks = 0;
    private List<CreatureBase> AttackedThisTurn = new List<CreatureBase>();
    private enum EResponseState { Basic, Emergency, PointPush, BunnyRush }
    private EResponseState responseState = EResponseState.Basic;

    private void OnDisable()
    {        
        _Creatures.Clear();
        iSavingTokens = 0;
        responseState = EResponseState.Basic;
        B_Init = false;
    }

    public override void OnTick(IBoardState[] data)
    {
        if (!B_Init) Init();
        AttackedThisTurn.Clear();
        UpdateLaneAdvantage();
        iUnicornAttacks = 0;
        responseState = EResponseState.Basic;
        // ####### --- Advanced Response System --- #######
        EmergencyCheck();
        if (responseState == EResponseState.Emergency)
        {
            SpawnTroop(iResponseLane);
            responseState = EResponseState.Basic;
        }
        else // Ensures emergencies are priorities over point pushing and bunny rushing.
        {
            // WHY THE PROBLEM HERE?
            //PointPushCheck();

            //responseState = EResponseState.Basic;
            if (responseState == EResponseState.PointPush)
            {
                LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Point Push Triggered - iSavingTokens =" + iSavingTokens, LogLevel.Debug);
                if (AIResponse.Tokens >= iSavingTokens) PointPush();
            }
            else // Ensures point pushes are priorities over bunny rushing.
            {
                BunnyThreatCheck();
                if (responseState == EResponseState.BunnyRush && AIResponse.Tokens >= iSavingTokens) BunnyRush();
            }
        }
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Advanced Responses Complete - iSavingTokens =" + iSavingTokens, LogLevel.Debug);

        // ####### --- Defense --- #######                
        int iCount = 0;
        int iCurrentThreshold = iDefTokenThreshold + iSavingTokens;
        while ((AIResponse.Tokens > iCurrentThreshold) && iCount < 5)
        {
            iCount++;
            if (TokenCheck() == false) return;
            int iWeakLane = GetDefendingLane();
            LogStack.Log("Weak Lane = " + iWeakLane.ToString(), LogLevel.Debug);
            SpawnTroop(iWeakLane);
        }
        if (TokenCheck() == false) return;

        // ####### --- Offense (A) --- ####### 
        // ------- Stage One - Bunnies -------
        List<CreatureBase> StageTroops = GetTroops(Spawnable.Bunny, true); // Front Bunnies.
        AttackWithTroops(StageTroops);
        StageTroops.Clear(); // Reset for Second Stage

        // ------- Stage Two - Unicorns ------- 
        StageTroops = GetTroops(Spawnable.Unicorn); // All Unicorns.
        AttackWithTroops(StageTroops);
        StageTroops.Clear();


        //if (TokenCheck() == false) return;
        //foreach (CreatureBase creature in _Creatures)
        //{
        //    List<CreatureBase> RangedTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange(4, creature.ActiveLaneNode, this);
        //    if (RangedTargetCreatures.Count > 0)
        //        int moveSpaces = creature.ActiveLaneNode.laneManager.GetOpenNodes(creature.ActiveLaneNode, creature.RightFacing);

        //    if (_creature.Owner != creature.Owner) AIResponse.Attack(creature);
        //}

        iCount = 0;
        // ####### --- Offense (M) --- #######
        while (AIResponse.Tokens >= iAttackTokenThreshold && iCount < 5)
        {
            iCount++;
            if (TokenCheck() == false) return;
            int iLane = Random.Range(1, 3);
            List<CreatureBase> AvailableLaneTroops = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);

            if (AvailableLaneTroops.Count > 0 && AIResponse.Tokens >= iAttackTokenThreshold)
            {
                if (!AIResponse.Move(AvailableLaneTroops[0], 1))
                {
                    // Failed. TODO - Add outputs of fail to logstack.
                }
            }
        }

        AIResponse.FinalizeResponse();
    }

    private void Init()
    {
        LI_ActiveTroops.Clear();
        LI_LaneAdvantage.Clear();
        LI_LaneTypes.Clear();
        iFriendlyCount = 0;
        for (int iCount = 0; iCount < TournamentManager._instance.lanes.Count; iCount++)
        {
            LI_ActiveTroops.Add(0);
            LI_LaneAdvantage.Add(0);
            LI_LaneTypes.Add(false);
            iFriendlyCount += TournamentManager._instance.lanes[iCount].GetFriendliesInLane(this).Count;
        }
        B_StartLeft = GetStartSide();
        B_Init = true;
    }

    private void SpawnTroop(int iLane)
    {
        CreatureBase creatureofFirstNode = B_StartLeft ? TournamentManager._instance.lanes[iLane].startNode.activeCreature : TournamentManager._instance.lanes[iLane].endNode.activeCreature;
        if (creatureofFirstNode == null)
        {
            LogStack.Log("Trying to spawn in slot... 0:" + iLane, LogLevel.Debug);
            if (AIResponse.Spawn((LI_LaneTypes[iLane]) ? Spawnable.Unicorn : Spawnable.Bunny, iLane + 1))
            {
                //Swaps spawning creature type (Alternates between bunnies and unicorns!)
                LI_LaneTypes[iLane] = !LI_LaneTypes[iLane];
            }
        }
        else ShiftLane(iLane);
    }

    private bool ShiftLane(int iLane)
    {
        List<CreatureBase> LaneCreatures = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);
        LogStack.Log("Shifting lane... creatures to shift = " + LaneCreatures.Count, LogLevel.Debug);
        if (iFriendlyCount >= iOffensiveThreshold) LaneCreatures.Reverse();
        foreach (CreatureBase _creature in LaneCreatures)
        {
            LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID(), LogLevel.Debug);
            if (AIResponse.Move(_creature, 1))
            {
                LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID() + " | Can move", LogLevel.Debug);
                if (TokenCheck() == false) return true;
            }
            else if (iFriendlyCount > iOffensiveThreshold) return false; // Ensures front troops push for end.
        }
        return true;
    }

    private bool TokenCheck()
    {
        if (AIResponse.Tokens <= 0)
        {
            AIResponse.FinalizeResponse();
            return false;
        }
        return true;
    }

    private bool GetStartSide()
    {
        if (TournamentManager._instance.P1.name == name) return true;
        else return false;           
    }

    private void UpdateLaneAdvantage()
    {
        for (int i = 0; i < 3; i++)
        {
            LaneManager activeLane = TournamentManager._instance.lanes[i];
            LI_LaneAdvantage[i] = activeLane.GetFriendliesInLane(this).Count - activeLane.GetEnemiesInLane(this).Count;
            iFriendlyCount += TournamentManager._instance.lanes[i].GetFriendliesInLane(this).Count;
            LogStack.Log("LaneAdvantage[" + i + "] = " + LI_LaneAdvantage[i].ToString(), LogLevel.Debug);
        }
    }

    private int GetDefendingLane() // Fetch the lane with the strongest troop advantage to the attacker.
    {
        int iMin = 99;
        int iCount = 0;
        int iPos = -1;

        UpdateLaneAdvantage();
        foreach (int iVal in LI_LaneAdvantage)
        {
            if (iVal < iMin)
            {
                iMin = iVal;
                iPos = iCount;
            }
            iCount++;
        }
        return iPos;
    }

    private void EmergencyCheck()
    {
        int iBiggestThreat = -1;
        int iCount = 0;
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            if (lane.GetFriendliesInLane(this).Count == 0)
            {
                if (lane.GetEnemiesInLane(this).Count > iBiggestThreat)
                {
                    iResponseLane = iCount;
                    iBiggestThreat = lane.GetEnemiesInLane(this).Count;
                }
            }
            iCount++;
        }
        if (iBiggestThreat != -1) responseState = EResponseState.Emergency;
    }

    private void PointPushCheck()
    {
        List<CreatureBase> PotentialPushers = GetTroops(true);
        int iMinDistance = iPointPushThreshold;
        foreach (CreatureBase troop in PotentialPushers)
        {
            int EndDist = LaneEndDistance(troop);
            if (EndDist < iMinDistance)
            {
                iMinDistance = EndDist;
                responseCreature = troop;
            }
        }
        if (iMinDistance == iPointPushThreshold)
        {
            LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% iMinDistance = iPointPushThreshold", LogLevel.Debug);
            iSavingTokens = 0;
            return;
        }
        iSavingTokens = iMinDistance;
        responseState = EResponseState.PointPush;
    }

    private int LaneEndDistance (CreatureBase troop)
    {
        LaneNode currentNode = troop.ActiveLaneNode;
        LaneNode endNode = B_StartLeft ? currentNode.laneManager.startNode : currentNode.laneManager.endNode;
        int iCount = 0;
        while (currentNode != null && currentNode != endNode && iCount < 20)
        {
            iCount++;
            currentNode = currentNode.laneManager.GetNextLaneNode(currentNode, B_StartLeft, 1);
        }
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Troop - " + troop.name + " , distance to end = " + iCount, LogLevel.Debug);
        return iCount;
    }

    private void PointPush()
    {
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% POINT PUSH! %%%%%%%%%%%%%% - iSavingTokens = " + iSavingTokens, LogLevel.Debug);
        for (int i = 0; i < iSavingTokens; i++)
        {
            AIResponse.Move(responseCreature);
        }
    }

    private void BunnyThreatCheck()
    {
        // TODO
    }

    private void BunnyRush()
    {
        // TODO
    }

    private void AttackWithTroops(List<CreatureBase> Troops)
    {
        foreach (CreatureBase troop in Troops)
        {
            List<CreatureBase> searchTargetCreatures = troop.ActiveLaneNode.laneManager.SearchRange((int)troop.Range, troop.ActiveLaneNode, this);
            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                if (_creature.Owner != this && !AttackedThisTurn.Contains(_creature))
                {
                    AttackedThisTurn.Add(_creature);
                    if (troop.CreatureType == Spawnable.Unicorn) // Unicorn Attacks
                    {
                        if (_creature.Health == 1) AIResponse.Attack(troop);
                        else
                        {
                            int iCount = 0;
                            int iSpareTokens = AIResponse.Tokens - (iOffensiveThreshold + iSavingTokens);
                            if (iSpareTokens < 0) iSpareTokens = 0;
                            while (iCount < _creature.Health && iUnicornAttacks < 2 + iSpareTokens)
                            {
                                //LogStack.Log("%%%%%%%%%%%%%%%%%%% Unicorn attack #" + iCount, LogLevel.Debug);
                                iCount++; iUnicornAttacks++;
                                AIResponse.Attack(troop);
                            }
                        }
                    }
                    else AIResponse.Attack(troop); // Bunny Attacks
                }
            }
        }
    }

    private List<CreatureBase> GetTroops(bool bFront = false)
    {
        List<CreatureBase> Troops = new List<CreatureBase>();
        foreach (LaneManager lane in TournamentManager._instance.lanes)
        {
            List<CreatureBase> LaneTroops = lane.GetFriendliesInLane(this);
            if (LaneTroops.Count > 0)
            {
                if (bFront) Troops.Add(LaneTroops[0]);
                else foreach (CreatureBase creature in LaneTroops) Troops.Add(creature);
            }
        }
        return Troops;
    }

    private List<CreatureBase> GetTroops(Spawnable type, bool bFront = false)
    {
        List<CreatureBase> Troops = GetTroops(bFront);    
        List<CreatureBase> TroopsOfType = new List<CreatureBase>();
        foreach (CreatureBase creature in Troops) if (creature.CreatureType == type) TroopsOfType.Add(creature);
        return TroopsOfType;
    }
}