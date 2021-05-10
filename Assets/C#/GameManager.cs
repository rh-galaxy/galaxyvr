using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using System.IO;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager theGM = null;

    public CameraController theCameraHolder;

    internal static string szUserID = "1";
    internal static string szUser = "DebugUser"; //use debug user if no VR user
    internal static bool bUserValid = false;
    internal static bool bNoHiscore = false;
    internal static bool bNoInternet = false;
    internal static bool bNoVR = false;

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

        //no VR
        //bUserValid = true;
        bNoHiscore = true;
        bNoVR = true;
        Screen.SetResolution(1280, 720, true);

        GameLevel.theReplay = oReplay;

        //this list keeps the last scores for each level for the entire game session, beginning with no score
        for (int i = 0; i < aLastScore.Length; i++) aLastScore[i] = -1;

        //set thread prio
        UnityEngine.Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.BelowNormal;
        Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;

        //init to black
        theCameraHolder.InitForMenu();
        oFadeMatCopy = new Material(oFadeMat);
        oFadeBox.GetComponent<MeshRenderer>().material = oFadeMatCopy;
        StartFade(0.01f, 0.0f, true);

        AudioSettings.OnAudioConfigurationChanged += AudioSettings_OnAudioConfigurationChanged;

#if LOGPROFILERDATA
        Profiler.logFile = "log" + logProfilerFileCnt.ToString();
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;
#endif
    }

    private void AudioSettings_OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        AudioStateMachine.instance.SetOutput(0);
    }

    internal HttpHiscore oHigh = new HttpHiscore();
    int[] aLastScore = new int[800]; //a bit of a hack

    bool bLoadDone = false;
    bool bLoadBeginDone = false;
    int iState = -3;

    //it is ensured through Edit->Project settings->Script Execution Order that this runs _after_ the updates of others.
    private void FixedUpdate()
    {
        if (iState == 10) oReplay.IncTimeSlot(); //everything regarding replay should be done in fixed update
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
            }
        }
    }
    public void StartFade(float fTime, float fDelay, bool bOut)
    {
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

    private static char[] splitChars = new char[] { '&' };
    private static string ParseURLParams(string aFullURL, string aName)
    {
        if (aFullURL == null || aFullURL.Length <= 1 || aName == null || aName.Length <= 1)
            return null;
        // skip "?" and split parameters at "&"
        int q = aFullURL.IndexOf('?');
        var parameters = aFullURL.Substring(q+1).Split(splitChars);

        foreach (var p in parameters)
        {
            int pos = p.IndexOf('=');
            if (pos > 0)
            {
                if (p.Substring(0, pos).Equals(aName)) return p.Substring(pos + 1);
            }
            else return null;
        }
        return null;
    }

    float fReinitAudioTimer = 4.0f;
    float fDelayTime = 0;

    string szReplayId;

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

        UpdateFade();

        //try reinit sound since the user must activate sound by clicking the app window
        // so it's not enough to just init once at the beginning
        fReinitAudioTimer += Time.deltaTime;
        if(fReinitAudioTimer>8.0f)
        {
            if(AudioStateMachine.instance.Init())
            {
                AudioStateMachine.instance.LevelTransition(bIsMapScene ? 1.0f : 0.0f);
                AudioStateMachine.instance.player = bIsMapScene ? GameLevel.theMap.player : null;
                AudioStateMachine.instance.SetOutput(0);
            }
            fReinitAudioTimer = 0.0f;
        }

        //the main state machine
        switch (iState)
        {
            case -3:
                //get parameters
#if UNITY_EDITOR
                //to make it run in editor where we have no web page url
                GameLevel.szLevel = "2race01";
                szReplayId = "2075437975870745";
#else
                GameLevel.szLevel = ParseURLParams(Application.absoluteURL, "Level");
                szReplayId = ParseURLParams(Application.absoluteURL, "Id");
#endif

                //by use of the EditorAutoLoad script the main scene should be loaded first
                // and should be active here ("Scenes/GameStart")
                iState++;

                StartCoroutine(LoadAsyncTileset());

                break;
            case -2:
                iState++;
                break;
            case -1:
                iState++;
                break;
            case 0:
                iState++;
                break;
            case 1:
                iState++;
                break;
            case 2:
                iState++;
                break;
            case 3:
                if (GameLevel.iLevelIndex >= 400)
                {
                    if (!LoadUserLevel()) break; //do until completion first in this state so the level can show info and minimap during this state (until play)
                }

                iState++;
                break;
            case 4:
                iState++;
                break;
            case 5:
                //menu part 2
                string szLevelToLoad = GameLevel.szLevel.Substring(1);
                if (GameLevel.iLevelIndex >= 200) szLevelToLoad = GameLevel.szLevel;

                //get replay
                StartCoroutine(oHigh.GetReplay(szLevelToLoad, szReplayId, oReplay));
                iState++;

                //set in the above, but since StartCoroutine returns before it has a chance
                // to run we need to set it
                oHigh.bIsDone = false;
                break;
            case 6:
                //menu part 2, while loading replay
                if (oHigh.bIsDone)
                {
                    iState++;
                }
                break;
            case 7:
                iState++;
                break;

            case 8:
                //begin loading the level (or replay)
                //Debug.Log("Load map Begin");
                szToLoad = "Scenes/PlayGame";
                bLoadDone = false;
                bIsMapScene = true;
                bBeginMapLoading = false;
                bLoadBeginDone = false;
                GameLevel.bMapLoaded = false;

                //always run replay
                oReplay.ResetBeforePlay();
                GameLevel.bRunReplay = true;

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

                    if (keyboard != null && keyboard.escapeKey.isPressed) //back to menu
                    {
                        bBackToMenu = true;
                    }
                    if (GameLevel.theMap.bGameOver)
                    {
                        bBackToMenu = true;
                    }

                    if (bBackToMenu)
                    {
                        StartFade(1.0f, 0.0f, true);
                        iState++;
                    }
                    break;
                }
            case 11:
                if (iFade==0) //fading done?
                {
                    theCameraHolder.InitForMenu();
                    szToLoad = "Scenes/GameStart";
                    bLoadDone = false;
                    bIsMapScene = false;
                    StartCoroutine(LoadAsyncScene());
                    fDelayTime = 0;
                    iState++;
                }
                break;
            case 12:
                //while menu is loading
                fDelayTime += Time.deltaTime;
                if (bLoadDone && fDelayTime>3.5f)
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
