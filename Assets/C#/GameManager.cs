﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Oculus.Platform;
#if !DISABLESTEAMWORKS //Add in Edit->Project Settings...->Player.Scripting Define Symbols [DISABLESTEAMWORKS]
using Steamworks; //when used: Edit->Project Settings...->Player.Scripting Backend must be [Mono] (not IL2CPP which should be used otherwise)
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager theGM = null;

    public CameraController theCameraHolder;

    internal static bool bOculusDevicePresent = false;
    internal static bool bValveDevicePresent = false;
    internal static ulong iUserID = 1;
    internal static string szUser = "DebugUser"; //use debug user if no VR user
    internal static bool bUserValid = false;
    internal static bool bNoHiscore = false;
    internal static bool bNoInternet = false;
    internal static bool bNoVR = false;

    string szLastLevel = "";
    int iAchievementHellbentCounter = 0;

    AsyncOperation asyncLoad;
    AudioSource oASMusic;

    Replay oReplay = new Replay(); //create one replay... this is recycled during the session

    //first code to run
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
            Destroy(gameObject); //<- this makes OnDestroy() be called and we don't want to deinit everything there
            return;
        }

        //the rest is done once only...
        DontDestroyOnLoad(gameObject);

        bool bInited = false;
#if !DISABLESTEAMWORKS
        this.enabled = true;
        bInited = InitValve();
#else
        bInited = InitOculus();
#endif

        if (!bInited)
        {
            //no VR
            //bUserValid = true;
            bNoHiscore = true;
            bNoVR = true;
            //XRSettings.enabled = false;
            Screen.SetResolution(1280, 720, true);
        }
        else
        {
            Screen.SetResolution(864, 960, false);
        }

        GameLevel.theReplay = oReplay;
        oASMusic = GetComponent<AudioSource>();
        oASMusic.Play();

        //this list keeps the last scores for each level for the entire game session, beginning with no score
        for (int i=0; i < aLastScore.Length; i++) aLastScore[i] = -1;

        UnityEngine.Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;

        theCameraHolder.InitForMenu();
    }

    //////start of valve specific code
#if !DISABLESTEAMWORKS
    float fBackTimerForViveController = 0.0f;

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
    protected Callback<UserStatsStored_t> UserStatsStored;
    protected Callback<UserAchievementStored_t> UserAchievementStored;

    bool InitValve()
    {
        try
        {
            if (SteamAPI.RestartAppIfNecessary((AppId_t)1035550))
            {
                Debug.LogError("[Steamworks.NET] SteamAPI.RestartAppIfNecessary returned false\n", this);
                return false;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location.\n" + e, this);
            return false;
        }

        bool bInited = SteamAPI.Init();
        if (!bInited)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed.", this);
            return false;
        }

        // cache the GameID for use in the callbacks
        gameID = new CGameID(SteamUtils.GetAppID());
        UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
        bool bSuccess = SteamUserStats.RequestCurrentStats();

        // get user name
        iUserID = SteamUser.GetSteamID().m_SteamID;
        szUser = "s_" + SteamFriends.GetPersonaName();
        bUserValid = true;

        if (XRDevice.isPresent)
            bValveDevicePresent = true;

        return bValveDevicePresent;
    }

    //cannot do this, because it is called when Awake is called a second time when loading another scene!
    // so OnDestroy() gets called for the new object that is then destroyed to enforce singleton
    //we only want to do this when the app exits
    /*private void OnDestroy()
    {
        if (!SteamManager.Initialized)
            return;

        SteamAPI.Shutdown();
    }*/

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
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)gameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
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
#endif
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

            //observe counteractive effect, enhances dark pixels levels for more banding as i see it
            //OVRManager.eyeTextureFormat = OVRManager.EyeTextureFormat.R11G11B10_FP;
            //OVRManager.eyeTextureFormat = OVRManager.EyeTextureFormat.R16G16B16A16_FP;

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
            
            UnityEngine.Application.Quit(); //it is possible to remove quit while developing
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
        }
        else
        {
            //save ID, and user name
            iUserID = msg.GetUser().ID;
            szUser = msg.GetUser().OculusID;
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
    int[] aLastScore = new int[400]; //a bit of a hack
    int iLastLevelIndex;
    bool bAutoSetLevelInfo = false;

    bool bStartReplay = false;
    bool bLoadDone = false;
    int iLoadingMap = 0;
    int iState = -3;

    bool bPause = false;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    private void FixedUpdate()
    {
        if (iState == 7) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
    }

    float fFadeFinishTime = 1.0f;
    float fFadeTimer = 0.0f;
    int iFade = 0; //0 no, 1 in from black, 2 out to black
    public Material oFadeMat;
    public GameObject oFadeBox;
    void UpdateFade()
    {
        if (iFade == 0)
            return;

        fFadeTimer += Time.deltaTime;

        float fProgress = fFadeTimer / fFadeFinishTime;
        float fFadeCurAlpha = fProgress;
        if(iFade == 1) fFadeCurAlpha = 1.0f - fProgress;
        oFadeMat.color = new Color(0, 0, 0, fFadeCurAlpha); 
        if (fProgress>0.999f)
        {
            iFade = 0;
            if(fFadeCurAlpha<0.1)
                oFadeBox.SetActive(false);
        }
    }
    public void StartFadeOut(float fTime)
    {
        fFadeFinishTime = fTime;
        fFadeTimer = 0.0f;
        iFade = 2;
        oFadeBox.SetActive(true);
        UpdateFade();
    }
    public void StartFadeIn(float fTime)
    {
        fFadeFinishTime = fTime;
        fFadeTimer = 0.0f;
        iFade = 1;
        oFadeBox.SetActive(true);
        UpdateFade();
    }

    //    Camera mainCam;
    void Update()
    {
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
            SteamAPI.RunCallbacks(); //must run every frame for some reason or garbage collector takes something and unity crashes
#endif
        if (Menu.bQuit)
        {
#if !DISABLESTEAMWORKS
            if (SteamManager.Initialized)
                SteamAPI.Shutdown();
#endif
#if UNITY_EDITOR
            //Application.Quit() does not work in the editor so
            // this need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        //pause if in oculus home universal menu
        // but for now (for debug purposes) keep the game running while XRDevice.userPresence!=Present
        bool bPauseNow = bPause; //no change below
        if (bOculusDevicePresent)
        {
            bPauseNow = (!OVRManager.hasInputFocus || !OVRManager.hasVrFocus) /*|| (XRDevice.userPresence!=UserPresenceState.Present)*/;
        }
        if (bValveDevicePresent)
        {
            bPauseNow = (XRDevice.userPresence == UserPresenceState.NotPresent);
        }
        /**/bPauseNow = false; //set to be able to play from editor without VR

        //save Camera.main whenever!null, because setting it disabled makes it null
//        if (Camera.main!=null) mainCam = Camera.main;

        //pause state change
        if (bPause != bPauseNow)
        {
            bPause = bPauseNow;
            if (bPauseNow)
            {
                Time.timeScale = 0.0f; //stops FixedUpdate

                //also need to stop all sound
                AudioListener.pause = true;

                //Update keeps running, but 
                // rendering must also be paused to pass oculus vrc
//                if (bOculusDevicePresent)
//                    mainCam.enabled = false;

                Menu.bPauseInput = true;
            }
            else
            {
                Time.timeScale = 1.0f;
                AudioListener.pause = false;

                //start rendering
//                if (bOculusDevicePresent)
//                    mainCam.enabled = true;

                Menu.bPauseInput = false;
            }
        }
        //to ignore input below, only way back is to unpause
        if (bPause) return;

        UpdateFade();

        //the main state machine
        switch (iState)
        {
            case -3:
                //by use of the EditorAutoLoad script the main scene should be loaded first
                //and should be active here ("Scenes/GameStart")
                Cursor.visible = false;
                //Screen.SetResolution(1280, 720, true);
                //^set 1280x720 when recording video, then let it run the 864x960 to get the default back to that (in Awake)
                iState++;
                break;
            case -2:
                //wait for oculus user id/name to be ready
                if (bUserValid || bNoHiscore)
                {
                    /**/StartFadeIn(0.5f);
                    iState++;
                }
                break;
            case -1:
                //get the progress, to see how many missions are unlocked
                if (!bNoInternet)
                {
                    StartCoroutine(oHigh.GetLimits());
                    //set in the above, but since StartCoroutine returns before it has a chanse
                    // to run we need to set it
                    oHigh.bIsDone = false;
                }
                else
                {
                    oHigh.bIsDone = true;
                }
                iState++;

                break;
            case 0:
                //wait for http reply (achievements_get.php)
                if (oHigh.bIsDone)
                {
                    int iMissionsFinished = 0;
                    for (int i = 0; i < oHigh.oLevelList.Count; i++)
                    {
                        stLevel = oHigh.oLevelList[i];
                        if (!stLevel.bIsTime)
                        {
                            if (stLevel.iBestScoreMs != -1) iMissionsFinished++;
                        }
                    }
                    if(oHigh.oLevelList.Count==0) bNoInternet = true; //set this so no further attempts are made at accessing the internet

                    int iToUnlock = (int)(iMissionsFinished * 1.35f) + 1;
                    if (bNoInternet || bNoHiscore) iToUnlock = 30; //unlock everything
                    Debug.Log("http loadinfo: finished " + iMissionsFinished + " unlocked " + iToUnlock);
                    Menu.theMenu.SetMissionUnlock(iToUnlock);
                    if(!bNoHiscore) Menu.theMenu.InitLevelRanking();
                    iState++;
                }
                break;
            case 1:
                //running menu
                {
                    //back at top level should result in this, but not working:
                    //if (bOculusDevicePresent && Input.GetKey(KeyCode.JoystickButton6))
                    //    OVRManager.PlatformUIConfirmQuit();

                    oASMusic.volume = 0.40f;
                    //oASMusic.volume = 0.00f;

                    //these 5 are set in menu part 2, reset them here
                    Menu.bWorldBestReplay1 = false;
                    Menu.bWorldBestReplay2 = false;
                    Menu.bWorldBestReplay3 = false;
                    Menu.bYourBestReplay = false;
                    Menu.bLevelPlay = false;

                    if (Menu.bLevelSelected || bAutoSetLevelInfo)
                    {
                        Menu.bLevelSelected = false; //reset when we have seen it
                        bAutoSetLevelInfo = false;

                        //goto menu part 2 for the selected level
                        iState++;
                        //^we already have the info from state 0, no need to load it again
                    }
                    break;
                }
            case 2:
                //we have the info already
                if (true)
                {
                    //set default level info (in case we have network error)
                    stLevel = new LevelInfo();
                    if(GameLevel.iLevelIndex>=200) stLevel.szName = GameLevel.szLevel;
                    else stLevel.szName = GameLevel.szLevel.Substring(1);
                    //stLevel.bIsTime = GameLevel.szLevel.StartsWith("2"); //not so good way of doing it but it's all we got
                    //^this is now set in SetLevelInfo, read from file
                    stLevel.iBestScoreMs = stLevel.iLastScoreMs = -1;
                    stLevel.iWRScore1 = stLevel.iWRScore2 = stLevel.iWRScore3 = -1;
                    stLevel.iLimit1 = stLevel.iLimit2 = stLevel.iLimit3 = -1;
                    stLevel.szWRName1 = "_None"; stLevel.szWRName2 = "_None"; stLevel.szWRName3 = "_None";

                    string szLevelToLoad = stLevel.szName;
                    if (GameLevel.iLevelIndex >= 200)
                    {
                        stLevel.iLastScoreMs = aLastScore[GameLevel.iLevelIndex];
                        iLastLevelIndex = GameLevel.iLevelIndex;
                    }
                    else
                    {
                        for (int i = 0; i < oHigh.oLevelList.Count; i++)
                        {
                            if (szLevelToLoad.CompareTo(oHigh.oLevelList[i].szName) == 0)
                            {
                                stLevel = oHigh.oLevelList[i];
                                stLevel.iLastScoreMs = aLastScore[i];
                                iLastLevelIndex = i;
                                break;
                            }
                        }
                    }

                    Menu.theMenu.SetLevelInfo(stLevel, false); //set our level info to menu, it will be displayed there

                    iState++;
                }
                break;
            case 3:
                //menu part 2
                if(Menu.bLevelSelected)
                {
                    iState = 1; //goto menu part 1 since we have selected another level
                }
                if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.JoystickButton7) || Input.GetKey(KeyCode.Escape)
                    || Menu.bLevelUnSelected )
                {
                    Menu.bLevelUnSelected = false;
                    iState = 1; //goto menu part 1 (back)
                    Menu.theMenu.SetLevelInfo(stLevel, true); //stLevel not used
                }

                //if bNoInternet is true this will not be possible be design
                if ((Menu.bWorldBestReplay1 && stLevel.iWRScore1 != -1)
                    || (Menu.bWorldBestReplay2 && stLevel.iWRScore2 != -1)
                    || (Menu.bWorldBestReplay3 && stLevel.iWRScore3 != -1)
                    || (Menu.bYourBestReplay && stLevel.iBestScoreMs != -1))
                {
                    string szReplayName = null;
                    if (Menu.bYourBestReplay) szReplayName = GameManager.szUser;
                    if (Menu.bWorldBestReplay1) szReplayName = stLevel.szWRName1;
                    if (Menu.bWorldBestReplay2) szReplayName = stLevel.szWRName2;
                    if (Menu.bWorldBestReplay3) szReplayName = stLevel.szWRName3;

                    StartCoroutine(oHigh.GetReplay(stLevel.szName, szReplayName, oReplay));
                    iState++; //load replay
                    /**/StartFadeOut(0.3f);

                    //set in the above, but since StartCoroutine returns before it has a chanse
                    // to run we need to set it
                    oHigh.bIsDone = false;
                }
                else if(Menu.bLevelPlay)
                {
                    iState += 2; //go directly to load level
                    /**/StartFadeOut(0.3f);
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
                    if (iLoadingMap <= 10) GameLevel.theMap.LoadBegin(iLoadingMap);
                    else if (GameLevel.theMap.LoadDone())
                    {
                        Debug.Log("Load map segments Done");
                        iState++;
                        //oASMusic.volume = 0.09f;
                        /**/oASMusic.volume = 0.00f;
                    }
                    iLoadingMap++;
                }
                break;
            case 7:
                //running game
                {
                    bool bBackToMenu = !GameLevel.bMapLoaded;

                    //valve, back is considered when both grip are held for 5 sec
#if !DISABLESTEAMWORKS
                    float fTrg1 = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryHandTrigger");         //axis 11   left grip trigger on valve (and oculus touch)
                    float fTrg2 = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryHandTrigger");       //axis 12   right grip trigger on valve (and oculus touch)
                    if (fTrg1 > 0.8 && fTrg2 > 0.8)
                    {
                        fBackTimerForViveController += Time.deltaTime;
                        if (fBackTimerForViveController>4.0)
                        {
                            fBackTimerForViveController = 0;
                            bBackToMenu = true;
                            bAutoSetLevelInfo = true; //causes the menu to open up the levelinfo for this last played level
                        }
                    } else fBackTimerForViveController = 0.0f;
#endif

                    if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.Escape)) //back to menu
                    {
                        bBackToMenu = true;
                        bAutoSetLevelInfo = true; //causes the menu to open up the levelinfo for this last played level
                    }
                    if (GameLevel.theMap.bGameOver)
                    {
                        bBackToMenu = true;
                        bAutoSetLevelInfo = true; //causes the menu to open up the levelinfo for this last played level

                        //if (!bNoInternet)
                        {
                            if(!GameLevel.bRunReplay && iLastLevelIndex < 200)
                            {
                                //////start of oculus specific code (achievements)
                                if (bOculusDevicePresent /**/&& XRDevice.userPresence != UserPresenceState.NotPresent)
                                {
                                    HandleOculusAchievements();
                                }
                                //////end of oculus specific code
                                //////start of valve specific code
#if !DISABLESTEAMWORKS
                                if (bUserValid /*&& bValveDevicePresent*/) //allow non VR mode to set steam achievements
                                {
                                    HandleValveAchievements();
                                }
#endif
                                //////end of valve specific code
                            }
                        }

                        //get score from GameLevel
                        int iScoreMs;
                        if (GameLevel.theMap.iLevelType == (int)LevelType.MAP_MISSION) iScoreMs = GameLevel.theMap.player.GetScore();
                        else iScoreMs = (int)(GameLevel.theMap.player.fTotalTime * 1000);
                        if (!GameLevel.bRunReplay) aLastScore[iLastLevelIndex] = iScoreMs;

                        if (!bNoHiscore && !bNoInternet && iLastLevelIndex<200)
                        {
                            //always update last level played
                            szLastLevel = GameLevel.szLevel;

                            if (!GameLevel.bRunReplay && (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel))
                            {
                                //protect against huge replays (medium blob in server db is 16MB but no need to support that large)
                                if (oReplay.GetSize() < 4 * 1024 * 1024) //at 200 bytes per sec this is ~6 hours, and normal rate is ~100 Bps.
                                {
                                    //finished ok, and with a new score or better than before, then send
                                    if (stLevel.iBestScoreMs == -1 || (!stLevel.bIsTime && iScoreMs > stLevel.iBestScoreMs) ||
                                        (stLevel.bIsTime && iScoreMs < stLevel.iBestScoreMs))
                                    {
                                        StartCoroutine(oHigh.SendHiscore(szLastLevel.Substring(1), iScoreMs, oReplay));

                                        //set in the above, but since StartCoroutine returns before it has a chanse
                                        // to run we need to set it
                                        oHigh.bIsDone = false;
                                    }
                                }
                            }
                        }
                    }

                    if (bBackToMenu)
                    {
                        theCameraHolder.InitForMenu();
                        /**/StartFadeOut(0.3f);
                        iState++;
                    }
                    break;
                }
            case 8:
                if (iFade==0) //fading done?
                {
                    szToLoad = "Scenes/GameStart";
                    bLoadDone = false;
                    bIsMapScene = false;
                    StartCoroutine(LoadAsyncScene());
                    iState++;
                }
                break;
            case 9:
                //while hiscore is being sent
                if (oHigh.bIsDone)
                    iState++;
                break;
            case 10:
                //while menu is loading
                if (bLoadDone)
                {
                    //restart at running start
                    iState = -2;
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
        // this is good for not freezing the view... done by separating some work to a thread
        // and having the rest split in ~7ms jobs

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
