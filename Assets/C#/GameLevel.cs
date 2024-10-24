﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;

//Level loading and generation
//It uses levels from a pixel/tile based map editor where a tile is 32 pixels

//I should explain why almost every number in the game has been scaled down by a factor 10...
//When the game was finished it came up in the review that there was almost no difference in the view when you moved your head in VR
// (3DoF instead of 6DoF). I had missed the fact that 1.0 units in Unity equals 1.0 meter in reality. And I saw no other way to fix
// it than to make everything 10 times smaller and bring the camera closer so that a map is a couple of meters instead of 100 meter.
// This also meant that I had to readjust ship engine power, ship mass, gravity and drag. It took 3 days.

public enum LevelType { MAP_MISSION, MAP_RACE, MAP_DOGFIGHT, MAP_MISSION_COOP }; //MAP_DOGFIGHT, MAP_MISSION_COOP not supported

struct S_TilesetInfo
{
    public S_TilesetInfo(string i_szMaterial, bool i_bRedBricks, string i_szMaterialWalls,
        string i_szMaterialBox, int i_iPlanet, int i_iTree, int i_iSkyBox)
    {
        szMaterial = i_szMaterial;
        bRedBricks = i_bRedBricks;
        szMateralWalls = i_szMaterialWalls;
        szMateralBox = i_szMaterialBox;
        iPlanet = i_iPlanet;
        iTree = i_iTree;
        iSkyBox = i_iSkyBox;
    }
    public string szMaterial;
    public bool bRedBricks;
    public string szMateralWalls;
    public string szMateralBox;
    public int iPlanet;
    public int iTree;
    public int iSkyBox;
}

public class S_EnemyInfo
{
    public S_EnemyInfo()
    {
        vWayPoints = new Vector2[10];
    }
    public int iEnemyType;
    public int iAngle;
    public int iFireInterval;
    public int iSpeed;

    public int iNumWayPoints;
    public Vector2[] vWayPoints;
}

public class S_DoorInfo
{
    public S_DoorInfo()
    {
        stButtonPos = new Vector2[3];
    }
    public Vector2 vPos;
    public float fLength;
    public bool bHorizontal;
    public float fOpenedForTime;
    public float fClosedForTime;
    public float fOpeningSpeed; //units/sec
    public float fClosingSpeed; //units/sec
    public int iNumButtons; //0 -> interval opening
    public Vector2[] stButtonPos;
}

public class GameLevel : MonoBehaviour
{
    //should really try to get rid of the statics and make the GameManager hold the object instead
    // for now this means there can be only one GameLevel...
    public static GameLevel theMap = null;
    public static string szLevel;
    public static int iLevelIndex;
    public static bool bMapLoaded = false;
    public static Replay theReplay = null;
    public static bool bRunReplay = false;

    //achievements, level complete
    bool bRunGameOverTimer = false;
    float fGameOverTimer = 0.0f;
    internal bool bGameOver = false;
    bool bPlayClipWin = false;
    bool bPlayClipGameOver = false;

    internal int iAchieveEnemiesKilled = 0;
    internal bool bAchieveFinishedMissionLevel = false;

    public GameObject oBackPlane; //used in motion movement, not visible
    public Player player;

    public float fWallHeight = .30f;
    public float fBumpHeight = 0.025f;

    static string szLevelPath0 = "Levels/";
    static string szLevelPath1 = "";

    string[] szLines;

    int[,] aMapHighres;
    int[,] aMap;
    internal EdgeCollider2D[] mapColliders;

    int iTilesetInfoIndex = 0;
    S_TilesetInfo[] m_stTilesetInfos = {
        new S_TilesetInfo("Cave_Alien", true, "Walls_Alien", "Walls_Alien", 5, 1, 6),
        new S_TilesetInfo("Cave_Evil", true, "Walls_Grey", "Walls_Grey", 3, 1, 1),
        new S_TilesetInfo("Cave_Cave", true, "Walls_Grey", "Walls_Grey", 2, 1, 4),
        new S_TilesetInfo("Cave_Cryptonite", true, "Walls_Cryptonite", "Walls_Cryptonite", 6, 3, 2),
        new S_TilesetInfo("Cave_Frost", true, "Walls_Frost", "Walls_Frost", 4, 3, 5),
        new S_TilesetInfo("Cave_Lava", false, "Walls_Lava", "Walls_Lava", 1, 2, 3),
        new S_TilesetInfo("Cave_Desert", false, "Walls_Desert", "Walls_Desert", 3, 11, 7) };

    internal Vector2 vGravity;
    internal float fDrag;

    int iWidth;
    int iHeight;
    int iMinPlayers;
    int iMaxPlayers;
    int iNumDoors;
    string szMapfile;
    string szTilefile;
    int iTileSize;
    internal int iLevelType;
    internal int iRaceLaps;

    Vector2[] stPlayerStartPos = new Vector2[8];

    const float DEFAULT_SHIPGRAVITYBASEX = 0.0f; //pixel/second 2
    const float DEFAULT_SHIPGRAVITYBASEY = 7.6f; //pixel/second 2
    const float DEFAULT_SHIPRESISTANCE = 0.68f;  //constant (velocity dependent)
    internal const float BULLETBASEVEL = 22.0f;  //pixel/second
    internal const float BULLETFREETIME = 3.1f;  //sec to be free from bullets when just come alive

    //map objects
    public LandingZone oLandingZoneObjBase;
    internal List<LandingZone> aLandingZoneList;

    public CheckPoint oCheckPointObjBase;
    internal List<CheckPoint> aCheckPointList;

    public Enemy oEnemyObjBase;
    internal List<Enemy> aEnemyList;
    public int GetNumEnemiesNearPlayer()
    {
        int iNumEnemies = 0;

        for (int i=0; i< aEnemyList.Count; i++)
        {
            if (aEnemyList[i] != null)
            {
                Vector2 vDist = new Vector2(aEnemyList[i].vPos.x, aEnemyList[i].vPos.y) - this.player.GetPosition();
                if (vDist.magnitude < (450 / 32.0f /10.0f)) iNumEnemies++; //~14 tiles, same as fire range
            }
        }
        return iNumEnemies;
    }
    public int GetNumBulletsNearPlayer()
    {
        int iNumBullets = 0;

        Bullet[] aBulletList = GetComponentsInChildren<Bullet>();
        for (int i = 0; i < aBulletList.Length; i++)
        {
            if (aBulletList[i].name.StartsWith("BulletE"))
            {
                Vector3 v = aBulletList[i].oCube.transform.position;

                Vector2 vDist = new Vector2(v.x, v.y) - this.player.GetPosition();
                if (vDist.magnitude < (256 / 32.0f /10.0f)) iNumBullets++; //~8 tiles
            }
        }
        return iNumBullets;
    }

    public GameObject oBulletObjBase;

    public Door oDoorObjBase;
    internal List<Door> aDoorList;

    public FlyingScore oFlyingScoreObjBase;

    //static noncollidable objects
    List<GameObject> aDecorationList; //single bricks, brick wall left,center,right
    public ZObject oZObjBase;
    public Decoration oDecorationObjBase; //barrels, trees

    public Material oSkyBoxMat1;
    public Material oSkyBoxMat2;
    public Material oSkyBoxMat3;
    public Material oSkyBoxMat4;
    public Material oSkyBoxMat5;
    public Material oSkyBoxMat6;
    public Material oSkyBoxMat7;
    public Planet oPlanet;

    public AudioClip oClipLevelStart;
    public AudioClip oClipLevelGameOver;
    public AudioClip oClipLevelWin;

    MeshGenerator oMeshGen;
    Material oMaterialWalls; //set when walls are created
    Material oMaterial;

    public GameLevel()
    {
        theMap = this;
    }

    int iFinalizeCounter;
    void Start()
    {
        bGameOver = false;

        iFinalizeCounter = 0;

        Material oMaterialBox = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMateralBox, typeof(Material)) as Material;
        //make back plane and border
        oMeshGen.map0_bg.GetComponent<MeshRenderer>().material = oMaterialBox;
        Vector3 vSize = GetMapSize();
        GameObject oObj;
        //left
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(-vSize.x / 20 - 0.05f, 0, .20f);
        oObj.transform.localScale = new Vector3(.10f, vSize.y / 10, vSize.z + .6f);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //right
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(vSize.x / 20 + 0.05f, 0, .20f);
        oObj.transform.localScale = new Vector3(.10f, vSize.y / 10, vSize.z + .6f);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //top
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, vSize.y / 20 + 0.05f, .20f);
        oObj.transform.localScale = new Vector3(vSize.x / 10 + .20f, .10f, vSize.z + .6f);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //bottom
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, -vSize.y / 20 - 0.05f, .20f);
        oObj.transform.localScale = new Vector3(vSize.x / 10 + .20f, .10f, vSize.z + .6f);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;

        //play area for motion movement, not visible
        oBackPlane.transform.localScale = new Vector3(vSize.x / 100, 1.0f, vSize.y / 100);

        //set new skybox
        int iSkyBox = m_stTilesetInfos[iTilesetInfoIndex].iSkyBox; //UnityEngine.Random.Range(1, 7);
        switch (iSkyBox)
        {
            case 1: RenderSettings.skybox = oSkyBoxMat1; break;
            case 2: RenderSettings.skybox = oSkyBoxMat2; break;
            case 3: RenderSettings.skybox = oSkyBoxMat3; break;
            case 4: RenderSettings.skybox = oSkyBoxMat4; break;
            case 5: RenderSettings.skybox = oSkyBoxMat5; break;
            case 6: RenderSettings.skybox = oSkyBoxMat6; break;
            case 7: RenderSettings.skybox = oSkyBoxMat7; break;
        }

        Debug.Log("Start done - continue load in Update");
    }

    Thread thread;
    ManualResetEvent oEvent = new ManualResetEvent(false);
    bool bMeshBkReady = false;
    void LoadThread()
    {
        Debug.Log("Load thread start");
        //create background plane first of all
        oMeshGen.GenerateMeshBackground(iWidth, iHeight, 1.10f, 0.105f, 1.0f);

        bMeshBkReady = true;
        oEvent.WaitOne();

        //generate new high res map based on the textures of the tiles
        int substeps = 6;
        float pixelsamplepos = 32.0f / substeps;
        LoadGrid(substeps, pixelsamplepos);

        //generate final mesh, set tile material
        oMeshGen.GenerateMeshInit(aMapHighres, 0.10f / substeps, fWallHeight, fBumpHeight, oMaterial, oMaterialWalls);

        oMeshGen.GenerateMesh();

        //create and set the walls mesh
        oMeshGen.CalculateMeshOutlines();

        oMeshGen.CreateWallMesh();

        //create bumps on non outline vertices in the map mesh
        oMeshGen.CreateBumps();

        oMeshGen.GenerateUvs();

        bMapLoaded = true;
        Debug.Log("Load thread done");
    }

    int iLoadBeginState = 0;
    bool[,] oTileSet;
    byte[] bytes;
    public bool LoadBegin()
    {
        bool bIsCustom = iLevelIndex >= 200 && iLevelIndex < 400;
        bool bIsCustom2 = iLevelIndex >= 400;
        int substeps = 6;
        if (iLoadBeginState == 0)
        {
            bMapLoaded = false;
            Enemy.iOwnerIdBase = 10;

            //load des pass 1 (unity: must be done from main thread)
            Debug.Log("Loading level: " + szLevel);
            LoadDesPass1(szLevel, bIsCustom, bIsCustom2);
            oMeshGen = GetComponent<MeshGenerator>();

            iLoadBeginState++;
        }
        else if (iLoadBeginState == 1)
        {
            //note: not working, loaded from GameManager now because the same code does not work from here!!
            //load tileset (unity: must be done from main thread)
            //string szPngTileset = szTilefile.Remove(szTilefile.LastIndexOf('.')) + ".png";
            //LoadTileSetBegin(szPngTileset);

            iLoadBeginState++;
        }
        else if (iLoadBeginState == 2)
        {
            if (LoadTileSetFinalize()) iLoadBeginState++;
        }
        else if (iLoadBeginState >= 3 && iLoadBeginState <= 8)
        {
            //load tileset to bool array (unity: must be done from main thread)
            // this is to avoid doing GetPixel later (GetPixels instead of GetPixel didn't save time)
            int segs = oTileTexture.height / 6;
            int n2 = iLoadBeginState - 3;

            if (n2 == 0) oTileSet = new bool[oTileTexture.height, oTileTexture.width];
            for (int y = n2 * segs; y < (n2 < 5 ? (n2 + 1) * segs : oTileTexture.height); y++)
            {
                for (int x = 0; x < oTileTexture.width; x++)
                {
                    bool bPixel = oTileTexture.GetPixel(x, y) != Color.black;
                    oTileSet[y, x] = bPixel;
                }
            }

            iLoadBeginState++;
        }
        else if (iLoadBeginState == 9)
        {
            //load materials (unity: must be done from main thread)
            oMaterial = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMaterial, typeof(Material)) as Material;
            oMaterialWalls = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMateralWalls, typeof(Material)) as Material;

            iLoadBeginState++;
        }
        else if (iLoadBeginState == 10)
        {
            //load map file (unity: must be done from main thread)
            aMap = new int[iHeight, iWidth];
            //load tile numbers
            if (bIsCustom2)
            {
                bytes = GameManager.theGM.oHigh.aLevelBinaryData;
            }
            else if (bIsCustom)
            {
                bytes = File.ReadAllBytes(Application.persistentDataPath + "/" + szMapfile);
            }
            else
            {
                string szFilenameNoExt = szMapfile.Remove(szMapfile.LastIndexOf('.'));
                TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
                if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
                //if (f == null) return false;
                bytes = f.bytes;
            }

            for (int y = 0; y < iHeight; y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    aMap[iHeight - 1 - y, x] = bytes[y * iWidth + x];
                }
            }
            aMapHighres = new int[iHeight * substeps, iWidth * substeps];

            iLoadBeginState++;
        }
        else if (iLoadBeginState == 11)
        {
            //create thread for all other work that can be done
            // (before needing work in main thread, handled in LoadDone() and thread)
            ThreadStart ts = new ThreadStart(LoadThread);
            thread = new Thread(ts);
            thread.Priority = System.Threading.ThreadPriority.Lowest;
            thread.Start();

            iLoadBeginState = 0;
            return true;
        }

        return false;
    }

    public bool LoadDone()
    {
        if (bMeshBkReady)
        {
            oMeshGen.SetGenerateMeshBackgroundToUnity();

            bMeshBkReady = false;
            oEvent.Set();
        }

        return !thread.IsAlive;
    }

    void Update()
    {
        //finalize the loading in the first few frames after Start()
        if (iFinalizeCounter == 0)
        {
            //pause physics
            Time.timeScale = 0.0f;

            LoadDesPass2Init();
            iFinalizeCounter++;
        }
        else if (iFinalizeCounter == 1)
        {
            //done: LoadDesPass2 is split in one iteration per object that needs loading
            // (max 5 ms each, but there may be variations)
            bool bFinished = LoadDesPass2();

            if (bFinished)
            {
                iFinalizeCounter++;
            }
        }
        else if (iFinalizeCounter >= 2 && iFinalizeCounter <= 4)
        {
            oMeshGen.GenerateMeshFinalize(iFinalizeCounter - 2);
            mapColliders = gameObject.GetComponents<EdgeCollider2D>();
            for (int i = 0; i < mapColliders.Length; i++)
            {
                mapColliders[i].edgeRadius = 0.000002f;
            }
            iFinalizeCounter++;
        }
        else if (iFinalizeCounter >= 5 && iFinalizeCounter <= 9)
        {
            int n2 = iFinalizeCounter - 5;
            int segs = iHeight / 5;

            for (int y = n2 * segs; y < (n2 < 4 ? (n2 + 1) * segs : iHeight); y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    ReplaceAndAddObjectPass2(x, y);
                }
            }
            iFinalizeCounter++;
        }
        else if (iFinalizeCounter == 10)
        {
            oPlanet.Init(m_stTilesetInfos[iTilesetInfoIndex].iPlanet);
            GetComponent<AudioSource>().PlayOneShot(oClipLevelStart, GameManager.theGM.fMasterVolMod);

            //(user name not currently used in Player)
            string szUser = GameManager.szUser == null ? "Incognito" : GameManager.szUser;
            player.Init(szUser, 0, stPlayerStartPos[0], this, CameraController.bPointMovement);

            //this must be done after init player
            //so best do it the same time as the level has finished popping up
            GameManager.theGM.cameraHolder.InitForGame(GameLevel.theMap, GameLevel.theMap.player.gameObject);

            GameManager.theGM.StartFade(0.5f, 0.5f, false);

            iFinalizeCounter++;
        }
        else if (iFinalizeCounter == 11)
        {
            //let the fade in finish
            //do not display on top objects while fade in
            if (!GameManager.theGM.oFadeBox.activeSelf)
            {
                player.gameObject.SetActive(true); //the windscreen (glass material)
                player.status.gameObject.SetActive(true); //status bar objects

                iFinalizeCounter++;
            }
        }
        else if (iFinalizeCounter == 12)
        {
            //time back to normal
            Time.timeScale = 1.0f;

            iFinalizeCounter++;
            Debug.Log("Load complete");
        }
        //end of init code

        if (!bMapLoaded || iFinalizeCounter <= 12) return;

        //motion controller movement
        if (CameraController.bPointMovement)
        {
            //raycast
            CameraController oCC = GameManager.theGM.cameraHolder;
            RaycastHit oHitInfo;
            if (Physics.Raycast(oCC.vHeadPosition, oCC.vGazeDirection, out oHitInfo, 400.0f, LayerMask.GetMask("MapPlane")))
            {
                //a hit, place cursor on object, show ray
                oCC.SetPointingInfo(oHitInfo.point, Quaternion.identity, oCC.vHeadPosition, oCC.qRotation);

                //set steering to oHitInfo.point
                player.vSteerToPoint = new Vector2(oHitInfo.point.x, oHitInfo.point.y);
            }
            else
            {
                //set at max distance
                Vector3 vPoint = (oCC.vHeadPosition + oCC.vGazeDirection * 17.0f);

                oCC.SetPointingInfo(vPoint, oCC.qRotation, oCC.vHeadPosition, oCC.qRotation);
            }
        }

        //game end conditions
        if (!bRunGameOverTimer)
        {
            //race finished
            if(player.bAchieveFinishedRaceLevel)  {
                bRunGameOverTimer = true;
            }
            //mission finished (won)
            if(GetMissionFinished())
            {
                bRunGameOverTimer = true;
                bAchieveFinishedMissionLevel = true;
                bPlayClipWin = true;
            }
            //mission finished (lost)
            if (iLevelType == (int)LevelType.MAP_MISSION && player.iNumLifes==0)
            {
                bRunGameOverTimer = true;
                bPlayClipGameOver = true;
            }
        }
        if (bRunGameOverTimer) {
            fGameOverTimer += Time.deltaTime;

            //delay sounds 0.5 sec
            if(bPlayClipWin && fGameOverTimer > 0.5f)
            {
                GetComponent<AudioSource>().PlayOneShot(oClipLevelWin, GameManager.theGM.fMasterVolMod);
                bPlayClipWin = false;
            }
            if (bPlayClipGameOver && fGameOverTimer > 0.5f)
            {
                GetComponent<AudioSource>().PlayOneShot(oClipLevelGameOver, GameManager.theGM.fMasterVolMod);
                bPlayClipGameOver = false;
            }

            if (fGameOverTimer > 6.0f)
            {
                bGameOver = true;

                //do not display on top objects while fade out
                if (GameManager.theGM.oFadeBox.activeSelf)
                {
                    player.gameObject.SetActive(false); //the windscreen (glass material)
                    player.status.gameObject.SetActive(false); //status bar objects
                }
            }
        }
    }

    private bool GetMissionFinished()
    {
        if (iLevelType != (int)LevelType.MAP_MISSION) return false;

        if(player.GetCargoLoaded() != 0) return false;
        for(int i=0; i<aLandingZoneList.Count; i++)
        {
            if (aLandingZoneList[i].GetTotalCargo() != 0) return false;
        }
        return true;
    }

    private void FixedUpdate()
    {
    }

    //old pixels pos (pos written in map des file) to new pos
    Vector2 AdjustPosition(Vector2 i_vPixelPos, Vector2 i_vPixelSize)
    {
        return new Vector2(i_vPixelPos.x / 32.0f - iWidth * 0.5f + (i_vPixelSize.x / 32.0f) * 0.5f,
            (iHeight * 32.0f - i_vPixelPos.y - i_vPixelSize.y * 0.5f) / 32.0f - iHeight * 0.5f) / 10;
    }
    //old pixels pos (pos from tile position) to new pos
    Vector2 AdjustPositionNoFlip(Vector2 i_vPixelPos, Vector2 i_vPixelSize)
    {
        return new Vector2(i_vPixelPos.x / 32.0f - iWidth * 0.5f + (i_vPixelSize.x / 32.0f) * 0.5f,
            (i_vPixelPos.y - i_vPixelSize.y * 0.5f) / 32.0f - iHeight * 0.5f) / 10;
    }

    public LandingZone GetLandingZone(int i_iId)
    {
        for (int i = 0; i < aLandingZoneList.Count; i++)
            if (aLandingZoneList[i].iId == i_iId) return aLandingZoneList[i];
        return null;
    }

    public Vector3 GetMapSize()
    {
        return new Vector3(iWidth * 1.0f, iHeight * 1.0f, fWallHeight);
    }

    public void DoorToggleOpenClose(int i_iDoorId)
    {
        aDoorList[i_iDoorId].ToggleOpenClose();
    }

    public static string FindMapFile(string szHeader)
    {
        string[] szLines = szHeader.Split((char)10);

        int iLineIndex = -1;
        while (iLineIndex < szLines.Length - 1)
        {
            iLineIndex++;
            char[] szSeparator = { (char)32 };
            string[] szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (szTokens.Length == 0) continue;
            if (szTokens[0].Length == 0) continue;
            if (!szTokens[0].StartsWith("*")) continue;

            if (szTokens[0].CompareTo("*MAPFILE") == 0)
            {
                return szTokens[1];
            }
        }
        return null;
    }

    //gets only the info needed to generate the map mesh
    bool LoadDesPass1(string i_szFilename, bool bIsCustom, bool bIsCustom2)
    {
        //free old map if exists
        iLevelType = (int)LevelType.MAP_MISSION;

        String szFileText;
        if (bIsCustom2)
        {
            szFileText = GameManager.theGM.oHigh.szLevelTextData;
        }
        else if (bIsCustom)
        {
            szFileText = System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(Application.persistentDataPath + "/" + i_szFilename));
        }
        else
        {
            int iPos = i_szFilename.LastIndexOf('.');
            string szFilenameNoExt = i_szFilename;
            if (iPos > 0) szFilenameNoExt = i_szFilename.Remove(iPos);
            TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
            if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
            if (f == null) return false;

            szFileText = System.Text.Encoding.UTF8.GetString(f.bytes);
        }

        szLines = szFileText.Split((char)10);

        int iLineIndex = -1;
        while (iLineIndex < szLines.Length - 1)
        {
            iLineIndex++;
            char[] szSeparator = { (char)32 };
            string[] szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (szTokens.Length == 0) continue;
            if (szTokens[0].Length == 0) continue;
            if (!szTokens[0].StartsWith("*")) continue;

            if (szTokens[0].CompareTo("*MAPSIZE") == 0)
            {
                iWidth = int.Parse(szTokens[1]);
                iHeight = int.Parse(szTokens[2]);
            }
            else if (szTokens[0].CompareTo("*MAPFILE") == 0)
            {
                szMapfile = szTokens[1];
            }
            else if (szTokens[0].CompareTo("*TILESIZE") == 0)
            {
                iTileSize = int.Parse(szTokens[1]);
            }
            else if (szTokens[0].CompareTo("*TILESET") == 0)
            {
                szTilefile = szTokens[1];

                if (szTilefile.CompareTo("ts_alien.tga") == 0) iTilesetInfoIndex = 0;
                else if (szTilefile.CompareTo("ts_evil.tga") == 0) iTilesetInfoIndex = 1;
                else if (szTilefile.CompareTo("ts_cave.tga") == 0) iTilesetInfoIndex = 2;
                else if (szTilefile.CompareTo("ts_cryptonite.tga") == 0) iTilesetInfoIndex = 3;
                else if (szTilefile.CompareTo("ts_frost.tga") == 0) iTilesetInfoIndex = 4;
                else if (szTilefile.CompareTo("ts_lava.tga") == 0) iTilesetInfoIndex = 5;
                else if (szTilefile.CompareTo("ts_desert.tga") == 0) iTilesetInfoIndex = 6;
            }

        }
        //header loaded

        return true;
    }

    //creates all objects
    void LoadDesPass2Init()
    {
        //free old map if exists
        iLevelType = (int)LevelType.MAP_MISSION;
        iRaceLaps = 0;
        iMinPlayers = 1;
        iMaxPlayers = 1;
        iNumDoors = 0;

        vGravity = new Vector2(DEFAULT_SHIPGRAVITYBASEX, -DEFAULT_SHIPGRAVITYBASEY);
        fDrag = DEFAULT_SHIPRESISTANCE;

        aLandingZoneList = new List<LandingZone>();
        aCheckPointList = new List<CheckPoint>();
        aEnemyList = new List<Enemy>();
        aDoorList = new List<Door>();
        aDecorationList = new List<GameObject>();

        iLineIndex = -1;
        iStartPosCounter = 0;
        iZoneCounter = 0;
    }
    int iLineIndex;
    int iStartPosCounter;
    int iZoneCounter;
    bool LoadDesPass2()
    {
        //des file is in szLines after LoadDesPass1()

        //to parse 0.00 as float on any system
        CultureInfo ci = new CultureInfo("en-US");
        bool bFinished = true;

        while (iLineIndex < szLines.Length - 1)
        {
            iLineIndex++;
            char[] szSeparator = { (char)32 };
            string[] szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (szTokens.Length == 0) continue;
            if (szTokens[0].Length == 0) continue;
            if (!szTokens[0].StartsWith("*")) continue;

            bFinished = false;
            if (szTokens[0].CompareTo("*MAPTYPE") == 0)
            {
                if (szTokens[1].CompareTo("RACE") == 0)
                {
                    iLevelType = (int)LevelType.MAP_RACE;
                    iRaceLaps = int.Parse(szTokens[2]);
                }
                else if (szTokens[1].CompareTo("MISSION") == 0)
                {
                    iLevelType = (int)LevelType.MAP_MISSION;
                }
                else if (szTokens[1].CompareTo("MISSION_COOP") == 0)
                {
                    iLevelType = (int)LevelType.MAP_MISSION_COOP;
                }
                else if (szTokens[1].CompareTo("DOGFIGHT") == 0)
                {
                    if (iMinPlayers < 2) iMinPlayers = 2;
                    iLevelType = (int)LevelType.MAP_DOGFIGHT;
                }
            }
            else if (szTokens[0].CompareTo("*MAPSIZE") == 0)
            {
                iWidth = int.Parse(szTokens[1]);
                iHeight = int.Parse(szTokens[2]);
            }
            else if (szTokens[0].CompareTo("*MAPFILE") == 0)
            {
                szMapfile = szTokens[1];
            }
            else if (szTokens[0].CompareTo("*TILESIZE") == 0)
            {
                iTileSize = int.Parse(szTokens[1]);
            }
            else if (szTokens[0].CompareTo("*TILESET") == 0)
            {
                szTilefile = szTokens[1];

                if (szTilefile.CompareTo("ts_alien.tga") == 0) iTilesetInfoIndex = 0;
                else if (szTilefile.CompareTo("ts_evil.tga") == 0) iTilesetInfoIndex = 1;
                else if (szTilefile.CompareTo("ts_cave.tga") == 0) iTilesetInfoIndex = 2;
                else if (szTilefile.CompareTo("ts_cryptonite.tga") == 0) iTilesetInfoIndex = 3;
                else if (szTilefile.CompareTo("ts_frost.tga") == 0) iTilesetInfoIndex = 4;
                else if (szTilefile.CompareTo("ts_lava.tga") == 0) iTilesetInfoIndex = 5;
                else if (szTilefile.CompareTo("ts_desert.tga") == 0) iTilesetInfoIndex = 6;
            }
            else if (szTokens[0].CompareTo("*GRAVITY") == 0)
            {
                vGravity.x = float.Parse(szTokens[1], ci.NumberFormat) / 9.2f;
                vGravity.y = -float.Parse(szTokens[2], ci.NumberFormat) / 9.2f;
            }
            else if (szTokens[0].CompareTo("*RESISTANCE") == 0)
            {
                fDrag = float.Parse(szTokens[1], ci.NumberFormat);
                //vDrag.y = float.Parse(szTokens[2]); //no support in physics engine
            }
            else if (szTokens[0].CompareTo("*PLAYERSTARTPOS") == 0)
            {
                if (iStartPosCounter < 8)
                {
                    int x = int.Parse(szTokens[1]);
                    int y = int.Parse(szTokens[2]);
                    stPlayerStartPos[iStartPosCounter] = AdjustPosition(new Vector2(x, y), new Vector2(48, 48));
                }
                iStartPosCounter++;
            }
            else if (szTokens[0].CompareTo("*MAXPLAYERS") == 0)
            {
                iMaxPlayers = int.Parse(szTokens[1]);
            }
            else if (szTokens[0].CompareTo("*MINPLAYERS") == 0)
            {
                iMinPlayers = int.Parse(szTokens[1]);
            }
            else if (szTokens[0].CompareTo("*LANDINGZONE") == 0)
            {
                int x, y, w, iNumCargo = 0;
                List<int> aCargoList = new List<int>();

                x = int.Parse(szTokens[1]);
                y = int.Parse(szTokens[2]);
                w = int.Parse(szTokens[3]);

                bool bHomeBase = szTokens[4].CompareTo("HOMEBASE") == 0;
                bool bExtraLife = szTokens[5].CompareTo("EXTRALIFE") == 0;
                bool bTower = szTokens[6].CompareTo("ANTENNA") == 0;
                bool bHangar = szTokens[7].CompareTo("WAREHOUSE") == 0;
                if (szTokens.Length > 8) iNumCargo = int.Parse(szTokens[8]);

                for (int i = 0; i < iNumCargo; i++)
                {
                    aCargoList.Add(int.Parse(szTokens[9 + i]));
                }

                Vector2 vPos = AdjustPosition(new Vector2(x, y), new Vector2(32 * w, 4));
                LandingZone oZone = Instantiate(oLandingZoneObjBase, this.transform);
                oZone.Init(iZoneCounter++, vPos, w, fWallHeight,
                    bHomeBase, bTower, bHangar, !bHangar && (iLevelType != (int)LevelType.MAP_RACE), aCargoList, bExtraLife);
                aLandingZoneList.Add(oZone);

                break;
            }
            else if (szTokens[0].CompareTo("*CHECKPOINTS") == 0)
            {
                int iNumCP = int.Parse(szTokens[1]);

                //read checkpoints
                for (int i = 0; i < iNumCP; i++)
                {
                    iLineIndex++;
                    szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);

                    Vector2 vPos1 = new Vector2(int.Parse(szTokens[1]), int.Parse(szTokens[2]));
                    Vector2 vPos2 = new Vector2(int.Parse(szTokens[3]), int.Parse(szTokens[4]));

                    Vector2 vPos1Adj = AdjustPosition(vPos1, new Vector2(20, 16));
                    Vector2 vPos2Adj = AdjustPosition(vPos2, new Vector2(20, 16));

                    CheckPoint oCP = Instantiate(oCheckPointObjBase, this.transform);
                    oCP.Init(vPos1Adj, vPos2Adj, i);
                    aCheckPointList.Add(oCP);
                    if (i == 0) oCP.SetBlinkState(true); //first CP is the one to get first
                }
                break;
            }
            else if (szTokens[0].CompareTo("*DOOR") == 0)
            {
                S_DoorInfo stParams = new S_DoorInfo();

                stParams.vPos.x = int.Parse(szTokens[1]);
                stParams.vPos.y = int.Parse(szTokens[2]);
                stParams.fLength = (int.Parse(szTokens[3])) / 32.0f;

                //command "*ANGLE"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.bHorizontal = int.Parse(szTokens[1]) != 90 && int.Parse(szTokens[1]) != 270;

                if (!stParams.bHorizontal) stParams.vPos = AdjustPosition(stParams.vPos, new Vector2(52, 36));
                else stParams.vPos = AdjustPosition(stParams.vPos, new Vector2(36, 52));

                //command "*OPENTIME"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.fOpenedForTime = (float)(int.Parse(szTokens[1]) / 1000.0f);
                //command "*CLOSETIME"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.fClosedForTime = (float)(int.Parse(szTokens[1]) / 1000.0f);
                //command "*OPENSPEED"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.fOpeningSpeed = (float)(int.Parse(szTokens[1]) / 32.0f);
                //command "*CLOSESPEED"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.fClosingSpeed = (float)(int.Parse(szTokens[1]) / 32.0f);
                //command "*NUMBUTTONS"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.iNumButtons = int.Parse(szTokens[1]);
                //command "*BUTTONSX"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stParams.iNumButtons; i++)
                    stParams.stButtonPos[i].x = int.Parse(szTokens[i + 1]);
                //command "*BUTTONSY"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stParams.iNumButtons; i++)
                    stParams.stButtonPos[i].y = int.Parse(szTokens[i + 1]);
                //make new coords
                for (int i = 0; i < stParams.iNumButtons; i++)
                    stParams.stButtonPos[i] = AdjustPosition(stParams.stButtonPos[i], new Vector2(24, 18));

                //add door to list
                Door oDoor = Instantiate(oDoorObjBase, this.transform);
                oDoor.Init(stParams, iNumDoors, this);
                aDoorList.Add(oDoor);
                iNumDoors++; //id

                break;
            }
            else if (szTokens[0].CompareTo("*ZOBJECT") == 0)
            {
                int x, y, iAngle, iType;
                iType = int.Parse(szTokens[1]);
                x = int.Parse(szTokens[2]);
                y = int.Parse(szTokens[3]);
                iAngle = int.Parse(szTokens[4]);

                Vector2 vPos = Vector3.zero;
                if (iType == 0)
                {
                    if (iAngle == 0 || iAngle == 180) vPos = AdjustPosition(new Vector2(x, y), new Vector2(32 * 4, 12));
                    else vPos = AdjustPosition(new Vector2(x, y), new Vector2(12, 32 * 4));
                }
                else if (iType == 1)
                {
                    if (iAngle == 0 || iAngle == 180) vPos = AdjustPosition(new Vector2(x, y), new Vector2(32 * 8, 12));
                    else vPos = AdjustPosition(new Vector2(x, y), new Vector2(12, 32 * 8));
                }
                else //2, 3
                {
                    if (iAngle == 0 || iAngle == 180) vPos = AdjustPosition(new Vector2(x, y), new Vector2(32 * 12, 16));
                    else vPos = AdjustPosition(new Vector2(x, y), new Vector2(16, 32 * 12));
                }

                ZObject oZObj = Instantiate(oZObjBase, this.transform);
                oZObj.Init(iType, iAngle, vPos);

                break;
            }
            else if (szTokens[0].CompareTo("*ENEMY") == 0)
            {
                S_EnemyInfo stEnemy = new S_EnemyInfo();
                stEnemy.iEnemyType = int.Parse(szTokens[1]);

                //command "*ANGLE"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stEnemy.iAngle = int.Parse(szTokens[1]);

                //command "*FIREINTERVAL"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stEnemy.iFireInterval = int.Parse(szTokens[1]);

                //command "*SPEED"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stEnemy.iSpeed = int.Parse(szTokens[1]);

                //command "*NUMWAYPOINTS"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stEnemy.iNumWayPoints = int.Parse(szTokens[1]);

                //command "*WAYPOINTSX"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stEnemy.iNumWayPoints; i++)
                    stEnemy.vWayPoints[i].x = int.Parse(szTokens[i + 1]);

                //command "*WAYPOINTSY"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stEnemy.iNumWayPoints; i++)
                    stEnemy.vWayPoints[i].y = int.Parse(szTokens[i + 1]);

                for (int i = 0; i < stEnemy.iNumWayPoints; i++)
                {
                    switch (stEnemy.iEnemyType)
                    {
                        case 0:
                        case 2:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(16, 12));
                                stEnemy.vWayPoints[i].x -= 4.5f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(12, 16));
                                stEnemy.vWayPoints[i].y -= 4.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(16, 12));
                                stEnemy.vWayPoints[i].x += 9.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(12, 16));
                                stEnemy.vWayPoints[i].y += 7.0f / 32.0f /10.0f;
                                stEnemy.vWayPoints[i].x += 3.0f / 32.0f /10.0f;
                            }
                            break;
                        case 1:
                        case 3:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x -= 4.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y -= 2.5f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x += 6.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y += 4.0f / 32.0f /10.0f;
                            }
                            break;
                        case 4:
                            stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(64, 32));
                            break;
                        case 5:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x -= 5.5f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y -= 5.5f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x += 5.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y += 6.0f / 32.0f /10.0f;
                            }
                            break;
                        case 6:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].x -= 6.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].y -= 6.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].x += 6.0f / 32.0f /10.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].y += 2.0f / 32.0f /10.0f;
                            }
                            break;
                    }
                }
                if (stEnemy.iAngle == 270)
                {
                    stEnemy.iAngle = 90;
                }
                else if (stEnemy.iAngle == 90)
                {
                    stEnemy.iAngle = 270;
                }

                Enemy oEnemy = Instantiate(oEnemyObjBase, this.transform);
                oEnemy.Init(stEnemy, this);
                aEnemyList.Add(oEnemy);

                break;
            }

            //header row loaded
            bFinished = true; //assume true if the loop will end the next time
        }
        //header loaded or row loaded
        return bFinished;
    }

    //removes incompatible objects (before mesh is generated)
    void ReplaceAndAddObjectPass1(int x, int y)
    {
        int iTile = aMap[y, x];
        switch (iTile)
        {
            //tree1
            //case 62: aMap[y, x] = 17; break;
            //tree2
            //case 63: aMap[y, x] = 24; break;
            //barrels
            //case 64: aMap[y, x] = 17; break;

            //radio tower
            case 76: aMap[y, x] = 0; break;
            //case 77: aMap[y, x] = 17; break;
            //parts of brick objects
            case 38: aMap[y, x] = 0; break;
        }
    }

    //add former objects in tile as 3D objects, and decoration tiles
    void ReplaceAndAddObjectPass2(int x, int y)
    {
        int iTile = aMap[y, x];
        switch (iTile)
        {
            case 49:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 26.0f, y * 32.0f + (32.0f - 8.0f)), new Vector2(16.0f, 8.0f));

                    GameObject oObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    oObj.transform.parent = transform;

                    Material oMaterial;
                    if (m_stTilesetInfos[iTilesetInfoIndex].bRedBricks)
                        oMaterial = Resources.Load("RedBricks", typeof(Material)) as Material;
                    else oMaterial = Resources.Load("GreyBricks", typeof(Material)) as Material;

                    oObj.transform.position = new Vector3(vPos.x, vPos.y, -.15f - fBumpHeight);
                    oObj.transform.localScale = new Vector3(16.0f / 320.0f, 8.0f / 320.0f, 1.0f);
                    oObj.GetComponent<MeshRenderer>().material = oMaterial;

                    aDecorationList.Add(oObj);
                    break;
                }
            case 35:
            case 36:
            case 37:
            case 51:
            case 52:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 0.0f, y * 32.0f + (32.0f - 0.0f)), new Vector2(32.0f, 32.0f));

                    GameObject oObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    oObj.transform.parent = transform;

                    Material oMaterial = null;
                    if (m_stTilesetInfos[iTilesetInfoIndex].bRedBricks)
                    {
                        if (iTile == 35 || iTile == 52) oMaterial = Resources.Load("RedBricks_left", typeof(Material)) as Material;
                        if (iTile == 36) oMaterial = Resources.Load("RedBricks_center", typeof(Material)) as Material;
                        if (iTile == 37 || iTile == 51) oMaterial = Resources.Load("RedBricks_right", typeof(Material)) as Material;
                    }
                    else
                    {
                        if (iTile == 35 || iTile == 52) oMaterial = Resources.Load("GreyBricks_left", typeof(Material)) as Material;
                        if (iTile == 36) oMaterial = Resources.Load("GreyBricks_center", typeof(Material)) as Material;
                        if (iTile == 37 || iTile == 51) oMaterial = Resources.Load("GreyBricks_right", typeof(Material)) as Material;
                    }

                    //there are no or only negative bumps in the mesh on these tiles, so we need not offset z by -fBumpHeight,
                    // still we offset it by a little to not collide with the mesh that is under it
                    oObj.transform.position = new Vector3(vPos.x, vPos.y, -.15f - 0.008f);
                    oObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    oObj.GetComponent<MeshRenderer>().material = oMaterial;

                    aDecorationList.Add(oObj);
                    break;
                }
            //trees
            case 62:
            case 63:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 11.0f, y * 32.0f + (32.0f + 5.0f)), new Vector2(32.0f, 32.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    oDObj.Init(m_stTilesetInfos[iTilesetInfoIndex].iTree, vPos);
                    break;
                }
            //barrels
            case 64:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 1.0f, y * 32.0f + (32.0f + 7.0f)), new Vector2(64.0f, 32.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    oDObj.Init(0, vPos);
                    break;
                }
            //house
            case 59:
            case 73:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 18.0f, y * 32.0f + (32.0f + 7.0f)), new Vector2(32.0f, 32.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    if (iTile < 73) oDObj.Init(4 + (iTile - 59), vPos);
                    else oDObj.Init(7 + (iTile - 73), vPos);
                    break;
                }
            case 60:
            case 74:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f + 0.0f, y * 32.0f + (32.0f + 7.0f)), new Vector2(32.0f, 32.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    if (iTile < 73) oDObj.Init(4 + (iTile - 59), vPos);
                    else oDObj.Init(7 + (iTile - 73), vPos);
                    break;
                }
            case 61:
            case 75:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f - 18.0f, y * 32.0f + (32.0f + 7.0f)), new Vector2(32.0f, 32.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    if (iTile < 73) oDObj.Init(4 + (iTile - 59), vPos);
                    else oDObj.Init(7 + (iTile - 73), vPos);
                    break;
                }
            //radio tower
            case 77:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f - 0.0f, y * 32.0f + (32.0f + 12.0f)), new Vector2(32.0f, 64.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    oDObj.Init(10, vPos);
                    break;
                }
        }
    }

    void LoadGrid(int substeps, float pixelsamplepos)
    {
        for (int y = 0; y < iHeight; y++)
        {
            for (int x = 0; x < iWidth; x++)
            {
                ReplaceAndAddObjectPass1(x, y);
                int iTile = aMap[y, x];
                Rect r = stTileRects[iTile];
                for (int y2 = 0; y2 < substeps; y2++)
                {
                    for (int x2 = 0; x2 < substeps; x2++)
                    {
                        int posx = (int)r.position.x + (int)(x2 * pixelsamplepos + pixelsamplepos / 2);
                        int posy = (int)r.position.y + (int)(y2 * pixelsamplepos + pixelsamplepos / 2);
                        //sample every pixelsamplepos pixel in the tile texture
                        aMapHighres[y * substeps + y2, x * substeps + x2] = oTileSet[posy, posx] ? iTile : 0;
                        //aMapHighres[y * substeps + y2, x * substeps + x2] = iTile;
                    }
                }
            }
        }
    }

    //not working
    void LoadTileSetBegin(string i_szFilename)
    {
        szTilesetFilename = szLevelPath0 + i_szFilename.Remove(szTilefile.LastIndexOf('.'));
        StartCoroutine(LoadAsyncTileset());
    }
    string szTilesetFilename;
    public Texture2D oTileTexture;
    public bool bTilesetLoaded = false;
    IEnumerator LoadAsyncTileset()
    {
        ResourceRequest loadAsync = Resources.LoadAsync<TextAsset>(szTilesetFilename);

        while (!loadAsync.isDone)
        {
            yield return null;
        }

        TextAsset f = (TextAsset)loadAsync.asset;
        oTileTexture = new Texture2D(2, 2);
        oTileTexture.LoadImage(f.bytes, false);

        bTilesetLoaded = true;
    }
    int iNumTiles;
    Rect[] stTileRects;
    bool LoadTileSetFinalize()
    {
        if (!bTilesetLoaded) return false;

        //analyze tileset (get rectangles)
        int i, j;
        int iNumX, iNumY, iTileNum;
        //iNumX = oTileTexture.width / iTileSize;
        //iNumY = oTileTexture.height / iTileSize;
        iNumX = 448 / iTileSize; //this is now when texture is 512x256,
        iNumY = 256 / iTileSize; //but to get the rects right the original size is used...
        iNumTiles = iNumX * iNumY;
        stTileRects = new Rect[iNumTiles];

        iTileNum = 0;
        for (j = 0; j < iNumY; j++)
        {
            for (i = 0; i < iNumX; i++)
            {
                stTileRects[iTileNum].x = i * iTileSize;
                stTileRects[iTileNum].y = (iNumY - 1 - j) * iTileSize;
                stTileRects[iTileNum].width = iTileSize;
                stTileRects[iTileNum].height = iTileSize;
                iTileNum++;
            }
        }

        return true;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //minimap, made independent of other code, therefore there is some duplicate code here
    static int iMiniWidth, iMiniHeight;
    static string szMiniMapfile;
    static string szMiniMapDescription;
    static bool LoadDesForMiniMap(string i_szFilename, bool bIsCustom, bool bIsCustom2, out bool o_bIsTime)
    {
        o_bIsTime = false;
        szMiniMapDescription = "";
        String szFileText;
        if(bIsCustom2)
        {
            szFileText = GameManager.theGM.oHigh.szLevelTextData;
        }
        else if(bIsCustom)
        {
            szFileText = System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(Application.persistentDataPath + "/" + i_szFilename));
        }
        else
        {
            int iPos = i_szFilename.LastIndexOf('.');
            string szFilenameNoExt = i_szFilename;
            if (iPos > 0) szFilenameNoExt = i_szFilename.Remove(iPos);
            TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
            if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
            if (f == null) return false;
            szFileText = System.Text.Encoding.UTF8.GetString(f.bytes);
        }

        string[] szLines = szFileText.Split((char)10);
        int iLineIndex = -1;

        bool bResult1 = false, bResult2 = false;
        while (iLineIndex < szLines.Length - 1)
        {
            iLineIndex++;
            char[] szSeparator = { (char)32 };
            string[] szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (szTokens.Length == 0) continue;
            if (szTokens[0].Length == 0) continue;
            if (!szTokens[0].StartsWith("*")) continue;

            if (szTokens[0].CompareTo("*MAPTYPE") == 0)
            {
                o_bIsTime = (szTokens[1].CompareTo("RACE") == 0);
            }
            else if (szTokens[0].CompareTo("*MAPSIZE") == 0)
            {
                iMiniWidth = int.Parse(szTokens[1]);
                iMiniHeight = int.Parse(szTokens[2]);
                bResult1 = true;
            }
            else if (szTokens[0].CompareTo("*MAPFILE") == 0)
            {
                szMiniMapfile = szTokens[1];
                bResult2 = true;
            }
            else if (szTokens[0].CompareTo("*MAPDESC") == 0)
            {
                char[] szSeparator2 = { (char)'\"' };
                string[] szWithin = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator2, StringSplitOptions.RemoveEmptyEntries);

                szMiniMapDescription = szWithin[1].Trim(' ');
            }
            if (bResult1 && bResult2) return true;
        }

        return false;
    }

    public static Texture2D GetMiniMap(string i_szFilename, bool bIsCustom, bool bIsCustom2, out bool o_bIsTime, out string o_szDescription)
    {
        int x, y;

        Color stColor = new Color(0.01f, 0.01f, 0.01f, 1.0f);
        Color stColor1 = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        Color stColor2 = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        //load des file
        o_szDescription = "";
        if (!LoadDesForMiniMap(i_szFilename, bIsCustom, bIsCustom2, out o_bIsTime)) return null;
        o_szDescription = szMiniMapDescription;

        //load tile numbers
        int[,] aMap = new int[iMiniHeight, iMiniWidth];
        byte[] bytes;
        if (bIsCustom2)
        {
            bytes = GameManager.theGM.oHigh.aLevelBinaryData;
        }
        else if (bIsCustom)
        {
            bytes = File.ReadAllBytes(Application.persistentDataPath + "/" + szMiniMapfile);
        }
        else
        {
            string szFilenameNoExt = szMiniMapfile.Remove(szMiniMapfile.LastIndexOf('.'));
            TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
            if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
            if (f == null) return null;
            bytes = f.bytes;
        }

        for (y = 0; y < iMiniHeight; y++)
        {
            for (x = 0; x < iMiniWidth; x++)
            {
                aMap[iMiniHeight - 1 - y, x] = bytes[y * iMiniWidth + x];
            }
        }

        //Texture2D oTileTexture = new Texture2D(iMiniWidth, iMiniHeight);
        Texture2D oTileTexture = new Texture2D(96, 96); //make it square with a size that is as large as the biggest level in x and in y

        for (y = 0; y < 96; y++)
        {
            for (x = 0; x < 96; x++)
            {
                oTileTexture.SetPixel(x, y, stColor2);
            }
        }

        for (y = 0; y < iMiniHeight; y++)
        {
            for (x = 0; x < iMiniWidth; x++)
            {
                int iTile = 0;
                iTile = aMap[y, x];
                if (iTile != 0)
                {
                    iTile = 2; //assume. a solid tile on all current tilesets (2=transparent)
                               //if any neighbor is 0, this tile is a border (1)
                    int k, l;
                    for (k = x - 1; k <= x + 1; k++)
                    {
                        for (l = y - 1; l <= y + 1; l++)
                        {
                            if (k < iMiniWidth && l < iMiniHeight && k >= 0 && l >= 0 && aMap[l, k] == 0)
                            {
                                iTile = 1;
                                break;
                            }
                        }
                        if (iTile == 1) break;
                    }
                }

                if (iTile == 0)
                    oTileTexture.SetPixel(x, y, stColor);
                else if (iTile == 1)
                    oTileTexture.SetPixel(x, y, stColor1);
                //else
                //    oTileTexture.SetPixel(x, y, stColor2);
            }
        }

        oTileTexture.wrapMode = TextureWrapMode.Clamp; //prevents botton/left edges from beeing visible on top/right
        oTileTexture.Apply();
        return oTileTexture;
    }

    void OnDrawGizmos()
    {
        /*
        if (map != null)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x ++)
                {
                    Gizmos.color = (map[y,x] == 0)?Color.black: Color.white;
                    Vector3 pos = new Vector3(-width/2 + x + .5f,0, -height/2 + y + .5f);
                    Gizmos.DrawCube(pos,Vector3.one);
                }
            }
        }
        */
    }

}
