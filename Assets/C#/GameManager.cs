﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using System.IO;
using System.Threading;

#if !NOOCULUS
using Oculus.Platform;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager theGM = null;

    public CameraController cameraHolder;

    internal static bool bOculusDevicePresent = false;
    internal static string szUserID = "1";
    internal static string szUser = "DebugUser"; //use debug user if no VR user
    internal static bool bUserValid = false;
    internal static bool bNoHiscore = false;
    internal static bool bNoInternet = false;
    internal static bool bNoVR = false;

    internal float fMasterVolMod = 1.0f;
    internal bool bEasyMode;
    internal bool bCargoSwingingMode;

    string szLastLevel = "";
    int iAchievementHellbentCounter = 0;

    AsyncOperation asyncLoad;

    Replay oReplay = new Replay(); //create one replay... this is recycled during the session


#if LOGPROFILERDATA
    int logProfilerFrameCnt = 0;
    int logProfilerFileCnt = 0;
#endif
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
        bInited = InitOculus();

        if(bInited) //extra check for headset
        {
            UnityEngine.XR.InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (headDevice == null || !headDevice.isValid) bInited = false;
        }

        if (!bInited)
        {
            //no VR

            //init user
            szUserID = SystemInfo.deviceUniqueIdentifier; //this means PC without VR scores are per device
            szUser = "DefaultUser"; //this is overwrited if name.txt has a name

            string szFileText = "";
            string szFile = UnityEngine.Application.persistentDataPath + "/" + "name.txt";
            if (File.Exists(szFile))
            {
                try
                {
                    szFileText = System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(szFile)).Trim();
                    if (szFileText.Length > 32) szFileText = szFileText.Substring(0, 32); //cap at 32 chars
                }
                catch (Exception e) { Debug.Log(e.Message); }
            }
            if (szFileText.Length > 0) szUser = szFileText;

            bUserValid = true;
            bNoHiscore = false;
            bNoVR = true;
            Screen.SetResolution(1280, 720, true);

            Debug.Log("Error initing VR, continue with no VR");
        }
        else
        {
            Screen.SetResolution(864, 960, false);
        }

        GameLevel.theReplay = oReplay;

        //this list keeps the last scores for each level for the entire game session, beginning with no score
        for (int i = 0; i < aLastScore.Length; i++) aLastScore[i] = -1;
        for (int i = 0; i < aLastScore2.Length; i++) aLastScore2[i] = -1;

        //set thread prio
        UnityEngine.Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.BelowNormal;
        Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;

        //init to black
        oFadeMatCopy = new Material(oFadeMat);
        oFadeBox.GetComponent<MeshRenderer>().material = oFadeMatCopy;
        StartFade(0.01f, 0.0f, true);

        AudioStateMachine.instance.Init();
        if (bNoVR) AudioStateMachine.instance.SetOutput();
#if !NOOCULUS
        else AudioStateMachine.instance.SetOutputByRiftSetting();
#endif
        AudioSettings.OnAudioConfigurationChanged += AudioSettings_OnAudioConfigurationChanged;
        GameManager.theGM.fMasterVolMod = PlayerPrefs.GetFloat("MyMasterVolMod", 1.0f);
        AudioStateMachine.instance.masterVolume = 1.25f * fMasterVolMod;

        cameraHolder.InitForMenu();
#if LOGPROFILERDATA
        Profiler.logFile = "log" + logProfilerFileCnt.ToString();
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;
#endif
    }

    private void AudioSettings_OnAudioConfigurationChanged(bool deviceWasChanged)
    {
#if !NOOCULUS
        if (!bNoVR) AudioStateMachine.instance.SetOutputByRiftSetting();
        else AudioStateMachine.instance.SetOutput(0);
#else
        AudioStateMachine.instance.SetOutput(0);
#endif
    }

    //////start of oculus specific code
    bool InitOculus()
    {
        //hack for handling mac platform which will throw uncatched exception
        if (UnityEngine.Application.platform != RuntimePlatform.WindowsEditor
         && UnityEngine.Application.platform != RuntimePlatform.WindowsPlayer
         && UnityEngine.Application.platform != RuntimePlatform.Android)
            return false;

#if !NOOCULUS
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
            //UnityEngine.Application.Quit();
            return false;
        }

        bOculusDevicePresent = true;
        return true;
#else
        return false;
#endif
    }

#if !NOOCULUS
    void EntitlementCallback(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("Not entitled to play this game");

            UnityEngine.Application.Quit(); //it is possible to remove quit while developing

            //this only happens when running in Unity Editor
            bNoVR = true; //will make the game not pause
            bNoHiscore = true; //will make the game continue in the state machine
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
            szUserID = msg.GetUser().ID.ToString();
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
            //user levels achievement check
            if (GameLevel.iLevelIndex >= 55)
                Achievements.AddCount("UserLevels", 1);

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
            if (GameLevel.iLevelIndex < 55)
            {
                string szBits = "0000000000000000000000000000000000000000000000000000000";
                char[] aBitsChars = szBits.ToCharArray();
                aBitsChars[GameLevel.iLevelIndex] = '1';
                string szBits2 = new string(aBitsChars, 0, aBitsChars.Length - 0);
                Achievements.AddFields("Galaxy55", szBits2);
            }
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
            Achievements.AddCount("Hitchhiker42", (ulong)iTemp * 5); //5m per unit is reasonable
    }
    //////end of oculus specific code

    void UnlockMissionGoldAchievement()
    {
        if (bOculusDevicePresent)
        {
            Achievements.Unlock("CargoGold30");
        }
    }
    void UnlockRaceGoldAchievement()
    {
        if (bOculusDevicePresent)
        {
            Achievements.Unlock("RaceGold25");
        }
    }
#endif

    void HandleAchievements_PC()
    {
        //handle achievements
        if (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel)
        {
            //finished any level with ok result

            //survivor achievement check
            if (GameLevel.theMap.player.bAchieveNoDamage)
                PlayerPrefs.SetInt("AchSurvivor", 1);
            //hellbent achievement check
            if (szLastLevel.CompareTo(GameLevel.szLevel) != 0) iAchievementHellbentCounter = 1;
            else iAchievementHellbentCounter++;
            if (iAchievementHellbentCounter == 8)
                PlayerPrefs.SetInt("AchHellbent", 1);
            //speedster achievement check
            if (GameLevel.theMap.player.bAchieveFullThrottle)
                PlayerPrefs.SetInt("AchSpeedster", 1);
            //racer achievement check
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
            {
                int iStatsTotalRacePlayed = PlayerPrefs.GetInt("AchRacerTotalRacePlayed", 0) + 1;
                if (iStatsTotalRacePlayed >= 12) PlayerPrefs.SetInt("AchRacer", 1);
                PlayerPrefs.SetInt("AchRacerTotalRacePlayed", iStatsTotalRacePlayed);
            }
            //transporter achievement check (named loader)
            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
            {
                int iStatsTotalMissionPlayed = PlayerPrefs.GetInt("AchLoaderTotalMissionPlayed", 0) + 1;
                if (iStatsTotalMissionPlayed >= 12) PlayerPrefs.SetInt("AchLoader", 1);
                PlayerPrefs.SetInt("AchRacerTotalMissionPlayed", iStatsTotalMissionPlayed);
            }
            //user levels achievement check
            if (GameLevel.iLevelIndex >= 55)
            {
                int iStatsUserLevelsPlayed = PlayerPrefs.GetInt("AchTotalUserLevelsPlayed", 0) + 1;
                if (iStatsUserLevelsPlayed >= 20) PlayerPrefs.SetInt("AchUserLevels", 1);
                PlayerPrefs.SetInt("AchTotalUserLevelsPlayed", iStatsUserLevelsPlayed);
            }
            if (GameLevel.theMap.bAchieveFinishedMissionLevel)
            {
                //cargo beginner
                if (GameLevel.szLevel.CompareTo("1mission00") == 0)
                    PlayerPrefs.SetInt("AchCargo1", 1);
                //cargo apprentice
                if (GameLevel.szLevel.CompareTo("1mission03") == 0)
                    PlayerPrefs.SetInt("AchCargo2", 1);
                //cargo expert
                if (GameLevel.szLevel.CompareTo("1mission06") == 0 && GameLevel.theMap.player.iAchieveShipsDestroyed == 0)
                    PlayerPrefs.SetInt("AchCargo3", 1);
                //cargo master
                if (GameLevel.szLevel.CompareTo("1mission09") == 0 && GameLevel.theMap.player.bAchieveNoDamage)
                    PlayerPrefs.SetInt("AchCargo4", 1);
            }
            if (GameLevel.theMap.player.bAchieveFinishedRaceLevel)
            {
                //race beginner
                if (GameLevel.szLevel.CompareTo("2race00") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    PlayerPrefs.SetInt("AchRace1", 1);
                //race apprentice
                if (GameLevel.szLevel.CompareTo("2race03") == 0 && GameLevel.theMap.player.fTotalTime < 180.0f)
                    PlayerPrefs.SetInt("AchRace2", 1);
                //race expert
                if (GameLevel.szLevel.CompareTo("2race06") == 0 && GameLevel.theMap.player.fTotalTime < 60.0f)
                    PlayerPrefs.SetInt("AchRace3", 1);
                //race master
                if (GameLevel.szLevel.CompareTo("2race10") == 0 && GameLevel.theMap.player.fTotalTime < 104.0f)
                    PlayerPrefs.SetInt("AchRace4", 1);
            }
            if (GameLevel.iLevelIndex < 55)
            {
                int bits = 0x00000001;
                int iStatsLevelsPlayedBits1 = PlayerPrefs.GetInt("AchGalaxy55LevelsPlayedBits1", 0);
                int iStatsLevelsPlayedBits2 = PlayerPrefs.GetInt("AchGalaxy55LevelsPlayedBits2", 0);
                if (GameLevel.iLevelIndex >= 31) iStatsLevelsPlayedBits2 |= bits << (GameLevel.iLevelIndex - 31);
                else iStatsLevelsPlayedBits1 |= bits << GameLevel.iLevelIndex; //we use 31 bits in Bits1 and 24 bits in Bits2 (55 bits)
                if (iStatsLevelsPlayedBits1 == 0x7fffffff && iStatsLevelsPlayedBits2 == 0x00ffffff) PlayerPrefs.SetInt("AchGalaxy55", 1);
                PlayerPrefs.SetInt("AchGalaxy55LevelsPlayedBits1", iStatsLevelsPlayedBits1);
                PlayerPrefs.SetInt("AchGalaxy55LevelsPlayedBits2", iStatsLevelsPlayedBits2);
            }
        }
        //fuelburner achievement check
        int iTemp = (int)GameLevel.theMap.player.fAchieveFuelBurnt;
        if (iTemp > 0)
        {
            int iStatsTotalFuelBurnt = PlayerPrefs.GetInt("AchFuelburnerTotalFuelBurnt", 0) + iTemp;
            if (iStatsTotalFuelBurnt >= 1200) PlayerPrefs.SetInt("AchFuelburner", 1); //20 minutes
            PlayerPrefs.SetInt("AchFuelburnerTotalFuelBurnt", iStatsTotalFuelBurnt);
        }
        //ravager achievement check
        iTemp = GameLevel.theMap.iAchieveEnemiesKilled;
        if (iTemp > 0)
        {
            int iStatsTotalEnemiesKilled = PlayerPrefs.GetInt("AchRavagerTotalEnemiesKilled", 0) + iTemp;
            if (iStatsTotalEnemiesKilled >= 100) PlayerPrefs.SetInt("AchRavager", 1);
            PlayerPrefs.SetInt("AchRavagerTotalEnemiesKilled", iStatsTotalEnemiesKilled);
        }
        //kamikaze achievement check (named doom)
        iTemp = GameLevel.theMap.player.iAchieveShipsDestroyed;
        if (iTemp > 0)
        {
            int iStatsTotalShipsDestroyed = PlayerPrefs.GetInt("AchDoomTotalShipsDestroyed", 0) + iTemp;
            if (iStatsTotalShipsDestroyed >= 100) PlayerPrefs.SetInt("AchDoom", 1);
            PlayerPrefs.SetInt("AchDoomTotalShipsDestroyed", iStatsTotalShipsDestroyed);
        }
        //trigger achievement check
        iTemp = GameLevel.theMap.player.iAchieveBulletsFired;
        if (iTemp > 0)
        {
            int iStatsTotalBulletsFired = PlayerPrefs.GetInt("AchTriggerTotalBulletsFired", 0) + iTemp;
            if (iStatsTotalBulletsFired >= 1000) PlayerPrefs.SetInt("AchTrigger", 1);
            PlayerPrefs.SetInt("AchTriggerTotalBulletsFired", iStatsTotalBulletsFired);
        }
        //hitchhiker42 achievement check
        iTemp = (int)GameLevel.theMap.player.fAchieveDistance;
        if (iTemp > 0)
        {
            int iStatsTotalMetersTravelled = PlayerPrefs.GetInt("AchHitchhiker42TotalMetersTravelled", 0) + iTemp * 5; //5m per unit is reasonable
            if (iStatsTotalMetersTravelled >= 42000) PlayerPrefs.SetInt("AchHitchhiker42", 1);
            PlayerPrefs.SetInt("AchHitchhiker42TotalMetersTravelled", iStatsTotalMetersTravelled);
        }

        //save
        PlayerPrefs.Save();
    }
    void UnlockMissionGoldAchievement_PC()
    {
        if (bUserValid)
        {
            PlayerPrefs.SetInt("AchCargoGold30", 1);
            PlayerPrefs.Save();
        }
    }
    void UnlockRaceGoldAchievement_PC()
    {
        if (bUserValid)
        {
            PlayerPrefs.SetInt("AchRaceGold25", 1);
            PlayerPrefs.Save();
        }
    }

    LevelInfo stLevel;
    internal HttpHiscore oHigh = new HttpHiscore();
    int[] aLastScore = new int[800]; //a bit of a hack
    int[] aLastScore2 = new int[800]; //a bit of a hack
    int iLastLevelIndex;
    bool bAutoSetLevelInfo = false;

    bool bStartReplay = false;
    bool bLoadDone = false;
    bool bLoadBeginDone = false;
    int iSetLevelInfo = 0;
    int iState = -3;

    bool bPause = false;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    private void FixedUpdate()
    {
        if (iState == 10) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
    }

    /**/
    //to avoid a 6 second freeze when the file is not in cache
    // this is loaded here once hopefully while the user spend some seconds in the menu before this will be accessed by LoadSceneAsync.
    //so much for async.
    //note: with planet texture data now 100MB instead of 1000MB this is not needed as much
    byte[] preLoadBytes = new byte[1024 * 1024]; //1MB buffer
    string preLoadDataPath;
    Thread preLoadThread;
    void PreLoadAssetsToCache()
    {
        bool allRead = false;
        int iChunkSize = preLoadBytes.Length;
        int iChunkNr = 0;
        FileStream fs = null;
        try
        {
            fs = File.OpenRead(preLoadDataPath + "/sharedassets1.assets.resS");
        } catch { }
        while (fs != null && !allRead)
        {
            int fr = fs.Read(preLoadBytes, 0, iChunkSize);
            if (fr == 0) allRead = true;
            iChunkNr++;
            Thread.Sleep(5);
        }
    }

    //fading code
    float fFadeFinishTime = 1.0f;
    float fFadeTimer = 0.0f;
    float fFadeDelay = 0.0f;
    int iFade = 0; //0 no, 1 in from black, 2 out to black
    public Material oFadeMat;
    Material oFadeMatCopy;
    public GameObject oFadeBox;
    void UpdateFade()
    {
        if (iFade == 0)
            return;

        fFadeTimer += Time.unscaledDeltaTime;
        if (fFadeTimer < fFadeDelay)
            return;

        float fProgress = (fFadeTimer - fFadeDelay) / fFadeFinishTime;
        float fFadeCurAlpha = fProgress;
        if (iFade == 1) fFadeCurAlpha = 1.0f - fProgress;
        if (fFadeCurAlpha < 0.0f) fFadeCurAlpha = 0.0f;
        if (fFadeCurAlpha > 1.0f) fFadeCurAlpha = 1.0f;
        oFadeMatCopy.color = new Color(0, 0, 0, fFadeCurAlpha);
        if (fProgress > 0.999f)
        {
            iFade = 0;
            if (fFadeCurAlpha < 0.1)
            {
                //fade in done
                oFadeBox.SetActive(false);
                cameraHolder.Fade(true);
            }
        }
    }
    public void StartFade(float fTime, float fDelay, bool bOut)
    {
        if (bOut) cameraHolder.Fade(false);
        fFadeFinishTime = fTime;
        fFadeTimer = 0.0f;
        fFadeDelay = fDelay;
        if (bOut) iFade = 2; //out
        else iFade = 1; //in
        oFadeBox.SetActive(true);
        UpdateFade();
    }

    //the reason we have to deal with this here is that the GameLevel code is not started so can't run subroutines.
    int loadState = 0;
    bool LoadUserLevel()
    {
        switch (loadState)
        {
            case 0:
                StartCoroutine(oHigh.GetLevelFile(GameLevel.szLevel+".des", false));
                loadState++;
                break;
            case 1:
                if (oHigh.bIsDone)
                {
                    loadState++;
                }
                break;
            case 2:
                string szMapFile = GameLevel.FindMapFile(oHigh.szLevelTextData);
                StartCoroutine(oHigh.GetLevelFile(szMapFile, true));
                loadState++;
                break;
            case 3:
                if (oHigh.bIsDone)
                {
                    loadState++;
                }
                break;
        }
        if(loadState==4)
        {
            loadState = 0;
            return true;
        }
        return false;
    }

    bool bTrackingOriginSet = false;
    void Update()
    {
#if LOGPROFILERDATA
        //unity profiler log files can only be viewed 300 frames at a time! :(
        logProfilerFrameCnt++;
        if(logProfilerFrameCnt>300)
        {
            logProfilerFileCnt++;
            logProfilerFrameCnt = 0;
            Profiler.logFile = "log" + logProfilerFileCnt.ToString();
        }
#endif

        //get input devices
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;
        UnityEngine.XR.InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        UnityEngine.XR.InputDevice handRDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        UnityEngine.XR.InputDevice handLDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool buttonSelLSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool buttonSelL);

        //quit
        if (Menu.bQuit)
        {
#if UNITY_EDITOR
            //Application.Quit() does not work in the editor so
            // this need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        //recenter
        //do it from oculus right-Menu (select recenter)
        //implement Y/Z-adjust instead
        if (!bTrackingOriginSet) //always do once
        {
            if (headDevice!=null && headDevice.isValid)
            {
                headDevice.subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
                bTrackingOriginSet = true;
            }
        }

        bool bBackButton = false;

        //get user present
        bool presenceFeatureSupported = headDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out bool userPresent);

        //pause if in oculus home universal menu
        // but for now (for debug purposes) keep the game running while XRDevice.userPresence!=Present
        bool bPauseNow = bPause; //no change below
#if !NOOCULUS
        if (bOculusDevicePresent && !bNoVR)
        {
            bPauseNow = (!OVRPlugin.hasInputFocus || !OVRPlugin.hasVrFocus) /*|| !userPresent)*/;
        }
#endif
        if (bNoVR) bPauseNow = false;

        //pause state change
        if (bPause != bPauseNow)
        {
            bPause = bPauseNow;
            if (bPauseNow)
            {
                Time.timeScale = 0.0f; //stops FixedUpdate

                //also need to stop all sound
                AudioListener.pause = true;
                AudioStateMachine.instance.masterVolume = 0.0f;

                Menu.bPauseInput = true;
            }
            else
            {
                Time.timeScale = 1.0f;

                AudioListener.pause = false;
                AudioStateMachine.instance.masterVolume = 1.25f * fMasterVolMod;

                Menu.bPauseInput = false;
            }
        }

        UpdateFade();

        //to ignore input below, only way back is to unpause
        if (bPause) return;


        //the main state machine
        switch (iState)
        {
            case -3:
                //by use of the EditorAutoLoad script the main scene should be loaded first
                // and should be active here ("Scenes/GameStart")
                iState++;

                //to avoid stalls of 5+ sec the first time a level is started after app start
                preLoadDataPath = UnityEngine.Application.dataPath;
                ThreadStart ts = new ThreadStart(PreLoadAssetsToCache);
                preLoadThread = new Thread(ts);
                preLoadThread.Priority = System.Threading.ThreadPriority.Lowest;
                preLoadThread.Start();

                StartCoroutine(LoadAsyncTileset());

                break;
            case -2:
                //wait for user id/name to be ready
                if (bUserValid || bNoHiscore)
                {
                    StartFade(1.5f, 1.0f, false);
                    iState++;
                }
                break;
            case -1:
                //get the progress, to see how many levels are unlocked
                if (!bNoInternet)
                {
                    if (bNoVR) StartCoroutine(oHigh.GetLimits_PC());
                    else StartCoroutine(oHigh.GetLimits());
                    //set in the above, but since StartCoroutine returns before it has a chance
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
                    int iMissionFinished = 0;
                    int iRaceFinished = 0;
                    int iMissionFinishedGold = 0;
                    int iRaceFinishedGold = 0;
                    int iToGo = oHigh.oLevelList.Count > 55 ? 55 : oHigh.oLevelList.Count; //only count original 55
                    for (int i = 0; i < iToGo; i++)
                    {
                        stLevel = oHigh.oLevelList[i];
                        if (!stLevel.bIsTime)
                        {
                            if (stLevel.info.iBestScoreMs != -1)
                            {
                                iMissionFinished++;
                                if (stLevel.info.iBestScoreMs >= stLevel.info.iLimit1) iMissionFinishedGold++;
                            }
                        }
                        else
                        {
                            if (stLevel.info.iBestScoreMs != -1)
                            {
                                iRaceFinished++;
                                if (stLevel.info.iBestScoreMs < stLevel.info.iLimit1) iRaceFinishedGold++;
                            }
                        }
                    }
                    if (oHigh.oLevelList.Count == 0)
                    {
                        Debug.LogError("error getting level list, disableing further internet access");
                        bNoInternet = true; //set this so no further attempts are made at accessing the internet
                    }

                    //handle gold achievements
                    if (iMissionFinishedGold >= 30)
                    {
#if !NOOCULUS
                        UnlockMissionGoldAchievement();
#endif
                        if (bNoVR) UnlockMissionGoldAchievement_PC();
                    }
                    if (iRaceFinishedGold >= 25)
                    {
#if !NOOCULUS
                        UnlockRaceGoldAchievement();
#endif
                        if (bNoVR) UnlockRaceGoldAchievement_PC();
                    }
                    //handle unlocking
                    int iMissionToUnlock = (int)(iMissionFinished * 1.35f) + 1;
                    if (bNoInternet || bNoHiscore || iMissionToUnlock > 30) iMissionToUnlock = 30; //unlock everything
                    int iRaceToUnlock = (int)(iRaceFinished * 1.35f) + 1;
                    if (bNoInternet || bNoHiscore || iRaceToUnlock > 25) iRaceToUnlock = 25; //unlock everything
                    Debug.Log("http loadinfo: mission finished " + iMissionFinished + " unlocked " + iMissionToUnlock);
                    Debug.Log("http loadinfo: race finished " + iRaceFinished + " unlocked " + iRaceToUnlock);
                    Menu.theMenu.SetLevelUnlock(iMissionToUnlock, iRaceToUnlock);
                    Menu.theMenu.InitLevelRanking(bNoHiscore);
                    iState++;
                }
                break;
            case 1:
                //running menu
                {
                    //these 5 are set in menu part 2, reset them here
                    Menu.bWorldBestReplay1 = false;
                    Menu.bWorldBestReplay2 = false;
                    Menu.bWorldBestReplay3 = false;
                    Menu.bYourBestReplay = false;
                    Menu.bLevelPlay = false;

                    if (Menu.bLevelSelected || bAutoSetLevelInfo)
                    {
                        Menu.bLevelSelected = false; //reset when we have seen it
                        Menu.bLevelUnSelected = false; //in case unselect was triggered before
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
                    if (GameLevel.iLevelIndex >= 200) stLevel.szName = GameLevel.szLevel;
                    else stLevel.szName = GameLevel.szLevel.Substring(1);
                    //stLevel.bIsTime = GameLevel.szLevel.StartsWith("2"); //not so good way of doing it but it's all we got
                    //^this is now set in SetLevelInfo, read from file

                    stLevel.info.iBestScoreMs = stLevel.info.iLastScoreMs = -1;
                    stLevel.info.iWRScore1 = stLevel.info.iWRScore2 = stLevel.info.iWRScore3 = -1;
                    stLevel.info.iLimit1 = stLevel.info.iLimit2 = stLevel.info.iLimit3 = -1;
                    stLevel.info.szWRName1 = "_None"; stLevel.info.szWRName2 = "_None"; stLevel.info.szWRName3 = "_None";
                    stLevel.info2.iBestScoreMs = stLevel.info2.iLastScoreMs = -1;
                    stLevel.info2.iWRScore1 = stLevel.info2.iWRScore2 = stLevel.info2.iWRScore3 = -1;
                    stLevel.info2.iLimit1 = stLevel.info2.iLimit2 = stLevel.info2.iLimit3 = -1;
                    stLevel.info2.szWRName1 = "_None"; stLevel.info2.szWRName2 = "_None"; stLevel.info2.szWRName3 = "_None";

                    string szLevelToLoad = stLevel.szName;
                    if (GameLevel.iLevelIndex >= 200 && GameLevel.iLevelIndex<400)
                    {
                        stLevel.info.iLastScoreMs = aLastScore[GameLevel.iLevelIndex];
                        stLevel.info2.iLastScoreMs = aLastScore2[GameLevel.iLevelIndex];
                        iLastLevelIndex = GameLevel.iLevelIndex;
                    }
                    else
                    {
                        for (int i = 0; i < oHigh.oLevelList.Count; i++)
                        {
                            if (szLevelToLoad.CompareTo(oHigh.oLevelList[i].szName) == 0)
                            {
                                stLevel = oHigh.oLevelList[i];
                                stLevel.info.iLastScoreMs = aLastScore[i];
                                stLevel.info2.iLastScoreMs = aLastScore2[i];
                                iLastLevelIndex = i;
                                break;
                            }
                        }
                    }
                    iState++;
                }
                break;
            case 3:
                if (GameLevel.iLevelIndex >= 400)
                {
                    if (!LoadUserLevel()) break; //do until completion first in this state so the level can show info and minimap during this state (until play)
                }

                Menu.theMenu.SetLevelInfoPass1(stLevel); //set our level info to menu, it will be displayed there
                iSetLevelInfo = 0;
                iState++;
                break;
            case 4:
                //must always run after SetLevelInfoPass1
                if (Menu.theMenu.SetLevelInfoPass2(stLevel, iSetLevelInfo)) //set our level info to menu, it will be displayed there
                    iState++;
                iSetLevelInfo++;
                break;
            case 5:
                //menu part 2
                if (Menu.bLevelSelected)
                {
                    iState = 1; //goto menu part 1 since we have selected another level
                }
                if (gamepad != null) bBackButton = bBackButton || gamepad.selectButton.isPressed;
                if (keyboard != null) bBackButton = bBackButton || keyboard.escapeKey.isPressed;
                if (Menu.bLevelUnSelected || bBackButton || buttonSelL)
                {
                    Menu.bLevelUnSelected = false;
                    iState = 1; //goto menu part 1 (back)
                    Menu.theMenu.SetLevelInfoOff();
                }

                //if bNoInternet is true this will not be possible be design
                {
                    bool isLi2 = bCargoSwingingMode;
                    if (isLi2)
                    {
                        if ((Menu.bWorldBestReplay1 && stLevel.info2.iWRScore1 != -1)
                            || (Menu.bWorldBestReplay2 && stLevel.info2.iWRScore2 != -1)
                            || (Menu.bWorldBestReplay3 && stLevel.info2.iWRScore3 != -1)
                            || (Menu.bYourBestReplay && stLevel.info2.iBestScoreMs != -1))
                        {
                            string szReplayId = null;
                            if (Menu.bYourBestReplay) szReplayId = GameManager.szUserID;
                            if (Menu.bWorldBestReplay1) szReplayId = stLevel.info2.szWRId1;
                            if (Menu.bWorldBestReplay2) szReplayId = stLevel.info2.szWRId2;
                            if (Menu.bWorldBestReplay3) szReplayId = stLevel.info2.szWRId3;

                            if (bNoVR) StartCoroutine(oHigh.GetReplay2_PC(stLevel.szName, szReplayId, oReplay));
                            else StartCoroutine(oHigh.GetReplay2(stLevel.szName, szReplayId, oReplay));
                            iState++; //load replay
                            StartFade(0.3f, 0.0f, true);

                            //set in the above, but since StartCoroutine returns before it has a chance
                            // to run we need to set it
                            oHigh.bIsDone = false;
                        }
                    }
                    else
                    {
                        if ((Menu.bWorldBestReplay1 && stLevel.info.iWRScore1 != -1)
                            || (Menu.bWorldBestReplay2 && stLevel.info.iWRScore2 != -1)
                            || (Menu.bWorldBestReplay3 && stLevel.info.iWRScore3 != -1)
                            || (Menu.bYourBestReplay && stLevel.info.iBestScoreMs != -1))
                        {
                            string szReplayId = null;
                            if (Menu.bYourBestReplay) szReplayId = GameManager.szUserID;
                            if (Menu.bWorldBestReplay1) szReplayId = stLevel.info.szWRId1;
                            if (Menu.bWorldBestReplay2) szReplayId = stLevel.info.szWRId2;
                            if (Menu.bWorldBestReplay3) szReplayId = stLevel.info.szWRId3;

                            if (bNoVR) StartCoroutine(oHigh.GetReplay_PC(stLevel.szName, szReplayId, oReplay));
                            else StartCoroutine(oHigh.GetReplay(stLevel.szName, szReplayId, oReplay));
                            iState++; //load replay
                            StartFade(0.3f, 0.0f, true);

                            //set in the above, but since StartCoroutine returns before it has a chance
                            // to run we need to set it
                            oHigh.bIsDone = false;
                        }
                    }
                }
                if (Menu.bLevelPlay)
                {
                    iState += 2; //go directly to load level
                    StartFade(0.3f, 0.0f, true);
                }
                break;
            case 6:
                //menu part 2, while loading replay
                if (oHigh.bIsDone)
                {
                    bStartReplay = true;
                    iState++;
                }
                break;
            case 7:
                //wait for fade done
                if (iFade == 0)
                    iState++;
                break;

            case 8:
                //moved here from state 10 as a bug fix
                szLastLevel = GameLevel.szLevel;

                //begin loading the level (or replay)
                //Debug.Log("Load map Begin");
                szToLoad = "Scenes/PlayGame";
                bLoadDone = false;
                bIsMapScene = true;
                bBeginMapLoading = false;
                bLoadBeginDone = false;
                GameLevel.bMapLoaded = false;
                if (bStartReplay)
                {
                    oReplay.ResetBeforePlay();
                    GameLevel.bRunReplay = true;
                }
                else
                {
                    bool isLi2 = bCargoSwingingMode;
                    oReplay.Reset(3, bEasyMode, isLi2); //reset before recording a new one during play
                    GameLevel.bRunReplay = false;
                }
                bStartReplay = false; //we have seen it
                StartCoroutine(LoadAsyncScene());
                iState++;
                break;
            case 9:
                //while loading level
                if (bBeginMapLoading && bTilesetLoaded)
                {
                    //Debug.Log("Load level 90%");

                    //this is done here instead of in GameLevel because the loading code does not work from there!
                    if (!GameLevel.theMap.bTilesetLoaded)
                    {
                        GameLevel.theMap.oTileTexture = oTileTexture;
                        GameLevel.theMap.bTilesetLoaded = true;
                    }

                    if (!bLoadBeginDone) bLoadBeginDone = GameLevel.theMap.LoadBegin();
                    else if (GameLevel.theMap.LoadDone())
                    {
                        //Debug.Log("Load map segments Done");
                        iState++;
                    }
                }
                break;
            case 10:
                //running game
                {
                    bool bBackToMenu = !GameLevel.bMapLoaded;

                    if (gamepad != null) bBackButton = bBackButton || gamepad.selectButton.isPressed;
                    if (keyboard != null) bBackButton = bBackButton || keyboard.escapeKey.isPressed;
                    if (bBackButton || buttonSelL) //back to menu
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
#if !NOOCULUS
                                if (bOculusDevicePresent && userPresent) //VR user must be present for achievements
                                {
                                    HandleOculusAchievements();
                                }
#endif
                                if (bNoVR) HandleAchievements_PC();
                                //////end of oculus specific code
                            }
                        }

                        //get score from GameLevel
                        bool isLi2 = GameManager.theGM.bCargoSwingingMode;
                        int iScoreMs;
                        if (GameLevel.theMap.iLevelType == (int)LevelType.MAP_MISSION) iScoreMs = GameLevel.theMap.player.GetScore();
                        else
                        {
                            iScoreMs = (int)(GameLevel.theMap.player.fTotalTime * 1000);
                            if (GameLevel.theReplay.bEasyMode) iScoreMs *= 2;
                        }
                        if (!GameLevel.bRunReplay)
                        {
                            if (isLi2) aLastScore2[iLastLevelIndex] = iScoreMs;
                            else aLastScore[iLastLevelIndex] = iScoreMs;
                        }

                        if (!bNoHiscore && !bNoInternet && iLastLevelIndex<200)
                        {
                            //always update last level played
                            //szLastLevel = GameLevel.szLevel;
                            //^now done above at the time the play button is pressed

                            if (!GameLevel.bRunReplay && (GameLevel.theMap.player.bAchieveFinishedRaceLevel || GameLevel.theMap.bAchieveFinishedMissionLevel))
                            {
                                //protect against huge replays (medium blob in server db is 16MB but no need to support that large)
                                if (oReplay.GetSize() < 3 * 1024 * 1024) //at 200 bytes per sec this is ~4.5 hours, and normal rate is ~100 Bps.
                                {
                                    //finished ok, and with a new score or better than before, then send

                                    if(!isLi2)
                                    {
                                        if (stLevel.info.iBestScoreMs == -1 || (!stLevel.bIsTime && iScoreMs > stLevel.info.iBestScoreMs) ||
                                            (stLevel.bIsTime && iScoreMs < stLevel.info.iBestScoreMs))
                                        {
                                            string szFile = iLastLevelIndex < 55 ? szLastLevel.Substring(1) : szLastLevel;
                                            if (bNoVR) StartCoroutine(oHigh.SendHiscore_PC(szFile, iScoreMs, oReplay));
                                            else StartCoroutine(oHigh.SendHiscore(szFile, iScoreMs, oReplay));

                                            //set in the above, but since StartCoroutine returns before it has a chance
                                            // to run we need to set it
                                            oHigh.bIsDone = false;
                                        }
                                    }
                                    else
                                    {
                                        if (stLevel.info2.iBestScoreMs == -1 || (!stLevel.bIsTime && iScoreMs > stLevel.info2.iBestScoreMs) ||
                                            (stLevel.bIsTime && iScoreMs < stLevel.info2.iBestScoreMs))
                                        {
                                            string szFile = iLastLevelIndex < 55 ? szLastLevel.Substring(1) : szLastLevel;
                                            if (bNoVR) StartCoroutine(oHigh.SendHiscore2_PC(szFile, iScoreMs, oReplay));
                                            else StartCoroutine(oHigh.SendHiscore2(szFile, iScoreMs, oReplay));

                                            //set in the above, but since StartCoroutine returns before it has a chance
                                            // to run we need to set it
                                            oHigh.bIsDone = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bBackToMenu)
                    {
                        StartFade(0.3f, 0.0f, true);
                        iState++;
                    }
                    break;
                }
            case 11:
                if (iFade==0) //fading done?
                {
                    cameraHolder.InitForMenu();
                    szToLoad = "Scenes/GameStart";
                    bLoadDone = false;
                    bIsMapScene = false;
                    StartCoroutine(LoadAsyncScene());
                    iState++;
                }
                break;
            case 12:
                //while hiscore is being sent
                if (oHigh.bIsDone)
                    iState++;
                break;
            case 13:
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
        //start fading music
        AudioStateMachine.instance.LevelTransition(bIsMapScene ? 1.0f : 0.0f);
        if (!bIsMapScene) AudioStateMachine.instance.player = null; //set before switching scene

        //the Application loads the scene in the background as the current scene runs
        // this is good for not freezing the view... done by separating some work to a thread
        // and having the rest split in ~7ms jobs

        asyncLoad = SceneManager.LoadSceneAsync(szToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        //wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            //scene has loaded as much as possible, the last 10% can't be multi-threaded
            if (asyncLoad.progress >= 0.9f)
            {
                bBeginMapLoading = true;
                if (bIsMapScene && GameLevel.bMapLoaded || !bIsMapScene)
                    asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
        bLoadDone = asyncLoad.isDone;

        if (bIsMapScene) AudioStateMachine.instance.player = GameLevel.theMap.player; //set after switching scene
    }

    //we only use one tileset for building all maps, so we can load it once here and never again
    //this is a thing that was unsolvable, StartCoroutine() from GameLevel would not work, had to be done here
    //update: a clue why this doesn't work is that GameLevel.Update() does not yet run when we want to do the loading,
    // because we have not yet started the level. it affects both coroutines not running and apparently also
    // loadAsync.isDone does not get updated
    static string szTilesetPath = "Levels/ts_alien";
    Texture2D oTileTexture;
    bool bTilesetLoaded = false;
    IEnumerator LoadAsyncTileset()
    {
        ResourceRequest loadAsync = Resources.LoadAsync<TextAsset>(szTilesetPath);

        while (!loadAsync.isDone)
        {
            yield return null;
        }

        TextAsset f = (TextAsset)loadAsync.asset;
        oTileTexture = new Texture2D(2, 2);
        oTileTexture.LoadImage(f.bytes, false);

        bTilesetLoaded = true;
    }

}
