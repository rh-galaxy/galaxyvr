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

    public string szBestName1;
    public int iBestScore1;
    public string szBestName2;
    public int iBestScore2;
    public string szBestName3;
    public int iBestScore3;
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
            oLevelList.Clear(); //rebuild list

            //parse result
            for (int i=0; i<szLines.Length; i++)
            {
                char[] szSeparator = { (char)32 };
                string[] szTokens = szLines[i].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (szTokens.Length != 12) continue;

                stLevel.szName = szTokens[0].Trim('\"');
                stLevel.bIsTime = szTokens[1].CompareTo("1")==0;
                stLevel.iLimit1 = int.Parse(szTokens[2]);
                stLevel.iLimit2 = int.Parse(szTokens[3]);
                stLevel.iLimit3 = int.Parse(szTokens[4]);
                stLevel.iScoreMs = int.Parse(szTokens[5]);

                stLevel.szBestName1 = szTokens[6];
                stLevel.iBestScore1 = int.Parse(szTokens[7]);
                stLevel.szBestName2 = szTokens[8];
                stLevel.iBestScore2 = int.Parse(szTokens[9]);
                stLevel.szBestName3 = szTokens[10];
                stLevel.iBestScore3 = int.Parse(szTokens[11]);

                oLevelList.Add(stLevel);
            }
        }
        bIsDone = true;
    }

    //probably unity is escaping the & and other chars so $_POST in php contained only one big element with everything
    // luckily we can build our own raw sender...
    UnityWebRequest CreateUnityWebRequest(string url, string param)
    {
        UnityWebRequest requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(param);
        UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
        //uH.contentType = "application/json"; //this is ignored?
        requestU.uploadHandler = uH;
        requestU.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        DownloadHandler dH = new DownloadHandlerBuffer();
        requestU.downloadHandler = dH;
        return requestU;
    }

    public IEnumerator SendHiscore(string i_szLevel, int i_iScoreMs, Replay i_oReplay)
    {
        bIsDone = false;
        byte[] bytes = i_oReplay.SaveToMem();
        string base64 = System.Convert.ToBase64String(bytes);
        int iCount = (int)(DateTime.Now - dtLastAccess).TotalSeconds;

        string url = WEB_HOST + "/achievements_post.php";
        string data= "LEVEL="+ i_szLevel + "&NAME="+ GameManager.szUser + "&USERID="+ GameManager.iUserID + "&COUNTER="+
            iCount + "&SCORE="+ i_iScoreMs + "&REPLAY="+ base64;

        www = CreateUnityWebRequest(url, data); //UnityWebRequest.Post(url, data); <- didn't work
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
        }
        bIsDone = true;
    }

    public IEnumerator GetReplay(string i_szLevel, string i_szName, Replay i_oResult)
    {
        bIsDone = false;

        string url = WEB_HOST + "/hiscore_getreplay2.php?Level=" + i_szLevel + "&Name=" + i_szName;
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
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

}
