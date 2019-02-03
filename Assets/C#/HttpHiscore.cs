using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct LevelInfo
{
    public string szName;
    public bool bIsTime;
    public int iLimit1, iLimit2, iLimit3;
    public int iScoreMs;
}

public class HttpHiscore
{
    const string WEB_HOST = "http://galaxy-forces-vr.com";

    public List<LevelInfo> oLevelList = new List<LevelInfo>();
    public bool bIsDone = false;

    UnityWebRequestAsyncOperation async;
    DateTime dtLastAccess; //when GetLimits() has run

    public HttpHiscore()
    {

    }

    UnityWebRequest www;
    public IEnumerator GetLimits()
    {
        bIsDone = false;
        dtLastAccess = DateTime.Now; //set this before request

        string url = WEB_HOST + "/achievements_get.php?User=" + GameManager.szUser + "&UserId=" + GameManager.iUserID;
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //retrieve results as text
            string szResult = www.downloadHandler.text;
            LevelInfo stLevel = new LevelInfo();

            string[] szLines = szResult.Split((char)10);

            //parse result
            for(int i=0; i<szLines.Length; i++)
            {
                char[] szSeparator = { (char)32 };
                string[] szTokens = szLines[i].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (szTokens.Length != 6) continue;

                stLevel.szName = szTokens[0].Trim('\"');
                stLevel.bIsTime = szTokens[1].CompareTo("1")==0;
                stLevel.iLimit1 = int.Parse(szTokens[2]);
                stLevel.iLimit2 = int.Parse(szTokens[3]);
                stLevel.iLimit3 = int.Parse(szTokens[4]);
                stLevel.iScoreMs = int.Parse(szTokens[5]);

                oLevelList.Add(stLevel);
            }
        }
        bIsDone = true;
    }

/*    public LevelInfo GetLevelLimits(string szLevel)
    {
        LevelInfo stLevel = new LevelInfo();
        if (!GameManager.bUserValid) return stLevel;
        StartCoroutine(GetLimits());

        for (int i = 0; i < oLevelList.Count; i++)
        {
            stLevel = oLevelList[i];
            if(szLevel.CompareTo(stLevel.szName)==0) return stLevel;
        }

        return stLevel;
    }
*/
}
