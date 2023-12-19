using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct LiPart
{
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
}

public struct LevelInfo
{
    public string szName;
    public bool bIsTime;

    public LiPart info;
    public LiPart info2;

    public string szCreateor; //on user levels
}

public class HttpHiscore
{
    public static string WEB_HOST = "https://galaxy-forces-vr.com";

    public List<LevelInfo> oLevelList = new List<LevelInfo>();
    public bool bIsDone = false;

    UnityWebRequestAsyncOperation async;
    DateTime dtLastAccess; //when GetLimits() has run

    int iIsSteam = 1;

    public HttpHiscore()
    {

    }

    UnityWebRequest www;
    public IEnumerator GetLimits()
    {
        bIsDone = false;
        dtLastAccess = DateTime.Now; //set this before request

        string url = WEB_HOST + "/achievements_get2.php?User=" + UnityWebRequest.EscapeURL(GameManager.szUser) + "&UserId=" + UnityWebRequest.EscapeURL(GameManager.szUserID) + "&IsSteam=" + iIsSteam;
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;

            //retrieve results as text
            string szResult = www.downloadHandler.text;
            LevelInfo stLevel = new LevelInfo();

            string[] szLines = szResult.Split((char)10);
            oLevelList.Clear(); //rebuild list

            if (szLines.Length >= 2)
            {
                //parse result
                for (int i = 0; i < szLines.Length; i++)
                {
                    char[] szSeparator = { (char)32 };
                    string[] szTokens = szLines[i].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (szTokens.Length < 12) continue;

                    stLevel.szName = szTokens[0].Trim('\"');
                    stLevel.bIsTime = szTokens[1].CompareTo("1") == 0;
                    stLevel.info.iLimit1 = int.Parse(szTokens[2]);
                    stLevel.info.iLimit2 = int.Parse(szTokens[3]);
                    stLevel.info.iLimit3 = int.Parse(szTokens[4]);
                    stLevel.info.iBestScoreMs = int.Parse(szTokens[5]);
                    stLevel.info.iLastScoreMs = -1;
                    stLevel.info.iTotalPlaces = -1;
                    stLevel.info.iYourPlace = -1;

                    //handle spaces in names
                    char[] szSeparator2 = { (char)'\"' };
                    string[] szWithin = szLines[i].Trim('\r', '\n').Split(szSeparator2, StringSplitOptions.RemoveEmptyEntries);
                    stLevel.info.szWRName1 = szWithin[2].Trim(' ');
                    stLevel.info.iWRScore1 = int.Parse(szWithin[3]);
                    stLevel.info.szWRName2 = szWithin[4].Trim(' ');
                    stLevel.info.iWRScore2 = int.Parse(szWithin[5]);
                    stLevel.info.szWRName3 = szWithin[6].Trim(' ');

                    szTokens = szWithin[7].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                    stLevel.info.iWRScore3 = int.Parse(szTokens[0]);
                    if (szWithin.Length >= 9) stLevel.szCreateor = szWithin[8].Trim(' ');
                    else stLevel.szCreateor = "none";

                    if (szTokens.Length < 4) continue;
                    stLevel.info.szWRId1 = szTokens[1].Trim(' ');
                    stLevel.info.szWRId2 = szTokens[2].Trim(' ');
                    stLevel.info.szWRId3 = szTokens[3].Trim(' ');

                    if (szWithin.Length >= 10)
                    {
                        szTokens = szWithin[9].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                        if (szTokens.Length >= 2)
                        {
                            stLevel.info.iTotalPlaces = int.Parse(szTokens[0].Trim(' '));
                            stLevel.info.iYourPlace = int.Parse(szTokens[1].Trim(' '));
                        }
                    }

                    ///////////////////////////////////////////////////////////////////////////
                    {
                        i++; //mission and now race levels have 2 lines

                        szTokens = szLines[i].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                        if (szTokens.Length < 12) continue;

                        stLevel.szName = szTokens[0].Trim('\"');
                        stLevel.bIsTime = szTokens[1].CompareTo("1") == 0;
                        stLevel.info2.iLimit1 = int.Parse(szTokens[2]);
                        stLevel.info2.iLimit2 = int.Parse(szTokens[3]);
                        stLevel.info2.iLimit3 = int.Parse(szTokens[4]);
                        stLevel.info2.iBestScoreMs = int.Parse(szTokens[5]);
                        stLevel.info2.iLastScoreMs = -1;
                        stLevel.info2.iTotalPlaces = -1;
                        stLevel.info2.iYourPlace = -1;

                        //handle spaces in names
                        szWithin = szLines[i].Trim('\r', '\n').Split(szSeparator2, StringSplitOptions.RemoveEmptyEntries);
                        stLevel.info2.szWRName1 = szWithin[2].Trim(' ');
                        stLevel.info2.iWRScore1 = int.Parse(szWithin[3]);
                        stLevel.info2.szWRName2 = szWithin[4].Trim(' ');
                        stLevel.info2.iWRScore2 = int.Parse(szWithin[5]);
                        stLevel.info2.szWRName3 = szWithin[6].Trim(' ');

                        szTokens = szWithin[7].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                        stLevel.info2.iWRScore3 = int.Parse(szTokens[0]);
                        if (szWithin.Length >= 9) stLevel.szCreateor = szWithin[8].Trim(' ');
                        else stLevel.szCreateor = "none";

                        if (szTokens.Length < 4) continue;
                        stLevel.info2.szWRId1 = szTokens[1].Trim(' ');
                        stLevel.info2.szWRId2 = szTokens[2].Trim(' ');
                        stLevel.info2.szWRId3 = szTokens[3].Trim(' ');

                        if (szWithin.Length >= 10)
                        {
                            szTokens = szWithin[9].Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                            if (szTokens.Length >= 2)
                            {
                                stLevel.info2.iTotalPlaces = int.Parse(szTokens[0].Trim(' '));
                                stLevel.info2.iYourPlace = int.Parse(szTokens[1].Trim(' '));
                            }
                        }
                    }
                    ///////////////////////////////////////////////////////////////////////////

                    oLevelList.Add(stLevel);
                }
            }
        }

        www.Dispose();
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
        int iCount = (int)(DateTime.Now - dtLastAccess).TotalSeconds ^ 1467;

        string url = WEB_HOST + "/achievements_post.php";
        string data = "LEVEL=" + i_szLevel + "&NAME=" + UnityWebRequest.EscapeURL(GameManager.szUser) + "&USERID=" + UnityWebRequest.EscapeURL(GameManager.szUserID) + "&COUNTER=" +
            iCount + "&SCORE=" + i_iScoreMs + "&STEAM=" + iIsSteam + "&REPLAY=" + base64;

        www = CreateUnityWebRequest(url, data); //UnityWebRequest.Post(url, data); <- didn't work
        yield return www.SendWebRequest();

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;
        }
        www.Dispose();
        bIsDone = true;
    }
    public IEnumerator SendHiscore2(string i_szLevel, int i_iScoreMs, Replay i_oReplay)
    {
        bIsDone = false;
        byte[] bytes = i_oReplay.SaveToMem();
        string base64 = System.Convert.ToBase64String(bytes);
        int iCount = (int)(DateTime.Now - dtLastAccess).TotalSeconds ^ 1467;

        string url = WEB_HOST + "/achievements_post2.php";
        string data = "LEVEL=" + i_szLevel + "&NAME=" + UnityWebRequest.EscapeURL(GameManager.szUser) + "&USERID=" + UnityWebRequest.EscapeURL(GameManager.szUserID) + "&COUNTER=" +
            iCount + "&SCORE=" + i_iScoreMs + "&STEAM=" + iIsSteam + "&REPLAY=" + base64;

        www = CreateUnityWebRequest(url, data); //UnityWebRequest.Post(url, data); <- didn't work
        yield return www.SendWebRequest();

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;
        }
        www.Dispose();
        bIsDone = true;
    }

    public IEnumerator GetReplay(string i_szLevel, string i_szId, Replay i_oResult)
    {
        bIsDone = false;

        string url = WEB_HOST + "/hiscore_getreplay2.php?Level=" + i_szLevel + "&UserId=" + UnityWebRequest.EscapeURL(i_szId);
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;

            //retrieve results as text and convert it to binary
            byte[] bytes = System.Convert.FromBase64String(www.downloadHandler.text);

            i_oResult.LoadFromMem(bytes);
        }
        www.Dispose();
        bIsDone = true;
    }
    public IEnumerator GetReplay2(string i_szLevel, string i_szId, Replay i_oResult)
    {
        bIsDone = false;

        string url = WEB_HOST + "/hiscore_getreplay3.php?Level=" + i_szLevel + "&UserId=" + UnityWebRequest.EscapeURL(i_szId);
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;

            //retrieve results as text and convert it to binary
            byte[] bytes = System.Convert.FromBase64String(www.downloadHandler.text);

            i_oResult.LoadFromMem(bytes);
        }
        www.Dispose();
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

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }
        else
        {
            while (!www.downloadHandler.isDone)
                yield return null;

            if (!i_bBinary) szLevelTextData = www.downloadHandler.text;
            else aLevelBinaryData = www.downloadHandler.data;
        }
        www.Dispose();
        bIsDone = true;
    }

}
