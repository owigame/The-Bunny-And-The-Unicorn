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
    private int iOffenseiveThreshold = 9;
    private int iFriendlyCount = 0;
    private List<CreatureBase> AttackedThisTurn = new List<CreatureBase>();

    private void OnDisable()
    {        
        _Creatures.Clear();
        B_Init = false;
    }

    public override void OnTick(IBoardState[] data)
    {
        if (!B_Init) Init();
        AttackedThisTurn.Clear();

        // ####### --- Defense --- #######        
        UpdateLaneAdvantage();
        int iCount = 0;
        while (AIResponse.Tokens > iDefTokenThreshold && iCount < 5)
        {
            iCount++;
            if (TokenCheck() == false) return;            
            //b_DefendedOnce = true;        
            int iWeakLane = GetDefendingLane();
            LogStack.Log("Weak Lane = " + iWeakLane.ToString(), LogLevel.Debug);

            CreatureBase creatureofFirstNode = B_StartLeft ? TournamentManager._instance.lanes[iWeakLane].startNode.activeCreature : TournamentManager._instance.lanes[iWeakLane].endNode.activeCreature;
            if (creatureofFirstNode == null)
            {
                LogStack.Log("Trying to spawn in slot... 0:" + iWeakLane, LogLevel.Debug);
                if (AIResponse.Spawn((LI_LaneTypes[iWeakLane])?Spawnable.Unicorn:Spawnable.Bunny, iWeakLane + 1))
                {
                    //Swaps spawning creature type (Alternates between bunnies and unicorns!)
                    LI_LaneTypes[iWeakLane] = !LI_LaneTypes[iWeakLane];
                }
            }
            else ShiftLane(iWeakLane);
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
            int iLane = Random.Range(0, 2);
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

    private bool ShiftLane(int iLane)
    {
        List<CreatureBase> LaneCreatures = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);
        LogStack.Log("Shifting lane... creatures to shift = " + LaneCreatures.Count, LogLevel.Debug);
        if (iFriendlyCount >= iOffenseiveThreshold) LaneCreatures.Reverse();
        foreach (CreatureBase _creature in LaneCreatures)
        {
            LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID(), LogLevel.Debug);
            if (AIResponse.Move(_creature, 1))
            {
                LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID() + " | Can move", LogLevel.Debug);
                if (TokenCheck() == false) return true;
            }
            else return false;
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
                        for (int i = 0; i < _creature.Health; i++) AIResponse.Attack(troop);
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