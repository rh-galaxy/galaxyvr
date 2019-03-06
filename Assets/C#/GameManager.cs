using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Oculus.Platform;
#if !DISABLESTEAMWORKS //Add in Edit->Project Settings...->Player.Scripting Define Symbols [DISABLESTEAMWORKS]
using Steamworks; //when used: Edit->Project Settings...->Player.Scripting Backend must be [Mono] (not IL2CPP which should be used otherwise)
#endif

using VRTK;

public class GameManager : MonoBehaviour
{
    //VRTK//////////////////////////////////
#if !DISABLESTEAMWORKS
    public VRTK_ControllerEvents controllerEvents;

    public bool bTrigger = false; //accelerate
    public bool bGrip = false; //fire
    public bool bButton1 = false; //fire
    public bool bButton2 = false; 
    public bool bLeft = false, bRight = false;
    public bool bUp = false, bDown = false;
    public bool bStart = false; //back, is this menu button or system menu button?
    public bool bStartSeen = false;

    private void OnEnable()
    {
        controllerEvents = (controllerEvents == null ? GetComponent<VRTK_ControllerEvents>() : controllerEvents);
        if (controllerEvents == null)
        {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_ControllerEvents_ListenerExample", "VRTK_ControllerEvents", "the same"));
            return;
        }

        //Setup controller event listeners
        controllerEvents.TriggerPressed += DoTriggerPressed;
        controllerEvents.TriggerReleased += DoTriggerReleased;

        controllerEvents.GripPressed += DoGripPressed;
        controllerEvents.GripReleased += DoGripReleased;

        controllerEvents.TouchpadPressed += DoTouchpadPressed;
        controllerEvents.TouchpadReleased += DoTouchpadReleased;
        controllerEvents.TouchpadAxisChanged += DoTouchpadAxisChanged;
        controllerEvents.TouchpadTwoPressed += DoTouchpadTwoPressed;
        controllerEvents.TouchpadTwoReleased += DoTouchpadTwoReleased;
        controllerEvents.TouchpadTwoAxisChanged += DoTouchpadTwoAxisChanged;
        controllerEvents.TouchpadSenseAxisChanged += DoTouchpadSenseAxisChanged;

        controllerEvents.ButtonOnePressed += DoButtonOnePressed;
        controllerEvents.ButtonOneReleased += DoButtonOneReleased;

        controllerEvents.ButtonTwoPressed += DoButtonTwoPressed;
        controllerEvents.ButtonTwoReleased += DoButtonTwoReleased;

        controllerEvents.StartMenuPressed += DoStartMenuPressed;
        controllerEvents.StartMenuReleased += DoStartMenuReleased;
    }

    private void OnDisable()
    {
        if (controllerEvents != null)
        {
            controllerEvents.TriggerPressed -= DoTriggerPressed;
            controllerEvents.TriggerReleased -= DoTriggerReleased;

            controllerEvents.GripPressed -= DoGripPressed;
            controllerEvents.GripReleased -= DoGripReleased;

            controllerEvents.TouchpadPressed -= DoTouchpadPressed;
            controllerEvents.TouchpadReleased -= DoTouchpadReleased;
            controllerEvents.TouchpadAxisChanged -= DoTouchpadAxisChanged;
            controllerEvents.TouchpadTwoPressed -= DoTouchpadTwoPressed;
            controllerEvents.TouchpadTwoReleased -= DoTouchpadTwoReleased;
            controllerEvents.TouchpadTwoAxisChanged -= DoTouchpadTwoAxisChanged;
            controllerEvents.TouchpadSenseAxisChanged -= DoTouchpadSenseAxisChanged;

            controllerEvents.ButtonOnePressed -= DoButtonOnePressed;
            controllerEvents.ButtonOneReleased -= DoButtonOneReleased;

            controllerEvents.ButtonTwoPressed -= DoButtonTwoPressed;
            controllerEvents.ButtonTwoReleased -= DoButtonTwoReleased;

            controllerEvents.StartMenuPressed -= DoStartMenuPressed;
            controllerEvents.StartMenuReleased -= DoStartMenuReleased;
        }
    }

    private void LateUpdate()
    {
    }

    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        string debugString = "Controller on index '" + index + "' " + button + " has been " + action
                             + " with a pressure of " + e.buttonPressure + " / Primary Touchpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)" + " / Secondary Touchpad axis at: " + e.touchpadTwoAxis + " (" + e.touchpadTwoAngle + " degrees)";
        VRTK_Logger.Info(debugString);
    }

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        bTrigger = true;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "pressed", e);
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        bTrigger = false;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "released", e);
    }

    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        bGrip = true;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "pressed", e);
    }

    private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {
        bGrip = false;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "released", e);
    }

    private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        bLeft = e.touchpadAxis.x < -0.3;
        bRight = e.touchpadAxis.x > 0.3;
        bDown = e.touchpadAxis.y > 0.5;
        //e.touchpadAxis
        //e.touchpadTwoAxis
        //bLeft, bRight;
        //bUp, bDown;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "pressed down", e);
    }

    private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "released", e);
    }

    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        bLeft = e.touchpadAxis.x < -0.3;
        bRight = e.touchpadAxis.x > 0.3;
        bDown = e.touchpadAxis.y > 0.5;
        //e.touchpadAxis
        //e.touchpadTwoAxis
        //bLeft, bRight;
        //bUp, bDown;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "axis changed", e);
    }

    private void DoTouchpadTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        bLeft = e.touchpadTwoAxis.x < -0.3;
        bRight = e.touchpadTwoAxis.x > 0.3;
        bDown = e.touchpadTwoAxis.y > 0.5;
        //e.touchpadAxis
        //e.touchpadTwoAxis
        //bLeft, bRight;
        //bUp, bDown;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "pressed down", e);
    }

    private void DoTouchpadTwoReleased(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "released", e);
    }

    private void DoTouchpadTwoAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        bLeft = e.touchpadTwoAxis.x < -0.3;
        bRight = e.touchpadTwoAxis.x > 0.3;
        bDown = e.touchpadTwoAxis.y > 0.5;
        //e.touchpadAxis
        //e.touchpadTwoAxis
        //bLeft, bRight;
        //bUp, bDown;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "axis changed", e);
    }

    private void DoTouchpadSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "sense axis changed", e);
    }

    private void DoButtonOnePressed(object sender, ControllerInteractionEventArgs e)
    {
        bButton1 = true;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "pressed down", e);
    }

    private void DoButtonOneReleased(object sender, ControllerInteractionEventArgs e)
    {
        bButton1 = false;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "released", e);
    }

    private void DoButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
    {
        bButton2 = true;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "pressed down", e);
    }

    private void DoButtonTwoReleased(object sender, ControllerInteractionEventArgs e)
    {
        bButton2 = false;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "released", e);
    }

    private void DoStartMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        bStart = true;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "pressed down", e);
    }

    private void DoStartMenuReleased(object sender, ControllerInteractionEventArgs e)
    {
        bStart = false;

        DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "released", e);
    }
#endif
    //end VRTK//////////////////////////////

    public static GameManager theGM = null;

    internal static bool bOculusDevicePresent = false;
    internal static bool bValveDevicePresent = false;
    internal static ulong iUserID = 1;
    internal static string szUser = "DebugUser"; //use debug user if no VR user
    internal static bool bUserValid = false;

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
            Destroy(gameObject); //<- this makes OnDestroy() be called and we don't want to deinit everything there
            return;
        }

        //the rest is done once only...
        DontDestroyOnLoad(gameObject);

        bool bInited;
#if !DISABLESTEAMWORKS
        this.enabled = true;
        bInited = InitValve();
#else
        bInited = InitOculus();
#endif

        /**//*if(!bInited)
        {
            //no VR
            UnityEngine.Application.Quit();
        }*/
        if(!bOculusDevicePresent && !bValveDevicePresent) bUserValid = true; //'DebugUser'

        GameLevel.theReplay = oReplay;
        oASMusic = GetComponent<AudioSource>();
        if(bMusicOn) oASMusic.Play();
    }

    //////start of valve specific code
#if !DISABLESTEAMWORKS
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
/**///                UnityEngine.Application.Quit();
                return false;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location.\n" + e, this);
/**///            UnityEngine.Application.Quit();
            return false;
        }

        bool bInited = SteamAPI.Init();
        if (!bInited)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed.", this);
/**///            UnityEngine.Application.Quit();
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
            /**///szUser = "o_" + msg.GetUser().OculusID;
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

    bool bStartReplay = false;
    bool bLoadDone = false;
    int iLoadingMap = 0;
    int iState = -3;

    bool bPause = false;
    bool bMusicOn = true;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    private void FixedUpdate()
    {
        if (iState == 7) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
    }

    void Update()
    {
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
            SteamAPI.RunCallbacks(); //must run every frame for some reason or garbage collector takes something and unity crashes
#endif
        if(Menu.bQuit)
        {
#if !DISABLESTEAMWORKS
            if (SteamManager.Initialized)
                SteamAPI.Shutdown();
#endif
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
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
            if ((OVRManager.hasInputFocus && OVRManager.hasVrFocus) /**/|| (XRDevice.userPresence!=UserPresenceState.Present))
            {
                bPauseNow = false;
            }
            else
            {
                bPauseNow = true;
            }
        }
        if (bValveDevicePresent)
        {
#if !DISABLESTEAMWORKS
            /*if (!bStartSeen && bStart)
            {
                bStartSeen = true;
                bPauseNow = !bPause;
            }
            if (!bStart)
            {
                bStartSeen = false;
            }*/
#endif
        }

        //pause state change
        if (bPause != bPauseNow)
        {
            bPause = bPauseNow;
            if(bPauseNow)
            {
                Time.timeScale = 0.0f;

                //also need to stop all sound
                if (bMusicOn) oASMusic.Pause();
                if (GameLevel.theMap != null) GameLevel.theMap.player.StopSound();
                
                //Update keeps running, but FixedUpdate stops
            }
            else
            {
                Time.timeScale = 1.0f;
                if (bMusicOn) oASMusic.UnPause();
            }
        }

        //the main state machine
        switch (iState)
        {
            case -3:
                //by use of the EditorAutoLoad script the main scene should be loaded first
                //and should be active here ("Scenes/GameStart")
                //Screen.SetResolution(1280, 720, true);
                //Screen.SetResolution(864, 960, false);
                //^set 1280x720 when recording video, then run the 864x960 to get the default back to that (bug in unity)
                iState++;
                break;
            case -2:
                //wait for oculus user id/name to be ready
                if (bUserValid) iState++;
                break;
            case -1:
                //get the progress, to see how many missions are unlocked
                StartCoroutine(oHigh.GetLimits());
                iState++;

                //set in the above, but since StartCoroutine returns before it has a chanse
                // to run we need to set it
                oHigh.bIsDone = false;

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
                            if (stLevel.iScoreMs != -1) iMissionsFinished++;
                        }
                    }
                    Debug.Log("http loadinfo: finished " + iMissionsFinished + " unlocked " + (int)(iMissionsFinished * 1.35f));
                    Menu.theMenu.SetMissionUnlock((int)(iMissionsFinished * 1.35f) + 1);
                    Menu.theMenu.InitLevelRanking();
                    iState++;
                }
                break;
            case 1:
                //running menu
                {
                    oASMusic.volume = 0.40f;
                    //oASMusic.volume = 0.00f;

                    //these 5 are set in menu part 2, reset them here
                    Menu.bWorldBestReplay1 = false;
                    Menu.bWorldBestReplay2 = false;
                    Menu.bWorldBestReplay3 = false;
                    Menu.bYourBestReplay = false;
                    Menu.bLevelPlay = false;

                    bool bStart = Menu.bLevelSelected;

                    if (bStart)
                    {
                        Menu.bLevelSelected = false; //reset when we have seen it

                        //goto menu part 2 for the selected level
                        if (GameManager.bUserValid) //will be and must always be true
                        {
                            ///StartCoroutine(oHigh.GetLimits()); 
                            iState++;

                            //set in the above, but since StartCoroutine returns before it has a chanse
                            // to run we need to set it
                            //oHigh.bIsDone = false;
                            //^we already have the info from state 0
                        }
                    }
                    break;
                }
            case 2:
                //we have the info already
                if (true)
                {
                    //set default level info (in case we have network error)
                    stLevel = new LevelInfo();
                    stLevel.szName = GameLevel.szLevel.Substring(1);
                    stLevel.bIsTime = GameLevel.szLevel.StartsWith("2"); //not so good way of doing it but it's all we got
                    stLevel.iScoreMs = stLevel.iBestScore1 = stLevel.iBestScore2 = stLevel.iBestScore3 = -1;
                    stLevel.iLimit1 = stLevel.iLimit2 = stLevel.iLimit3 = -1;
                    stLevel.szBestName1 = "_None"; stLevel.szBestName2 = "_None"; stLevel.szBestName3 = "_None";

                    string szLevelToLoad = GameLevel.szLevel.Substring(1);
                    for (int i = 0; i < oHigh.oLevelList.Count; i++)
                    {
                        if (szLevelToLoad.CompareTo(oHigh.oLevelList[i].szName) == 0) {
                            stLevel = oHigh.oLevelList[i];
                            break;
                        }
                    }
                    //Debug.Log("http loadinfo: "+stLevel.szName + " isTime " + stLevel.bIsTime.ToString());

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
                if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.JoystickButton1) || Input.GetKey(KeyCode.Escape)
#if !DISABLESTEAMWORKS
                    || bStart
#endif
                    )
                {
                    iState = 1; //goto menu part 1 (back)
                    Menu.theMenu.SetLevelInfo(stLevel, true); //stLevel not used
                }

                if ((Menu.bWorldBestReplay1 && stLevel.iBestScore1 != -1)
                    || (Menu.bWorldBestReplay2 && stLevel.iBestScore2 != -1)
                    || (Menu.bWorldBestReplay3 && stLevel.iBestScore3 != -1)
                    || (Menu.bYourBestReplay && stLevel.iScoreMs != -1))
                {
                    string szReplayName = null;
                    if (Menu.bYourBestReplay) szReplayName = GameManager.szUser;
                    if (Menu.bWorldBestReplay1) szReplayName = stLevel.szBestName1;
                    if (Menu.bWorldBestReplay2) szReplayName = stLevel.szBestName2;
                    if (Menu.bWorldBestReplay3) szReplayName = stLevel.szBestName3;

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
                        //oASMusic.volume = 0.09f;
                        /**/oASMusic.volume = 0.00f;
                    }
                }
                break;
            case 7:
                //running game
                {
                    bool bBackToMenu = !GameLevel.bMapLoaded;
                    if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.Escape)
#if !DISABLESTEAMWORKS
                        || bStart
#endif
                        ) //back to menu
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
#if !DISABLESTEAMWORKS
                        if (!GameLevel.bRunReplay && bValveDevicePresent /**/&& XRDevice.userPresence != UserPresenceState.NotPresent)
                        {
                            HandleValveAchievements();
                        }
#endif
                        //////end of valve specific code

                        //always update last level played
                        szLastLevel = GameLevel.szLevel;

                        int iScoreMs;
                        if (GameLevel.theMap.iLevelType == (int)LevelType.MAP_MISSION) iScoreMs = GameLevel.theMap.player.GetScore();
                        else iScoreMs = (int)(GameLevel.theMap.player.fTotalTime * 1000);

                        if (!GameLevel.bRunReplay && (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel))
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
                    iState = -1;
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
