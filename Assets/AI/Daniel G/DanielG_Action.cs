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

    private List<int> LI_LaneAdvantage = new List<int>();

    private int iDefTokenThreshold = 7;

    private void OnDisable()
    {        
        _Creatures.Clear();
        B_Init = false;
    }

    public override void OnTick(IBoardState data)
    {
        if (!B_Init) Init();        

        // Defense.        
        //bool b_DefendedOnce = false;
        while (AIResponse.Tokens > iDefTokenThreshold /*|| !b_DefendedOnce*/)
        {
            if (TokenCheck() == false) return;
            //b_DefendedOnce = true;
            int iWeakLane = GetDefendingLane();

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

            if (TournamentManager._instance.lanes[iWeakLane].GetFirstLaneNode(!B_StartLeft).activeCreature == null)
            {
                LogStack.Log("Trying to spawn in slot... 0:" + iWeakLane, LogLevel.Debug);
                if (!AIResponse.Spawn(Spawnable.Bunny, iWeakLane + 1))
                {
                    // Failed                    
                }
            }
            else
            {
                LogStack.Log("Failed... Trying to shift Lane", LogLevel.Debug);
                if (ShiftLane(iWeakLane)) // Failed - Space Occupied - Shift entire row...
                {
                    if (!AIResponse.Spawn(Spawnable.Bunny, iWeakLane + 1))
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

        // Offense - Attacking.
        foreach (CreatureBase creature in _Creatures)
        {
            List<CreatureBase> searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode, this);

            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                if (TokenCheck() == false) return;
                if (_creature.Owner != creature.Owner) AIResponse.Attack(creature);               
            }
        }

        // Offense - Moving.
        //if (TokenCheck() == false) return;
        //int iLane = Random.Range(0, 2);
        //List<CreatureBase> AvailableLaneTroops = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);

        //if (AvailableLaneTroops.Count > 0 && AIResponse.Tokens >= 3)
        //{
        //    if (!AIResponse.Move(AvailableLaneTroops[0], 1))
        //    {
        //        // Failed.
        //    }
        //}

        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

    private void Init()
    {
        LI_ActiveTroops.Clear();
        LI_LaneAdvantage.Clear();
        for (int iCount = 0; iCount < TournamentManager._instance.lanes.Count; iCount++) LI_ActiveTroops.Add(0);
        for (int iCount = 0; iCount < TournamentManager._instance.lanes.Count; iCount++) LI_LaneAdvantage.Add(0);
        B_StartLeft = GetStartSide();
        B_Init = true;
    }

    private bool ShiftLane(int iLane)
    {
        List<CreatureBase> LaneCreatures = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);
        //LaneCreatures.Reverse();
        foreach (CreatureBase _creature in LaneCreatures)
        {
            if (AIResponse.Move(_creature, 1))
            {
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