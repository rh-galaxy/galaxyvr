﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Oculus.Platform;
using Steamworks;

public class GameManager : MonoBehaviour
{
    public bool bPrefereOculus = true; //hardcoded and set depending on which exe we compile!
    public static GameManager theGM = null;

    internal static bool bOculusDevicePresent = false;
    internal static bool bValveDevicePresent = false;
    internal static ulong iUserID = 1;
    internal static string szUser = "DebugUser";
    internal static bool bUserValid = true; //use debug user if no oculus user

    string szLastLevel = "";
    int iAchievementHellbentCounter = 0;

    AsyncOperation asyncLoad;
    AudioSource oASMusic;

    Replay oReplay = new Replay(); //create one replay... this is recycled during the session

    // 
    void Awake()
    {
        //singleton
        if (theGM == null)
        {
            theGM = this;
        }
        else if (theGM != this)
        {
            //enforce singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }

        //the rest is done once only...
        DontDestroyOnLoad(gameObject);

        bool bInited;
        if (bPrefereOculus)
            bInited = InitOculus();
        else
            bInited = InitValve();

        /**//*if(!bInited)
        {
            //no VR
            UnityEngine.Application.Quit();
        }*/

        GameLevel.theReplay = oReplay;
        oASMusic = GetComponent<AudioSource>();
    }

    //////start of valve specific code
    CGameID gameID;
    bool bSteamStatsValid = false;
    int iSteamStatsTotalRacePlayed;
    int iSteamStatsTotalMissionPlayed;
    int iSteamStatsTotalFuelBurnt;
    int iSteamStatsTotalEnemiesKilled;
    int iSteamStatsTotalShipsDestroyed;
    int iSteamStatsTotalBulletsFired;
    int iSteamStatsTotalMetersTravelled;
    int iSteamStatsLevelsPlayedBits1;
    int iSteamStatsLevelsPlayedBits2;

    protected Callback<UserStatsReceived_t> UserStatsReceived;

    bool InitValve()
    {
        try
        {
/**/            if (SteamAPI.RestartAppIfNecessary(/**/(AppId_t)480))
/**///also fix appid...txt
            {
                UnityEngine.Application.Quit();
                return false;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location.\n" + e, this);
            UnityEngine.Application.Quit();
            return false;
        }

        bool bInited = SteamAPI.Init();
        if (!bInited)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed.", this);
            UnityEngine.Application.Quit();
            return false;
        }

        // cache the GameID for use in the callbacks
        gameID = new CGameID(SteamUtils.GetAppID());
        UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        bool bSuccess = SteamUserStats.RequestCurrentStats();

        // get user name
        iUserID = SteamUser.GetSteamID().m_SteamID;
        /**/szUser = "Steam " + SteamFriends.GetPersonaName();

        bValveDevicePresent = true;
        return true;
    }

    private void OnDestroy()
    {
        if (!SteamManager.Initialized)
            return;

        SteamAPI.Shutdown();
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;

        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("Received stats and achievements from Steam\n");
                bSteamStatsValid = true;

                // load stats
                SteamUserStats.GetStat("TotalRacePlayed", out iSteamStatsTotalRacePlayed);
                SteamUserStats.GetStat("TotalMissionPlayed", out iSteamStatsTotalMissionPlayed);
                SteamUserStats.GetStat("TotalFuelBurnt", out iSteamStatsTotalFuelBurnt);
                SteamUserStats.GetStat("TotalEnemiesKilled", out iSteamStatsTotalEnemiesKilled);
                SteamUserStats.GetStat("TotalShipsDestroyed", out iSteamStatsTotalShipsDestroyed);
                SteamUserStats.GetStat("TotalBulletsFired", out iSteamStatsTotalBulletsFired);
                SteamUserStats.GetStat("TotalMetersTravelled", out iSteamStatsTotalMetersTravelled);
                SteamUserStats.GetStat("LevelsPlayedBits1", out iSteamStatsLevelsPlayedBits1);
                SteamUserStats.GetStat("LevelsPlayedBits2", out iSteamStatsLevelsPlayedBits2);
            }
            else
            {
                Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    void HandleValveAchievements()
    {
        if (!bSteamStatsValid) return;
        //handle achievements
        if (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel)
        {
            //finished any level with ok result

            //survivor achievement check
            if (GameLevel.theMap.player.bAchieveNoDamage)
                SteamUserStats.SetAchievement("Survivor");
            //hellbent achievement check
            if (szLastLevel.CompareTo(GameLevel.szLevel) != 0) iAchievementHellbentCounter = 1;
            else iAchievementHellbentCounter++;
            if (iAchievementHellbentCounter == 8)
                SteamUserStats.SetAchievement("Hellbent");
            //speedster achievement check
            if (GameLevel.theMap.player.bAchieveFullThrottle)
                SteamUserStats.SetAchievement("Speedster");
            //racer achievement check
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
            {
                iSteamStatsTotalRacePlayed++;
                if(iSteamStatsTotalRacePlayed>=12) SteamUserStats.SetAchievement("Racer");
                SteamUserStats.SetStat("TotalRacePlayed", iSteamStatsTotalRacePlayed);
            }
            //transporter achievement check (named loader)
            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
            {
                iSteamStatsTotalMissionPlayed++;
                if (iSteamStatsTotalMissionPlayed >= 12) SteamUserStats.SetAchievement("Loader");
                SteamUserStats.SetStat("TotalMissionPlayed", iSteamStatsTotalMissionPlayed);
            }
            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
            {
                //cargo beginner
                if (GameLevel.szLevel.CompareTo("1mission00") == 0)
                    SteamUserStats.SetAchievement("Cargo1");
                //cargo apprentice
                if (GameLevel.szLevel.CompareTo("1mission03") == 0)
                    SteamUserStats.SetAchievement("Cargo2");
                //cargo expert
                if (GameLevel.szLevel.CompareTo("1mission06") == 0 && GameLevel.theMap.player.iAchieveShipsDestroyed == 0)
                    SteamUserStats.SetAchievement("Cargo3");
                //cargo master
                if (GameLevel.szLevel.CompareTo("1mission09") == 0 && GameLevel.theMap.player.bAchieveNoDamage)
                    SteamUserStats.SetAchievement("Cargo4");
            }
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
            {
                //race beginner
                if (GameLevel.szLevel.CompareTo("2race00") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    SteamUserStats.SetAchievement("Race1");
                //race apprentice
                if (GameLevel.szLevel.CompareTo("2race03") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    SteamUserStats.SetAchievement("Race2");
                //race expert
                if (GameLevel.szLevel.CompareTo("2race06") == 0 && GameLevel.theMap.player.fTotalTime < 60.0f)
                    SteamUserStats.SetAchievement("Race3");
                //race master
                if (GameLevel.szLevel.CompareTo("2race10") == 0 && GameLevel.theMap.player.fTotalTime < 104.0f)
                    SteamUserStats.SetAchievement("Race4");
            }
            int bits = 0x00000001;
            if (GameLevel.iLevelIndex >= 31) iSteamStatsLevelsPlayedBits2 |= bits << (GameLevel.iLevelIndex-31);
            else iSteamStatsLevelsPlayedBits1 |= bits << GameLevel.iLevelIndex; //we use 31 bits in Bits1 and 24 bits in Bits2 (55 bits)
            if(iSteamStatsLevelsPlayedBits1 == 0x7fffffff && iSteamStatsLevelsPlayedBits2 == 0x00ffffff) SteamUserStats.SetAchievement("Galaxy55");
            SteamUserStats.SetStat("LevelsPlayedBits1", iSteamStatsLevelsPlayedBits1);
            SteamUserStats.SetStat("LevelsPlayedBits2", iSteamStatsLevelsPlayedBits2);
        }
        //fuelburner achievement check
        int iTemp = (int)GameLevel.theMap.player.fAchieveFuelBurnt;
        if (iTemp > 0)
        {
            iSteamStatsTotalFuelBurnt += iTemp;
            if (iSteamStatsTotalFuelBurnt >= 1200) SteamUserStats.SetAchievement("Fuelburner"); //20 minutes
            SteamUserStats.SetStat("TotalFuelBurnt", iSteamStatsTotalFuelBurnt);
        }
        //ravager achievement check
        iTemp = GameLevel.theMap.iAchieveEnemiesKilled;
        if (iTemp > 0)
        {
            iSteamStatsTotalEnemiesKilled += iTemp;
            if (iSteamStatsTotalEnemiesKilled >= 100) SteamUserStats.SetAchievement("Ravager");
            SteamUserStats.SetStat("TotalEnemiesKilled", iSteamStatsTotalEnemiesKilled);
        }
        //kamikaze achievement check (named doom)
        iTemp = GameLevel.theMap.player.iAchieveShipsDestroyed;
        if (iTemp > 0)
        {
            iSteamStatsTotalShipsDestroyed += iTemp;
            if (iSteamStatsTotalShipsDestroyed >= 100) SteamUserStats.SetAchievement("Doom");
            SteamUserStats.SetStat("TotalShipsDestroyed", iSteamStatsTotalShipsDestroyed);
        }
        //trigger achievement check
        iTemp = GameLevel.theMap.player.iAchieveBulletsFired;
        if (iTemp > 0)
        {
            iSteamStatsTotalBulletsFired += iTemp;
            if (iSteamStatsTotalBulletsFired >= 1000) SteamUserStats.SetAchievement("Trigger");
            SteamUserStats.SetStat("TotalBulletsFired", iSteamStatsTotalBulletsFired);
        }
        //hitchhiker42 achievement check
        iTemp = (int)GameLevel.theMap.player.fAchieveDistance;
        if (iTemp > 0)
        {
            iSteamStatsTotalMetersTravelled += iTemp * 5; //5m per unit is reasonable
            if (iSteamStatsTotalMetersTravelled >= 42000) SteamUserStats.SetAchievement("Hitchhiker42");
            SteamUserStats.SetStat("TotalMetersTravelled", iSteamStatsTotalMetersTravelled);
        }

        //send to server
        bool bSuccess = SteamUserStats.StoreStats();
    }
    //////end of valve specific code

    //////start of oculus specific code
    bool InitOculus()
    {
        if (XRDevice.model.StartsWith("Oculus Rift"))
        {
            //init Oculus SDK
            try
            {
                Core.AsyncInitialize("2005558116207772");
                Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
                Users.GetLoggedInUser().OnComplete(LoggedInUserCallback);
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
                UnityEngine.Application.Quit();
            }
            bOculusDevicePresent = true;
            return true;
        }
        return false;
    }

    void EntitlementCallback(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("Not entitled to play this game");
//removed quit while developing
/**///UnityEngine.Application.Quit();
        }
        else
        {
            //ok
            Debug.Log("You are entitled to play this game");
        }
    }

    void LoggedInUserCallback(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("No Oculus user");
//removed quit while developing
/**///UnityEngine.Application.Quit();
        }
        else
        {
            //save ID, and user name
            iUserID = msg.GetUser().ID;
            /**///szUser = "Oculus " + msg.GetUser().OculusID;
            /**/szUser = msg.GetUser().OculusID;
            Debug.Log("You are " + szUser);
            bUserValid = true;
        }
    }
    
    void HandleOculusAchievements()
    {
        //handle achievements
        if (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel)
        {
            //finished any level with ok result

            //survivor achievement check
            if (GameLevel.theMap.player.bAchieveNoDamage)
                Achievements.Unlock("Survivor");
            //hellbent achievement check
            if (szLastLevel.CompareTo(GameLevel.szLevel) != 0) iAchievementHellbentCounter = 1;
            else iAchievementHellbentCounter++;
            if (iAchievementHellbentCounter == 8)
                Achievements.Unlock("Hellbent");
            //speedster achievement check
            if (GameLevel.theMap.player.bAchieveFullThrottle)
                Achievements.Unlock("Speedster");
            //racer achievement check
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
                Achievements.AddCount("Racer", 1);
            //transporter achievement check (named loader)
            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
                Achievements.AddCount("Loader", 1);

            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
            {
                //cargo beginner
                if (GameLevel.szLevel.CompareTo("1mission00") == 0)
                    Achievements.Unlock("Cargo1");
                //cargo apprentice
                if (GameLevel.szLevel.CompareTo("1mission03") == 0)
                    Achievements.Unlock("Cargo2");
                //cargo expert
                if (GameLevel.szLevel.CompareTo("1mission06") == 0 && GameLevel.theMap.player.iAchieveShipsDestroyed == 0)
                    Achievements.Unlock("Cargo3");
                //cargo master
                if (GameLevel.szLevel.CompareTo("1mission09") == 0 && GameLevel.theMap.player.bAchieveNoDamage)
                    Achievements.Unlock("Cargo4");
            }
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
            {
                //race beginner
                if (GameLevel.szLevel.CompareTo("2race00") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    Achievements.Unlock("Race1");
                //race apprentice
                if (GameLevel.szLevel.CompareTo("2race03") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    Achievements.Unlock("Race2");
                //race expert
                if (GameLevel.szLevel.CompareTo("2race06") == 0 && GameLevel.theMap.player.fTotalTime < 60.0f)
                    Achievements.Unlock("Race3");
                //race master
                if (GameLevel.szLevel.CompareTo("2race10") == 0 && GameLevel.theMap.player.fTotalTime < 104.0f)
                    Achievements.Unlock("Race4");
            }
            string szBits = "0000000000000000000000000000000000000000000000000000000";
            char[] aBitsChars = szBits.ToCharArray();
            aBitsChars[GameLevel.iLevelIndex] = '1';
            string szBits2 = new string(aBitsChars, 0, aBitsChars.Length - 0);
            Achievements.AddFields("Galaxy55", szBits2);
        }
        //fuelburner achievement check
        int iTemp = (int)GameLevel.theMap.player.fAchieveFuelBurnt;
        if (iTemp > 0)
            Achievements.AddCount("Fuelburner", (ulong)iTemp);
        //ravager achievement check
        iTemp = GameLevel.theMap.iAchieveEnemiesKilled;
        if (iTemp > 0)
            Achievements.AddCount("Ravager", (ulong)iTemp);
        //kamikaze achievement check (named doom)
        iTemp = GameLevel.theMap.player.iAchieveShipsDestroyed;
        if (iTemp > 0)
            Achievements.AddCount("Doom", (ulong)iTemp);
        //trigger achievement check
        iTemp = GameLevel.theMap.player.iAchieveBulletsFired;
        if (iTemp > 0)
            Achievements.AddCount("Trigger", (ulong)iTemp);
        //hitchhiker42 achievement check
        iTemp = (int)GameLevel.theMap.player.fAchieveDistance;
        if (iTemp > 0)
            Achievements.AddCount("Hitchhiker42", (ulong)iTemp*5); //5m per unit is reasonable
    }
    //////end of oculus specific code

    LevelInfo stLevel;
    internal HttpHiscore oHigh = new HttpHiscore();

    bool bStartReplay = false;
    bool bLoadDone = false;
    int iLoadingMap = 0;
    int iState = -1;

    bool bMusicOn = true;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    int iCnt = 0;
    private void FixedUpdate()
    {
        iCnt++;
        if(bValveDevicePresent && (iCnt%50==0)) SteamAPI.RunCallbacks(); //run twice a sec

        if (iState == 7) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
    }

    void Update()
    {
        //pause if in oculus home universal menu
        // but for now (for debug purposes) keep the game running while XRDevice.userPresence!=Present
        if (bOculusDevicePresent)
        {
            if ((OVRManager.hasInputFocus && OVRManager.hasVrFocus) /**/|| (XRDevice.userPresence!=UserPresenceState.Present))
            {
                Time.timeScale = 1.0f;
                if (bMusicOn) oASMusic.UnPause();
            }
            else
            {
                Time.timeScale = 0.000001f;

                //also need to stop all sound
                if (bMusicOn) oASMusic.Pause();
                if (GameLevel.theMap!=null) GameLevel.theMap.player.StopSound();
                return;
            }
        }
        if (bMusicOn && !oASMusic.isPlaying) oASMusic.Play();
        else if (!bMusicOn && oASMusic.isPlaying) oASMusic.Pause();

        //the main state machine
        switch (iState)
        {
            case -1:
                //by use of the EditorAutoLoad script the main scene should be loaded first
                //and should be active here ("Scenes/GameStart")
                //set it in Unity->File->Scene Autoload
                iState++;
                break;
            case 0:
                //wait for oculus user id/name to be ready
                //if (bUserValid) iState++;
                /*couldn't play if no vr-support, just skip for now*/iState++;
                break;
            case 1:
                //running menu
                {
                    oASMusic.volume = 0.55f;

                    //these 3 are set in menu part 2, reset them here
                    Menu.bWorldBestReplay = false;
                    Menu.bYourBestReplay = false;
                    Menu.bLevelPlay = false;

                    bool bStart = Menu.bLevelSelected;
                    Menu.bLevelSelected = false; //reset when we have seen it

                    if (bStart)
                    {
                        //goto menu part 2 for the selected level
                        if (GameManager.bUserValid) //will be and must always be true
                        {
                            StartCoroutine(oHigh.GetLimits());
                            iState++;

                            //set in the above, but since StartCoroutine returns before it has a chanse
                            // to run we need to set it
                            oHigh.bIsDone = false;

                            //set default level info (in case we have network error)
                            stLevel = new LevelInfo();
                            stLevel.szName = GameLevel.szLevel.Substring(1);
                            stLevel.bIsTime = GameLevel.szLevel.StartsWith("2"); //not so good way of doing it but it's all we got
                            stLevel.iScoreMs = stLevel.iBestScore1 = stLevel.iBestScore2 = stLevel.iBestScore3 = -1;
                            stLevel.iLimit1 = stLevel.iLimit2 = stLevel.iLimit3 = -1;
                        }
                    }
                    break;
                }
            case 2:
                //wait for http reply (achievements_get.php)
                if(oHigh.bIsDone)
                {
                    string szLevelToLoad = GameLevel.szLevel.Substring(1);
                    for (int i = 0; i < oHigh.oLevelList.Count; i++)
                    {
                        if (szLevelToLoad.CompareTo(oHigh.oLevelList[i].szName) == 0) {
                            stLevel = oHigh.oLevelList[i];
                            break;
                        }
                    }
                    //Debug.Log("http loaded page: "+stLevel.szName + " isTime " + stLevel.bIsTime.ToString());

                    Menu.theMenu.SetLevelInfo(stLevel); //set our level info to menu, it will be displayed there

                    iState++;
                }
                //todo: handle network failure
                break;
            case 3:
                //menu part 2
                if(Menu.bLevelSelected)
                {
                    iState = 1; //goto menu part 1 since we have selected another level
                }

                if ((Menu.bWorldBestReplay && stLevel.iBestScore1 != -1) || (Menu.bYourBestReplay && stLevel.iScoreMs != -1))
                {
                    string szReplayName = null;
                    if (Menu.bYourBestReplay) szReplayName = GameManager.szUser;
                    if (Menu.bWorldBestReplay) szReplayName = stLevel.szBestName1;

                    StartCoroutine(oHigh.GetReplay(stLevel.szName, szReplayName, oReplay));
                    iState++; //load replay

                    //set in the above, but since StartCoroutine returns before it has a chanse
                    // to run we need to set it
                    oHigh.bIsDone = false;
                }
                else if(Menu.bLevelPlay)
                {
                    iState += 2; //go directly to load level
                }
                break;
            case 4:
                //menu part 2, while loading replay
                if (oHigh.bIsDone)
                {
                    bStartReplay = true;
                    iState++;
                }
                break;
            case 5:
                //begin loading the level (or replay), set this in state 3 later, now state 1
                //Debug.LogError("Begin load level");
                szToLoad = "Scenes/PlayGame";
                bLoadDone = false;
                bIsMapScene = true;
                bBeginMapLoading = false;
                iLoadingMap = 0;
                GameLevel.bMapLoaded = false;
                Menu.theMenu.SetWaiting(true);
                if (bStartReplay)
                {
                    oReplay.ResetBeforePlay();
                    GameLevel.bRunReplay = true;
                }
                else
                {
                    oReplay.Reset(); //reset before recording a new one during play
                    GameLevel.bRunReplay = false;
                }
                bStartReplay = false; //we have seen it
                StartCoroutine(LoadAsyncScene());
                iState++;
                break;
            case 6:
                //while loading level
                if (bBeginMapLoading)
                {
                    //Debug.Log("Load level 90%");
                    if (GameLevel.theMap.LoadInSegments(iLoadingMap++))
                    {
                        Debug.Log("LoadSeg Done");
                        iState++;
                        oASMusic.volume = 0.09f;
                    }
                }
                break;
            case 7:
                //running game
                {
                    bool bBackToMenu = !GameLevel.bMapLoaded;
                    if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.Escape)) //back to menu
                    {
                        bBackToMenu = true;
                    }
                    if (GameLevel.theMap.bGameOver)
                    {
                        bBackToMenu = true;

                        //////start of oculus specific code (achievements)
                        if (!GameLevel.bRunReplay && bOculusDevicePresent /**/&& XRDevice.userPresence != UserPresenceState.NotPresent)
                        {
                            HandleOculusAchievements();
                        }
                        //////end of oculus specific code
                        //////start of valve specific code
                        if (!GameLevel.bRunReplay && bValveDevicePresent /**/&& XRDevice.userPresence != UserPresenceState.NotPresent)
                        {
                            HandleValveAchievements();
                        }
                        //////end of valve specific code

                        //always update last level played
                        szLastLevel = GameLevel.szLevel;

                        int iScoreMs;
                        if (GameLevel.theMap.iLevelType == (int)LevelType.MAP_MISSION) iScoreMs = GameLevel.theMap.player.GetScore();
                        else iScoreMs = (int)(GameLevel.theMap.player.fTotalTime * 1000);

                        if (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel)
                        {
                            //finished ok, and with a new score or better than before, then send
                            if (stLevel.iScoreMs == -1 || (!stLevel.bIsTime && iScoreMs> stLevel.iScoreMs) ||
                                (stLevel.bIsTime && iScoreMs < stLevel.iScoreMs) )
                            {
                                StartCoroutine(oHigh.SendHiscore(szLastLevel.Substring(1), iScoreMs, oReplay));

                                //set in the above, but since StartCoroutine returns before it has a chanse
                                // to run we need to set it
                                oHigh.bIsDone = false;
                            }
                        }
                    }

                    if (bBackToMenu)
                    {
                        szToLoad = "Scenes/GameStart";
                        bLoadDone = false;
                        bIsMapScene = false;
                        StartCoroutine(LoadAsyncScene());
                        iState++;
                    }
                    break;
                }
            case 8:
                //while hiscore is being sent
                if (oHigh.bIsDone)
                    iState++;
                break;
            case 9:
                //while menu is loading
                if (bLoadDone)
                {
                    //restart at running start
                    iState = 1;
                    bLoadDone = false;
                }
                break;
        }
    }

    bool bIsMapScene = false;
    bool bBeginMapLoading = false;
    string szToLoad = "";
    IEnumerator LoadAsyncScene()
    {
        //the Application loads the scene in the background as the current scene runs
        // this is good for not freezing the view... finaly done by splitting the loading of the map mesh in parts

        asyncLoad = SceneManager.LoadSceneAsync(szToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        //wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // scene has loaded as much as possible, the last 10% can't be multi-threaded
            if (asyncLoad.progress >= 0.9f)
            {
                bBeginMapLoading = true;
                if (bIsMapScene && GameLevel.bMapLoaded || !bIsMapScene)
                    asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
        bLoadDone = asyncLoad.isDone;
    }

}
