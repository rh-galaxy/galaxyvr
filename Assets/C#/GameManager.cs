using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Oculus.Platform;

public class GameManager : MonoBehaviour
{
    public static GameManager theGM = null;

    internal static bool bOculusDevicePresent = false;
    internal static ulong iUserID = 1;
    internal static string szUser = "UserNone";
    internal static bool bUserValid = false;

    string szLastLevel = "";
    int iAchievementHellbentCounter = 0;

    AsyncOperation asyncLoad;

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
        }

        MapGenerator.theReplay = oReplay;
    }

    // 
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
            //save ID, will need to save that as unique value in the web score db instead of user name
            // since the user can change their user name (once every 6 month) so need to update the name in the 
            // db if that happens (the only way I can think of doing this is to send both id and name and update
            // name if changed, this can result with two different IDs with the same name in the web db... well,
            // let it happen)
            iUserID = msg.GetUser().ID;
            szUser = msg.GetUser().OculusID;
            Debug.Log("You are " + szUser);
            bUserValid = true;
        }
    }
    
    void HandleOculusAchievements()
    {
        //handle achievements
        if (MapGenerator.theMap.player.bAchieveFinishedRaceLevel || MapGenerator.theMap.bAchieveFinishedMissionLevel)
        {
            //finished any level with ok result

            //survivor achievement check
            if (MapGenerator.theMap.player.bAchieveNoDamage)
                Achievements.Unlock("Survivor");
            //hellbent achievement check
            if (szLastLevel.CompareTo(MapGenerator.szLevel) != 0) iAchievementHellbentCounter = 1;
            else iAchievementHellbentCounter++;
            if (iAchievementHellbentCounter == 8)
                Achievements.Unlock("Hellbent");
            //speedster achievement check
            if (MapGenerator.theMap.player.bAchieveFullThrottle)
                Achievements.Unlock("Speedster");
            //racer achievement check
            if (MapGenerator.theMap.player.bAchieveFinishedRaceLevel)
                Achievements.AddCount("Racer", 1);
            //transporter achievement check (named loader)
            if (MapGenerator.theMap.bAchieveFinishedMissionLevel)
                Achievements.AddCount("Loader", 1);

            if (MapGenerator.theMap.bAchieveFinishedMissionLevel)
            {
                //cargo beginner
                if (MapGenerator.szLevel.CompareTo("1mission00") == 0)
                    Achievements.Unlock("Cargo1");
                //cargo apprentice
                if (MapGenerator.szLevel.CompareTo("1mission03") == 0)
                    Achievements.Unlock("Cargo2");
                //cargo expert
                if (MapGenerator.szLevel.CompareTo("1mission06") == 0 && MapGenerator.theMap.player.iAchieveShipsDestroyed == 0)
                    Achievements.Unlock("Cargo3");
                //cargo master
                if (MapGenerator.szLevel.CompareTo("1mission09") == 0 && MapGenerator.theMap.player.bAchieveNoDamage)
                    Achievements.Unlock("Cargo4");
            }
            if (MapGenerator.theMap.player.bAchieveFinishedRaceLevel)
            {
                //race beginner
                if (MapGenerator.szLevel.CompareTo("2race00") == 0)
                    Achievements.Unlock("Race1");
                //race apprentice
                if (MapGenerator.szLevel.CompareTo("2race03") == 0 && MapGenerator.theMap.player.fTotalTime > 200.0f)
                    Achievements.Unlock("Race2");
                //race expert
                if (MapGenerator.szLevel.CompareTo("2race06") == 0 && MapGenerator.theMap.player.fTotalTime > 60.0f)
                    Achievements.Unlock("Race3");
                //race master
                if (MapGenerator.szLevel.CompareTo("2race10") == 0 && MapGenerator.theMap.player.fTotalTime > 104.0f)
                    Achievements.Unlock("Race4");
            }

            //fuelburner achievement check
            int iTemp = (int)MapGenerator.theMap.player.fAchieveFuelBurnt;
            if (iTemp > 0)
                Achievements.AddCount("Fuelburner", (ulong)iTemp);
            //ravager achievement check
            iTemp = MapGenerator.theMap.iAchieveEnemiesKilled;
            if (iTemp > 0)
                Achievements.AddCount("Ravager", (ulong)iTemp);
            //kamikaze achievement check (named doom)
            iTemp = MapGenerator.theMap.player.iAchieveShipsDestroyed;
            if (iTemp > 0)
                Achievements.AddCount("Doom", (ulong)iTemp);
            //trigger achievement check
            iTemp = MapGenerator.theMap.player.iAchieveBulletsFired;
            if (iTemp > 0)
                Achievements.AddCount("Trigger", (ulong)iTemp);
        }
    }

    internal HttpHiscore oHigh = new HttpHiscore();
    /**//*void HandleHiscoreStart()
    {
        LevelInfo GetLevelLimits(string szLevel)
    }*/

    bool bStartReplay = false;
    bool bLoadDone = false;
    int iLoadingMap = 0;
    int iState = -1;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    private void FixedUpdate()
    {
        if (iState == 3) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
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
            }
            else
            {
                Time.timeScale = 0.000001f;

                //also need to stop all sound
                //...
                return;
            }
        }

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
                    bool bStart = Menu.bLevelSelected;
                    bStartReplay = Input.GetKey(KeyCode.R); //just for test now
                    Menu.bLevelSelected = false; //reset when we have seen it

                    //start the selected level
                    if (bStart || bStartReplay)
                    {
                        if (GameManager.bUserValid)
                        {
                            StartCoroutine(oHigh.GetLimits());
                        }
                        iState++;
                    }
                    break;
                }
            case 2:
                //wait for http reply (achievements_get.php)
                if(oHigh.bIsDone)
                {
                    LevelInfo stLevel = new LevelInfo();
                    for (int i = 0; i < oHigh.oLevelList.Count; i++)
                    {
                        stLevel = oHigh.oLevelList[i];
                        string szLevelToLoad = MapGenerator.szLevel.Substring(1);
                        if (szLevelToLoad.CompareTo(stLevel.szName) == 0) break;
                    }
                    Debug.Log("http loaded page: "+stLevel.szName + " isTime " + stLevel.bIsTime.ToString());

                    iState++;
                }
                break;
            case 3:
                //menu part 2 later
                //...
                iState++;
                break;
            case 4:
                //begin loading the level (or replay), set this in state 3 later, now state 1
                //Debug.LogError("Begin load level");
                szToLoad = "Scenes/PlayGame";
                bLoadDone = false;
                bIsMapScene = true;
                bBeginMapLoading = false;
                iLoadingMap = 0;
                MapGenerator.bMapLoaded = false;
                Menu.theMenu.SetWaiting(true);
                if (bStartReplay)
                {
                    oReplay.ResetBeforePlay();
                    MapGenerator.bRunReplay = true;
                }
                else
                {
                    oReplay.Reset(); //reset before recording a new one during play
                    MapGenerator.bRunReplay = false;
                }
                StartCoroutine(LoadAsyncScene());
                iState++;
                break;
            case 5:
                //while loading level
                if (bBeginMapLoading)
                {
                    //Debug.Log("Load level 90%");
                    if (MapGenerator.theMap.LoadInSegments(iLoadingMap++))
                    {
                        Debug.Log("LoadSeg Done");
                        iState++;
                    }
                }
                break;
            case 6:
                //running game
                {
                    bool bBackToMenu = !MapGenerator.bMapLoaded;
                    if (Input.GetKey(KeyCode.JoystickButton6) || Input.GetKey(KeyCode.Escape)) //back to menu
                    {
                        bBackToMenu = true;
                    }
                    if (MapGenerator.theMap.bGameOver)
                    {
                        bBackToMenu = true;

                        //////start of oculus specific code (achievements)
                        if (!MapGenerator.bRunReplay && bOculusDevicePresent /**/&& XRDevice.userPresence != UserPresenceState.NotPresent)
                        {
                            HandleOculusAchievements();
                        }
                        //////end of oculus specific code

                        //always update last level played
                        szLastLevel = MapGenerator.szLevel;
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
            case 7:
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
                if (bIsMapScene && MapGenerator.bMapLoaded || !bIsMapScene)
                    asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
        bLoadDone = asyncLoad.isDone;
    }

}
