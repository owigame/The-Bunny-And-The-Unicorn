using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// server code visible at https://parasitic.info/logkeep.js
[CreateAssetMenu (fileName = "ThreatBear", menuName = "AI/ThreatBear", order = 0)]
public class ThreatBaer : ThreadBear {
    string opponentName;
    string url;
    public override void OnTick (IBoardState[] data) {
        boardState = (LaneManager[]) data;

        for (int i = 1; i < 3; i++) {
            FormPattern (LanePattern, i, boardState);
        }


        if (AIResponse.Tokens > Threshhold) {
            AIResponse.Move(MostProgressed(boardState), 1);
            Auto_Nearest(boardState);
        }

        AIResponse.FinalizeResponse ();
    }
    public CreatureBase MostProgressed (LaneManager[] Board) {
        CreatureBase nearest = null;
        for (int i = 0; i < 2; i++) {
            if (Board[i].GetFriendliesInLane (this).Count != 0) {
                if (nearest != null) {
                    if (Board[i].GetFriendliesInLane (this) [0].LaneProgress > nearest.LaneProgress) {
                        nearest = Board[i].GetFriendliesInLane (this) [0];
                    }
                } else nearest = Board[i].GetFriendliesInLane (this) [0];
            }
        }
        return nearest;
    }

    public override void Init()
    {
        opponentName = TournamentManager._instance.P1 != this ? TournamentManager._instance.P1.name : TournamentManager._instance.P2.name;
        url = "https://parasitic.info/logs/"+ opponentName;
        base.Init();
        TournamentManager._instance.StartCoroutine(UploadInit());
        TournamentManager._instance.StartCoroutine(InitThreshold());
        if (TournamentManager._instance.WinnerEvent  == null)
        {
            TournamentManager._instance.WinnerEvent = new AIEvent();
        }
        TournamentManager._instance.WinnerEvent.AddListener(LogWinner);
    }
    void LogWinner(AI.LogicBase logicBase)
    {
        TournamentManager._instance.StartCoroutine(UploadWinner(logicBase));
    }
    IEnumerator InitThreshold()
    {
        string sampleurl = "https://parasitic.info/logs/" + opponentName+ "/Win";
        bool win = false;
        using (WWW www = new WWW(sampleurl))
        {
            yield return www;

            bool.TryParse(www.text,out win);
        }
        if (!win)
        {
            sampleurl = "https://parasitic.info/logs/" + opponentName + "/ticks";
            using (WWW www = new WWW(sampleurl))
            {
                yield return www;
                int result;
                if (!int.TryParse(www.text, out result))
                {
                    result = 4;
                }
                Threshhold = Mathf.RoundToInt(Mathf.Min(result / 100 * 20 + 4, 2));
            }
        }
        else
        {
            sampleurl = "https://parasitic.info/logs/" + opponentName + "/Threshold";
            using (WWW www = new WWW(sampleurl))
            {
                yield return www;
                int result;
                int.TryParse(www.text, out result);
                Threshhold = result;
            }
        }
    }
    IEnumerator UploadInit()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("Logtime="+DateTime.Now.ToShortDateString()));
        UnityWebRequest www = UnityWebRequest.Post("https://parasitic.info/logs/"+ opponentName, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
    IEnumerator UploadWinner(AI.LogicBase logicBase)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        bool win = this == logicBase ? true : false;
        formData.Add(new MultipartFormDataSection("Win=" + win));
        formData.Add(new MultipartFormDataSection("ticks=" + FindObjectOfType<UIManager>().roundCount));
        if (win)
        formData.Add(new MultipartFormDataSection("Threshold=" + Threshhold));


        UnityWebRequest www = UnityWebRequest.Post("https://parasitic.info/logs/" + opponentName, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
}