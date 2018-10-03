using System.Collections.Generic;
using UnityEngine;
using Logging;
using AI;

[CreateAssetMenu(fileName = "DanielG_Action", menuName = "AI/DanielG_Action", order = 0)]
public class DanielG_Action : LogicBase
{
    private List<int> LI_ActiveTroops = new List<int>(3);
    public bool B_Init = false;
    public bool B_StartLeft = true;

    private void OnDisable()
    {        
        _Creatures.Clear();
        B_Init = false;
    }

    public override void OnTick(IBoardState data)
    {
        if (!B_Init) Init();        

        int iCount = 0;
        // Defense.
        foreach (LaneManager ActiveLane in TournamentManager._instance.lanes)
        {
            if (TokenCheck() == false) return;

            LogStack.Log("New Lane, iCount = " + iCount, LogLevel.Debug);
            if (Mathf.Ceil(ActiveLane.creatures.Count/2f) > LI_ActiveTroops[iCount])
            {
                if (ActiveLane.GetFirstLaneNode(!B_StartLeft).activeCreature == null)
                {
                    if (ShiftLane(iCount))
                    {
                        if (!AIResponse.Spawn(Spawnable.Unicorn, iCount + 1))
                        {
                            // Failed
                        }
                        else LI_ActiveTroops[iCount]++;
                    }
                }
                else if (!AIResponse.Spawn(Spawnable.Unicorn,iCount + 1))
                {
                    // Failed - Space Occupied - Shift entire row... TODO                    
                }
                else LI_ActiveTroops[iCount]++;
            }
            iCount++;
        }

        if (TokenCheck() == false) return;

        // Offense - Attacking.
        foreach (CreatureBase creature in _Creatures)
        {
            List<CreatureBase> searchTargetCreatures = creature.ActiveLaneNode.laneManager.SearchRange((int)creature.Range, creature.ActiveLaneNode);

            foreach (CreatureBase _creature in searchTargetCreatures)
            {
                if (TokenCheck() == false) return;
                if (_creature.Owner != creature.Owner) AIResponse.Attack(creature);               
            }
        }   

        // Offense - Moving.



        //IResponse[] responses = AIResponse.QueryResponse();
        AIResponse.FinalizeResponse();
    }

    private void Init()
    {
        LI_ActiveTroops.Clear();
        for (int iCount = 0; iCount < TournamentManager._instance.lanes.Count + 1; iCount++) LI_ActiveTroops.Add(0);
        B_StartLeft = GetStartSide();
        B_Init = true;
    }

    private bool ShiftLane(int iLane)
    {
        List<CreatureBase> LaneCreatures = TournamentManager._instance.lanes[iLane].creatures;
        foreach (CreatureBase _creature in LaneCreatures)
        {
            LogStack.Log("Checking Creature..." + _creature.name,LogLevel.Debug);
            if ( LI_ActiveTroops[iLane] == 0 || _creature.Owner != _Creatures[0].Owner)
            { //Found enemy creature in range
                //foundAttackTarget = true;
                //AIResponse.Attack(creature);
            }
            else
            {
                if (!AIResponse.Move(_creature,1))
                {
                    // Failed.
                    return false;
                }
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
}