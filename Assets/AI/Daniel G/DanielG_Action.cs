using System.Collections.Generic;
using UnityEngine;
using Logging;
using AI;
using System;

[CreateAssetMenu(fileName = "DanielG_Action", menuName = "AI/DanielG_Action", order = 0)]
public class DanielG_Action : LogicBase
{
    private List<int> LI_ActiveTroops = new List<int>();
    public bool B_Init = false;
    public bool B_StartLeft = true;

    private List<int> LI_LaneAdvantage = new List<int>();
    private List<bool> LI_LaneTypes = new List<bool>();

    [Header("Thresholds")]
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

    // --- PERFORMANCE TRACKING VARIABLES --- \\
    [Header("Hash Function Settings")]
    public int I_TurnsBetweenChecks = 4;
    public int I_TurnsBetweenChanges = 12;
    public int I_TurnsToTrack = 7;
    public int I_PointValue = 30;
    public int I_ProgressValue = 2;
    public int I_TroopValue = 4;
    public int I_TokenValue = 3;

    LogicBase Enemy = null;
    private int I_Round = 0;
    private int I_Deaths = 0;
    private int I_Kills = 0;
    private int I_Tokens = 0;
    private int I_EnemyTokens = 0;
    private int I_TokensSpent = 0;
    private int I_EnemyTokensSpent = 0;
    private int I_Points = 0;
    private int I_EnemyPoints = 0;
    private int I_PointsGain = 0;
    private int I_EnemyPointsGain = 0;
    private int I_Progress = 0;
    private int I_EnemyProgress = 0;
    private float F_AverageProgress = 0f;
    private float F_AverageEnemyProgress = 0f;
    private Queue<float> Q_RoundPerformances = new Queue<float>();
    private enum EPerformanceState { VeryBad, Bad, Normal, Good, VeryGood }
    [SerializeField]
    private EPerformanceState ELearningState = EPerformanceState.Normal;
    private Queue<EPerformanceState> Q_PerformanceStates = new Queue<EPerformanceState>();
    private enum EAdaptState { IncreaseDefenseAlot, IncreaseDefense, Neutral, IncreaseOffense, IncreaseOffenseAlot }
    [SerializeField]
    private EAdaptState EAdaption = EAdaptState.Neutral;

    private void OnDisable()
    {        
        _Creatures.Clear();
        iSavingTokens = 0;
        responseState = EResponseState.Basic;
        B_Init = false;
        TournamentManager.OnCreatureDead -= TrackCreatureDeaths;
    }

    public override void OnTick(IBoardState[] data)
    {
        if (!B_Init) Init();
        I_Round++;
        
        // ####### --- Performance Tracking (Learning) System --- #######
        PrintPerformance();
        UpdatePerformanceData();
        CalculatePerformance();
        if (I_Round % I_TurnsBetweenChecks == 0) EvaluatePerformance();
        if (I_Round % I_TurnsBetweenChanges == 0) Adapt();

        AttackedThisTurn.Clear();
        UpdateLaneAdvantage();
        iUnicornAttacks = 0;

        // ####### --- Advanced Response System --- #######
        responseState = EResponseState.Basic;
        EmergencyCheck();
        if (responseState == EResponseState.Emergency)
        {
            SpawnTroop(iResponseLane);
            responseState = EResponseState.Basic;
        }
        else // Ensures emergencies are priorities over point pushing and bunny rushing.
        {
            PointPushCheck();
            if (responseState == EResponseState.PointPush)
            {
                LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Point Push Triggered - iSavingTokens =" + iSavingTokens, LogLevel.Debug);
                if (AIResponse.Tokens >= iSavingTokens) AdvancedResponse();
            }
            else // Ensures point pushes are priorities over bunny rushing.
            {     
                BunnyThreatCheck();
                if (responseState == EResponseState.BunnyRush && AIResponse.Tokens > iSavingTokens)
                {
                    iSavingTokens--;
                    AdvancedResponse();
                }
            }
        }
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Advanced Responses Complete - iSavingTokens =" + iSavingTokens, LogLevel.Debug);

        // ####### --- Basic Response System --- #######
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
        //if (TokenCheck() == false) return;   

        // ####### --- Offense (A) --- ####### 
        // ------- Stage One - Bunnies -------
        List<CreatureBase> StageTroops = GetTroops(Spawnable.Bunny, true); // Front Bunnies.
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Front Bunnies Count =" + StageTroops.Count, LogLevel.Debug);
        AttackWithTroops(StageTroops);
        StageTroops.Clear(); // Reset for Second Stage

        // ------- Stage Two - Unicorns ------- 
        StageTroops = GetTroops(Spawnable.Unicorn); // All Unicorns.
        AttackWithTroops(StageTroops);
        StageTroops.Clear();

        // ####### --- Offense (M) --- ####### ------------------------------------------------------------------------ TODO - Change to use all available extra tokens? Use dedicated "Push" and "Push Support" lanes.
        if (TokenCheck() == false) return;
        iCurrentThreshold = iAttackTokenThreshold + iSavingTokens;
        int iAvalableTokens = AIResponse.Tokens - iCurrentThreshold;  
        if (iAvalableTokens > 0)
        {
            int iLane = UnityEngine.Random.Range(1, 3);
            List<CreatureBase> AvailableLaneTroops = TournamentManager._instance.lanes[iLane].GetFriendliesInLane(this);
            if (AvailableLaneTroops.Count > 0) AIResponse.Move(AvailableLaneTroops[0], iAvalableTokens);
        }
        
        AIResponse.FinalizeResponse();
    }   

    private void Init()
    {
        iDefTokenThreshold = 3;
        iAttackTokenThreshold = 7;
        iOffensiveThreshold = 9;
        iPointPushThreshold = 5;
        LI_ActiveTroops.Clear();
        LI_LaneAdvantage.Clear();
        LI_LaneTypes.Clear();
        iFriendlyCount = 0;
        I_Round = 0;
        for (int iCount = 0; iCount < TournamentManager._instance.lanes.Count; iCount++)
        {
            LI_ActiveTroops.Add(0);
            LI_LaneAdvantage.Add(0);
            LI_LaneTypes.Add(false);
            iFriendlyCount += TournamentManager._instance.lanes[iCount].GetFriendliesInLane(this).Count;
        }
        B_StartLeft = GetStartSide();
        if (B_StartLeft) Enemy = TournamentManager._instance.P2;
        else Enemy = TournamentManager._instance.P1;
        responseState = EResponseState.Basic;
        EAdaption = EAdaptState.Neutral;
        ELearningState = EPerformanceState.Normal;
        TournamentManager.OnCreatureDead += TrackCreatureDeaths;
        B_Init = true;
    }

    private void AdvancedResponse()
    {
        AIResponse.Move(responseCreature, iSavingTokens);
        iSavingTokens = 0;
    }

    private void SpawnTroop(int iLane)
    {
        CreatureBase creatureofFirstNode = B_StartLeft ? TournamentManager._instance.lanes[iLane].startNode.activeCreature : TournamentManager._instance.lanes[iLane].endNode.activeCreature;
        if (creatureofFirstNode == null)
        {
            //LogStack.Log("Trying to spawn in slot... 0:" + iLane, LogLevel.Debug);
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
        //LogStack.Log("Shifting lane... creatures to shift = " + LaneCreatures.Count, LogLevel.Debug);
        if (iFriendlyCount >= iOffensiveThreshold) LaneCreatures.Reverse();
        foreach (CreatureBase _creature in LaneCreatures)
        {
            //LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID(), LogLevel.Debug);
            if (AIResponse.Move(_creature, 1))
            {
                //LogStack.Log("Current Creature = " + _creature.name + " Current ID = " + _creature.GetInstanceID() + " | Can move", LogLevel.Debug);
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
            //LogStack.Log("LaneAdvantage[" + i + "] = " + LI_LaneAdvantage[i].ToString(), LogLevel.Debug);
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
            if (lane.GetFriendliesInLane(this).Count == 0 || lane.startNode.activeCreature == null)
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
        bool bOpenLane = false;
        foreach (CreatureBase troop in PotentialPushers)
        {
            LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% Checking Troop: " + troop.name + " LaneProgress = " + troop.LaneProgress, LogLevel.Debug);
            int EndDist = 10 - troop.LaneProgress;
            LogStack.Log("********************** Open Nodes = " + troop.ActiveLaneNode.laneManager.GetOpenNodes(troop.ActiveLaneNode, B_StartLeft) + " ********************* ", LogLevel.Debug);
            if (troop.LaneProgress > 1 && (10 - troop.LaneProgress <= troop.ActiveLaneNode.laneManager.GetOpenNodes(troop.ActiveLaneNode,B_StartLeft)))
            {
                LogStack.Log("********************** OPEN LANE DETECTED ********************* ",LogLevel.Debug);
                responseCreature = troop;
                iMinDistance = EndDist;
                bOpenLane = true;
                break;
            }
            if (EndDist < iMinDistance && !bOpenLane)
            {
                iMinDistance = EndDist;
                responseCreature = troop;
            }
        }
        if (!bOpenLane && iMinDistance == iPointPushThreshold)
        {
            //LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% iMinDistance = iPointPushThreshold", LogLevel.Debug);
            iSavingTokens = 0;
        }
        else
        {
            iSavingTokens = iMinDistance;
            responseState = EResponseState.PointPush;
        }
    }

    private void BunnyThreatCheck()
    {
        // TODO
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% BunnyThreatCheck(1) %%%%%%%%%%%%%% - iSavingTokens = " + iSavingTokens, LogLevel.Debug);

        List<CreatureBase> ThreatenedBunnies = GetTroops(Spawnable.Bunny,true);
        int iMaxThreatDist = 99;
        foreach (CreatureBase bunny in ThreatenedBunnies)
        {
            if (bunny.Health == 1) continue;
            int iTargetDist = -1;
            CreatureBase Target = GetClosestEnemy(bunny, out iTargetDist);
            if (Target != null && Target.CreatureType == Spawnable.Unicorn && iMaxThreatDist >= iTargetDist && iTargetDist <= bunny.Health + 1)
            {
                LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% BunnyThreatCheck(2) %%%%%%%%%%%%%% - FoundTarget = " + Target.name + " @ " + iTargetDist + " Tiles Away", LogLevel.Debug);
                responseCreature = bunny;
                iMaxThreatDist = iTargetDist;
            }
        }
        if (iMaxThreatDist != 99)
        {
            iSavingTokens = iMaxThreatDist + 1;
            responseState = EResponseState.BunnyRush;
            LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% BunnyThreatCheck(3) %%%%%%%%%%%%%% - iSavingTokens = " + iSavingTokens, LogLevel.Debug);
        }
        LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% BunnyThreatCheck(4) %%%%%%%%%%%%%% - iSavingTokens = " + iSavingTokens, LogLevel.Debug);
    }

    private CreatureBase GetClosestEnemy(CreatureBase Troop, out int iDistance, int iMaxDist = 7)
    {
        List<CreatureBase> searchTargets = Troop.ActiveLaneNode.laneManager.SearchRange(iMaxDist, Troop.ActiveLaneNode, this);
        if (searchTargets != null && searchTargets.Count > 0)
        {
            iDistance = Troop.ActiveLaneNode.laneManager.GetOpenNodes(Troop.ActiveLaneNode, _RightFacing);
            LogStack.Log("%%%%%%%%%%%%%%%%%%%%%% GetClosestEnemy(1) %%%%%%%%%%%%%% - iDistance = " + iDistance, LogLevel.Debug);
            if (iDistance >= iMaxDist) iDistance = 0;
            if (iDistance == 0) return null;
            return searchTargets[0];
        }
        else
        {
            iDistance = -1;
            return null;
        }
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
                            int iSpareTokens = AIResponse.Tokens - (iAttackTokenThreshold + iSavingTokens); // NB NB NB CHANGED iAttackTokenThreshold from iOffensiveThreshold
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

    private void TrackCreatureDeaths(CreatureBase creature)
    {
        if (creature.Owner == this) I_Deaths++;
        else I_Kills++;
    }

    private void UpdatePerformanceData()
    {
        I_Deaths = 0; // Reset both for tracking in the next round.
        I_Kills = 0;

        I_TokensSpent = AIResponse.Tokens - I_Tokens;
        I_Tokens = AIResponse.Tokens;                       
        I_EnemyTokensSpent = Enemy.AIResponse.Tokens - I_EnemyTokens;
        I_EnemyTokens = Enemy.AIResponse.Tokens;

        int CurrentPoints, CurrentEnemyPoints;
        if (B_StartLeft)
        {
            CurrentPoints = TournamentManager._instance.player1Score;
            CurrentEnemyPoints = TournamentManager._instance.player2Score;
        }
        else
        {
            CurrentEnemyPoints = TournamentManager._instance.player1Score;
            CurrentPoints = TournamentManager._instance.player2Score;
        }

        I_PointsGain = CurrentPoints - I_Points;
        I_Points = CurrentPoints;
        I_EnemyPointsGain = CurrentEnemyPoints - I_EnemyPoints;
        I_EnemyPoints = CurrentEnemyPoints;

        List<CreatureBase> Troops = GetTroops();
        List<CreatureBase> Enemies = Enemy._Creatures;
        F_AverageProgress = UpdateProgressData(Troops);
        F_AverageEnemyProgress = UpdateProgressData(Enemies);
    }

    private void CalculatePerformance() // The AI's "Learning" Hash Function.
    {
        float f_Performance = 0f;
        // Good Performance
        f_Performance += I_PointsGain * I_PointValue;
        f_Performance += F_AverageProgress * I_ProgressValue;
        f_Performance += I_Kills * I_TroopValue;
        f_Performance += I_TokensSpent * I_TokenValue;
        // Bad Performance
        f_Performance -= I_EnemyPointsGain * I_PointValue;
        f_Performance -= F_AverageEnemyProgress * I_ProgressValue;
        f_Performance -= I_Deaths * I_TroopValue;
        f_Performance -= I_EnemyTokensSpent * I_TokenValue;
        Q_RoundPerformances.Enqueue(f_Performance);
        if (Q_RoundPerformances.Count > I_TurnsToTrack) Q_RoundPerformances.Dequeue();        
    }

    private void EvaluatePerformance()
    {
        float fAveragePerformance = 0f;
        foreach (float performance in Q_RoundPerformances)
        {
            LogStack.Log("Performance = " + performance, LogLevel.Debug);
            fAveragePerformance += performance;
        }
        fAveragePerformance /= Q_RoundPerformances.Count;
        LogStack.Log("fAveragePerformance = " + fAveragePerformance, LogLevel.Debug);

        if (fAveragePerformance > 2.1f) ELearningState = EPerformanceState.VeryGood;
        else if (fAveragePerformance < -2.7f) ELearningState = EPerformanceState.VeryBad;
        else if (fAveragePerformance > 0.7f) ELearningState = EPerformanceState.Good;
        else if (fAveragePerformance < -1.3f) ELearningState = EPerformanceState.Bad;
        else ELearningState = EPerformanceState.Normal;

        Q_PerformanceStates.Enqueue(ELearningState);
        if (Q_PerformanceStates.Count > 2) Q_PerformanceStates.Dequeue();
    }

    private void PrintPerformance()
    {
        LogStack.Log("########################################################", LogLevel.Debug);
        LogStack.Log("################ PERFORMANCE REPORT - ACTION ################", LogLevel.Debug);
        LogStack.Log("########################################################", LogLevel.Debug);
        LogStack.Log("--------------------------- ", LogLevel.Debug);
        LogStack.Log("--- SCORE STATUS --- ", LogLevel.Debug);
        LogStack.Log("Enemy Points = " + I_EnemyPoints + " | Gained " + I_EnemyPointsGain, LogLevel.Debug);
        LogStack.Log("Points = " + I_Points + " | Gained " + I_PointsGain, LogLevel.Debug);
        LogStack.Log("--------------------------- ", LogLevel.Debug);
        LogStack.Log("--- PROGRESS STATUS --- ", LogLevel.Debug);
        LogStack.Log("Enemy Progress = " + I_EnemyProgress + " | Average = " + F_AverageEnemyProgress, LogLevel.Debug);
        LogStack.Log("Progress = " + I_Progress + " | Average " + F_AverageProgress, LogLevel.Debug);
        LogStack.Log("--------------------------- ", LogLevel.Debug);
        LogStack.Log("--- CREATURE STATUS --- ", LogLevel.Debug);
        LogStack.Log("Enemy Kills Last Turn = " + I_Kills, LogLevel.Debug);
        LogStack.Log("Friendly Deaths Last Turn = " + I_Deaths, LogLevel.Debug);
        LogStack.Log("--------------------------- ", LogLevel.Debug);
        LogStack.Log("--- TOKEN STATUS --- ", LogLevel.Debug);
        LogStack.Log("Enemy Tokens = " + I_EnemyTokens + " | Spent " + I_EnemyTokensSpent, LogLevel.Debug);
        LogStack.Log("Tokens = " + I_Tokens + " | Spent " + I_TokensSpent, LogLevel.Debug);
        LogStack.Log("--------------------------- ", LogLevel.Debug);
        LogStack.Log("########################################################", LogLevel.Debug);
    }

    private void Adapt()
    {
        int I_AvargePerformance = 0;
        foreach (EPerformanceState performance in Q_PerformanceStates) I_AvargePerformance += (int)performance;
        I_AvargePerformance = (int)Mathf.Round(I_AvargePerformance / Q_PerformanceStates.Count);

        switch (I_AvargePerformance)
        {
            case 0:
                IncreaseDefense(true);
                break;
            case 1:
                IncreaseDefense();
                break;
            case 2:
                if (UnityEngine.Random.Range(1, 3) == 1) IncreaseOffense();
                else IncreaseDefense();
                break;
            case 3:
                IncreaseOffense();
                break;
            case 4:
                IncreaseOffense(true);
                break;
            default:
                break;
        }

        EAdaption = (EAdaptState)I_AvargePerformance;
    }

    private void IncreaseDefense(bool b_Alot = false)
    {
        if (b_Alot)
        {
            if (iDefTokenThreshold > 0) iDefTokenThreshold--;
            iOffensiveThreshold++;
        }
        iOffensiveThreshold++;
    }

    private void IncreaseOffense(bool b_Alot = false)
    {
        if (b_Alot)
        {
            if (iOffensiveThreshold > 0) iOffensiveThreshold--;
            iDefTokenThreshold++;
        }
        if (iOffensiveThreshold > 0) iOffensiveThreshold--;
    }

    private float UpdateProgressData (List<CreatureBase> creatures)
    {
        if (creatures.Count <= 0) return -1f;
        int iProgress = 0;
        foreach (CreatureBase creature in creatures) iProgress += creature.LaneProgress;

        if (creatures[0].Owner == this) I_Progress = iProgress;
        else if (creatures[0].Owner == Enemy) I_EnemyProgress = iProgress;
        return (float)iProgress / creatures.Count;
    }
}