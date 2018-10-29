﻿using System.Collections.Generic;
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

    private void OnDisable()
    {        
        _Creatures.Clear();
        B_Init = false;
    }

    public override void OnTick(IBoardState[] data)
    {
        if (!B_Init) Init();
        int iCount = 0;
        UpdateLaneAdvantage();
        // Defense.        
        //bool b_DefendedOnce = false;
        while (AIResponse.Tokens > iDefTokenThreshold /*|| !b_DefendedOnce*/ && iCount < 5)
        {
            iCount++;
            if (TokenCheck() == false) return;            
            //b_DefendedOnce = true;        
            int iWeakLane = GetDefendingLane();
            LogStack.Log("Weak Lane = " + iWeakLane.ToString(), LogLevel.Debug);

            //if (!AIResponse.Spawn(Spawnable.Bunny, iWeakLane + 1))
            //{
            //    LogStack.Log("Failed... Trying to shift Lane", LogLevel.Debug);
            //    if (ShiftLane(iWeakLane)) // Failed - Space Occupied - Shift entire row...
            //    {
            //        if (!AIResponse.Spawn(Spawnable.Unicorn, iWeakLane + 1))
            //        {
            //            // Failed
            //        }
            //    }
            //}
            CreatureBase creatureofFirstNode = B_StartLeft ? TournamentManager._instance.lanes[iWeakLane].startNode.activeCreature : TournamentManager._instance.lanes[iWeakLane].endNode.activeCreature;
            if (creatureofFirstNode == null)
            {
                LogStack.Log("Trying to spawn in slot... 0:" + iWeakLane, LogLevel.Debug);
                if (AIResponse.Spawn((LI_LaneTypes[iWeakLane])?Spawnable.Unicorn:Spawnable.Bunny, iWeakLane + 1))
                {
                    //SwapSpawningCreatureType (Alternates between bunnies and unicorns!)
                    LI_LaneTypes[iWeakLane] = !LI_LaneTypes[iWeakLane];
                }
            }
            else /*ShiftLane(iWeakLane);*/
            {
                LogStack.Log("Failed... Trying to shift Lane", LogLevel.Debug);

                if (ShiftLane(iWeakLane)) // Failed - Space Occupied - Shift entire row...
                {
                    if (!AIResponse.Spawn(Spawnable.Unicorn, iWeakLane + 1))
                    {
                        // Failed
                    }
                }
            }
        }

        
        //int iCount = 0;
        //foreach (LaneManager ActiveLane in TournamentManager._instance.lanes)
        //{
        //    if (TokenCheck() == false) return;

        //    LogStack.Log("New Lane, iCount = " + iCount, LogLevel.Debug);
        //    if (Mathf.Ceil(ActiveLane.creatures.Count/2f) > LI_ActiveTroops[iCount])
        //    {
        //        if (ActiveLane.GetFirstLaneNode(!B_StartLeft).activeCreature == null)
        //        {
        //            if (ShiftLane(iCount))
        //            {
        //                if (!AIResponse.Spawn(Spawnable.Unicorn, iCount + 1))
        //                {
        //                    // Failed
        //                }
        //                else LI_ActiveTroops[iCount]++;
        //            }
        //        }
        //        else if (!AIResponse.Spawn(Spawnable.Unicorn,iCount + 1))
        //        {
        //            // Failed - Space Occupied - Shift entire row... TODO                    
        //        }
        //        else LI_ActiveTroops[iCount]++;
        //    }
        //    iCount++;
        //}

        if (TokenCheck() == false) return;

        // Offense - Attacking Melee.
        foreach (CreatureBase creature in _Creatures)
        {
            List<CreatureBase> searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this);

            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                //if (TokenCheck() == false) return;
                if (_creature.Owner != this)
                {
                    if (creature.CreatureType == Spawnable.Unicorn && _creature.Health > 1)
                    {
                        AIResponse.Attack(creature);
                    }
                    AIResponse.Attack(creature);
                }
            }
        }

        //if (TokenCheck() == false) return;
        //foreach (CreatureBase creature in _Creatures)
        //{
        //    List<CreatureBase> RangedTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange(4, creature.ActiveLaneNode, this);
        //    if (RangedTargetCreatures.Count > 0)
        //        int moveSpaces = creature.ActiveLaneNode.laneManager.GetOpenNodes(creature.ActiveLaneNode, creature.RightFacing);

        //    if (_creature.Owner != creature.Owner) AIResponse.Attack(creature);

        //}
        iCount = 0;
        // Offense - Moving.
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

        //IResponse[] responses = AIResponse.QueryResponse();
        LogStack.Log("%%%%%%% Loop Count: " + iCount, LogLevel.System);
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
                LogStack.Log("Current Creature = " + _creature.name+ " Current ID = " + _creature.GetInstanceID()+" | Can move", LogLevel.Debug);
                if (TokenCheck() == false) return true;
            }
            else
            {
                // Failed.
                return false;
            }
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

    private void UpdateActiveTroops()
    {

    }

    private void UpdateLaneAdvantage()
    {
        for (int i = 0; i < 3; i++)
        {
            LaneManager activeLane = TournamentManager._instance.lanes[i];
            LI_LaneAdvantage[i] = activeLane.GetFriendliesInLane(this).Count - activeLane.GetEnemiesInLane(this).Count;
            LogStack.Log("Friendlies = " + activeLane.GetFriendliesInLane(this).Count.ToString(), LogLevel.Debug);
            LogStack.Log("Enemies = " + activeLane.GetEnemiesInLane(this).Count.ToString(), LogLevel.Debug);
            iFriendlyCount += TournamentManager._instance.lanes[i].GetFriendliesInLane(this).Count;

            LogStack.Log(LI_LaneAdvantage[i].ToString(), LogLevel.Debug);
        }
    }

    private int GetDefendingLane() // Fetch the lane with the strongest advantage to the attacker.
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
}