﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct LevelInfo
{
    public string szName;
    public bool bIsTime;
    public int iLimit1, iLimit2, iLimit3;
    public int iLastScoreMs; //your last
    public int iBestScoreMs; //your best

    public string szWRId1;
    public string szWRName1;
    public int iWRScore1;
    public string szWRId2;
    public string szWRName2;
    public int iWRScore2;
    public string szWRId3;
    public string szWRName3;
    public int iWRScore3;

    public int iTotalPlaces;
    public int iYourPlace;

    public string szCreateor; //on user levels
}

public class HttpHiscore
{
#if UNITY_EDITOR
    public static string WEB_HOST = "https://galaxy-forces-vr.com"; //access from unity editor
#else
    public static string WEB_HOST = ""; //local access from code already on web server
#endif

    public List<LevelInfo> oLevelList = new List<LevelInfo>();
    public bool bIsDone = false;

    UnityWebRequestAsyncOperation async;
    DateTime dtLastAccess; //when GetLimits() has run

    int iIsSteam = 0;

    public HttpHiscore()
    {

    }

    UnityWebRequest www;

    public IEnumerator GetReplay(string i_szLevel, string i_szId, bool isQuest, Replay i_oResult)
    {
        bIsDone = false;
        
        string url = WEB_HOST + "/hiscore_getreplay2.php?Level=" + i_szLevel + "&UserId=" + UnityWebRequest.EscapeURL(i_szId);
        if (isQuest) url = WEB_HOST + "/hiscore_getreplay2_quest.php?Level=" + i_szLevel + "&UserId=" + UnityWebRequest.EscapeURL(i_szId);

        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text and convert it to binary
            byte[] bytes = System.Convert.FromBase64String(www.downloadHandler.text);

            i_oResult.LoadFromMem(bytes);
        }
        bIsDone = true;
    }

    internal string szLevelTextData;
    internal byte[] aLevelBinaryData;
    public IEnumerator GetLevelFile(string i_szFilename, bool i_bBinary)
    {
        bIsDone = false;

        string url = WEB_HOST + "/user_levels/" + i_szFilename;
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (!i_bBinary) szLevelTextData = www.downloadHandler.text;
            else aLevelBinaryData = www.downloadHandler.data;
        }
        bIsDone = true;
    }

}
