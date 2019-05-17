using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.XR;

public enum LevelType { MAP_MISSION, MAP_RACE, MAP_DOGFIGHT, MAP_MISSION_COOP }; //MAP_DOGFIGHT, MAP_MISSION_COOP not supported

struct S_TilesetInfo
{
    public S_TilesetInfo(string i_szMaterial, bool i_bRedBricks, string i_szMaterialWalls,
        string i_szMaterialBox, int i_iPlanet, int i_iTree)
    {
        szMaterial = i_szMaterial;
        bRedBricks = i_bRedBricks;
        szMateralWalls = i_szMaterialWalls;
        szMateralBox = i_szMaterialBox;
        iPlanet = i_iPlanet;
        iTree = i_iTree;
    }
    public string szMaterial;
    public bool bRedBricks;
    public string szMateralWalls;
    public string szMateralBox;
    public int iPlanet;
    public int iTree;
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
    //should realy try to get rid of the statics and make the GameManager hold the object instead
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

    public GameObject backPlane;
    public Player player;

    public float fWallHeight = 3.0f;
    public float fBumpHeight = 0.25f;

    static string szLevelPath0 = "Levels/";
    static string szLevelPath1 = "";

    string[] szLines;

    int[,] aMapHighres;
    int[,] aMap;

    int iTilesetInfoIndex = 0;
    S_TilesetInfo[] m_stTilesetInfos = {
        new S_TilesetInfo("Cave_Alien", true, "Walls_Alien", "Walls_Alien", 5, 1),
        ///**/new S_TilesetInfo("Cave_Alien", true, "Walls_Crack_Test", "Walls_Alien", 5, 1),
        new S_TilesetInfo("Cave_Evil", true, "Walls_Grey", "Walls_Grey", 3, 1),
        new S_TilesetInfo("Cave_Cave", true, "Walls_Grey", "Walls_Grey", 5, 1),
        new S_TilesetInfo("Cave_Cryptonite", true, "Walls_Cryptonite", "Walls_Cryptonite", 2, 3),
        new S_TilesetInfo("Cave_Frost", true, "Walls_Frost", "Walls_Frost", 4, 3),
        new S_TilesetInfo("Cave_Lava", false, "Walls_Lava", "Walls_Lava", 1, 2),
        new S_TilesetInfo("Cave_Desert", false, "Walls_Desert", "Walls_Desert", 5, 11) };

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

    const int DEFAULT_SHIPGRAVITYBASEX = 0;      //pixel/second 2
    const int DEFAULT_SHIPGRAVITYBASEY = 70;     //pixel/second 2
    const float DEFAULT_SHIPRESISTANCE = 0.68f;  //constant (velocity dependent)
    internal const int BULLETBASEVEL = 220;      //pixel/second
    internal const float BULLETFREETIME = 3.1f;  //sec to be free from bullets when just come alive

    //map objects
    public LandingZone oLandingZoneObjBase;
    List<LandingZone> aLandingZoneList;

    public CheckPoint oCheckPointObjBase;
    internal List<CheckPoint> aCheckPointList;

    public Enemy oEnemyObjBase;
    internal List<Enemy> aEnemyList;
    public int GetNumEnemiesNearPlayer(/*Vector2 vPos, float fRadius*/)
    {
        int iNumEnemies = 0;
        for (int i=0; i< aEnemyList.Count; i++)
        {
            if (aEnemyList[i] != null)
            {
                Vector2 vDist = new Vector2(aEnemyList[i].vPos.x, aEnemyList[i].vPos.y) - this.player.GetPosition();
                if (vDist.magnitude < (450 / 32.0f)) iNumEnemies++; //~14 tiles, same as fire range
            }
        }
        return iNumEnemies;
    }

    public GameObject oBulletObjBase;

    public Door oDoorObjBase;
    internal List<Door> aDoorList;

    //static noncollidable objects
    List<GameObject> aDecorationList; //single bricks, brick wall left,center,right
    public ZObject oZObjBase;
    public Decoration oDecorationObjBase; //barrels, trees

    public Material oSkyBoxMat1;
    public Material oSkyBoxMat2;
    public Material oSkyBoxMat3;
    public Material oSkyBoxMat4;
    public Material oSkyBoxMat5;
    public Planet oPlanet;

    public AudioClip oClipLevelStart;
    public AudioClip oClipLevelGameOver;
    public AudioClip oClipLevelWin;

    MeshGenerator oMeshGen;
    Material oMaterialWalls; //set when walls are created, and used also in creating the map border

    public GameLevel()
    {
        theMap = this;
    }

    int iFinalizeCounter;
    void Start()
    {
        bGameOver = false;

        LoadDesPass2();
        iFinalizeCounter = 0;

        //(user not currently used in Player)
        string szUser = GameManager.szUser == null ? "Incognito" : GameManager.szUser;
        player.Init(szUser, 0, stPlayerStartPos[0], this);

        Material oMaterialBox = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMateralBox, typeof(Material)) as Material;
        //make back plane and border
        //        /**/oMeshGen.GenerateMeshBackground(oMaterialBox);
        /*backPlane.transform.localPosition = new Vector3(0, 0, 6.0f);
        backPlane.transform.localScale = new Vector3(iWidth / 10.0f, 1.0f, iHeight / 10.0f);
        backPlane.GetComponent<MeshRenderer>().material = oMaterialBox;*/
        /**///backPlane.GetComponent<MeshRenderer>().material = oMaterialBox;
        oMeshGen.map0_bk.GetComponent<MeshRenderer>().material = oMaterialBox;
        Vector3 vSize = GetMapSize();
        GameObject oObj;
        //left
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(-vSize.x / 2 - 0.5f, 0, 2.0f);
        oObj.transform.localScale = new Vector3(1.0f, vSize.y, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //right
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(vSize.x / 2 + 0.5f, 0, 2.0f);
        oObj.transform.localScale = new Vector3(1.0f, vSize.y, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //top
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, vSize.y / 2 + 0.5f, 2.0f);
        oObj.transform.localScale = new Vector3(vSize.x + 2.0f, 1.0f, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;
        //bottom
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, -vSize.y / 2 - 0.5f, 2.0f);
        oObj.transform.localScale = new Vector3(vSize.x + 2.0f, 1.0f, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialBox;

        //change fov if non VR since the default setting shows to wide fov
        // and is not behaving reliably
        if (!XRDevice.isPresent)
            Camera.main.fieldOfView = 40.0f;

        //set random skybox
        int iSkyBox = UnityEngine.Random.Range(1, 5);
        switch(iSkyBox)
        {
            case 1: RenderSettings.skybox = oSkyBoxMat1; break;
            case 2: RenderSettings.skybox = oSkyBoxMat2; break;
            case 3: RenderSettings.skybox = oSkyBoxMat3; break;
            case 4: RenderSettings.skybox = oSkyBoxMat4; break;
            case 5: RenderSettings.skybox = oSkyBoxMat5; break;
        }

        Debug.Log("Start done");
    }

    public bool LoadInSegments(int n)
    {
        if (n == 0)
        {
            Debug.Log("Loading Level: " + szLevel);
            LoadDesPass1(szLevel);
            oMeshGen = GetComponent<MeshGenerator>();

            //set back plain first of all
            /**/oMeshGen.GenerateMeshBackground(iWidth, iHeight);

            //load and generate map
            string szPngTileset = szTilefile.Remove(szTilefile.LastIndexOf('.')) + ".png";
            LoadTileSet(szPngTileset);
            return false;
        }
        else
        {
///**/float t1 = Time.realtimeSinceStartup;
            bool bFinished = LoadMap(n - 1);
///**/Debug.Log("LoadMap: " + (Time.realtimeSinceStartup - t1)*1000.0f);
            if (bFinished) bMapLoaded = true;
            return bFinished;
        }

    }

    void Update()
    {
        //finalize the loading in the first few frames after Start()
        if (iFinalizeCounter <= 16)
        {
///**/float t1 = Time.realtimeSinceStartup;
            oMeshGen.GenerateMeshFinalize(iFinalizeCounter);
///**/Debug.Log("GenerateMeshFinalize: " + (Time.realtimeSinceStartup - t1) * 1000.0f);
            iFinalizeCounter++;

            //pause physics
            Time.timeScale = 0.0f;
        }
        else if (iFinalizeCounter == 17)
        {
            for (int y = 0; y < iHeight; y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    ReplaceAndAddObjectPass2(x, y);
                }
            }
            iFinalizeCounter++;

            oPlanet.Init(m_stTilesetInfos[iTilesetInfoIndex].iPlanet);
            GetComponent<AudioSource>().PlayOneShot(oClipLevelStart);

            //time back to normal
            Time.timeScale = 1.0f;
        }
        //end of init code

        if (!bMapLoaded) return;

        if(!bRunGameOverTimer)
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

            //delay sounds 1.5 sec
            if(bPlayClipWin && fGameOverTimer > 1.5f)
            {
                GetComponent<AudioSource>().PlayOneShot(oClipLevelWin);
                bPlayClipWin = false;
            }
            if (bPlayClipGameOver && fGameOverTimer > 1.5f)
            {
                GetComponent<AudioSource>().PlayOneShot(oClipLevelGameOver);
                bPlayClipGameOver = false;
            }

            if (fGameOverTimer > 6.0f) bGameOver = true;
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
        (iHeight * 32 - i_vPixelPos.y - i_vPixelSize.y * 0.5f) / 32.0f - iHeight * 0.5f);
    }
    //old pixels pos (pos from tile position) to new pos
    Vector2 AdjustPositionNoFlip(Vector2 i_vPixelPos, Vector2 i_vPixelSize)
    {
        return new Vector2(i_vPixelPos.x / 32.0f - iWidth * 0.5f + (i_vPixelSize.x / 32.0f) * 0.5f,
        (i_vPixelPos.y - i_vPixelSize.y * 0.5f) / 32.0f - iHeight * 0.5f);
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

    //gets only the info needed to generate the map mesh
    bool LoadDesPass1(string i_szFilename)
    {
        //free old map if exists
        iLevelType = (int)LevelType.MAP_MISSION;

        int iPos = i_szFilename.LastIndexOf('.');
        string szFilenameNoExt = i_szFilename;
        if (iPos > 0) szFilenameNoExt = i_szFilename.Remove(iPos);
        TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
        if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
        if (f == null) return false;

        String szFileText = System.Text.Encoding.UTF8.GetString(f.bytes);
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
    bool LoadDesPass2()
    {
        //free old map if exists
        iLevelType = (int)LevelType.MAP_MISSION;
        iRaceLaps = 0;
        iMinPlayers = 1;
        iMaxPlayers = 1;
        iNumDoors = 0;

        //des file is in szLines after LoadDesPass1()

        vGravity = new Vector2(DEFAULT_SHIPGRAVITYBASEX, -DEFAULT_SHIPGRAVITYBASEY);
        fDrag = DEFAULT_SHIPRESISTANCE;

        aLandingZoneList = new List<LandingZone>();
        aCheckPointList = new List<CheckPoint>();
        aEnemyList = new List<Enemy>();
        aDoorList = new List<Door>();
        aDecorationList = new List<GameObject>();

        int iLineIndex = -1;
        int iStartPosCounter = 0;
        int iZoneCounter = 0;

        //to parse 0.00 as float on any system
        CultureInfo ci = new CultureInfo("en-US");

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
                vGravity.x = float.Parse(szTokens[1], ci.NumberFormat);
                vGravity.y = -float.Parse(szTokens[2], ci.NumberFormat);
            }
            else if (szTokens[0].CompareTo("*RESISTANCE") == 0)
            {
                fDrag = float.Parse(szTokens[1], ci.NumberFormat);
                //m_vDrag.y = float.Parse(szTokens[2]); //no support in physics engine
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
                                stEnemy.vWayPoints[i].x -= 4.5f/32.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(12, 16));
                                stEnemy.vWayPoints[i].y -= 4.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(16, 12));
                                stEnemy.vWayPoints[i].x += 9.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(12, 16));
                                stEnemy.vWayPoints[i].y += 7.0f / 32.0f;
                                stEnemy.vWayPoints[i].x += 3.0f / 32.0f;
                            }
                            break;
                        case 1:
                        case 3:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x -= 4.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y -= 2.5f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x += 6.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y += 4.0f / 32.0f;
                            }
                            break;
                        case 4:
                            stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(64, 32));
                            break;
                        case 5:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x -= 5.5f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y -= 5.5f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 24));
                                stEnemy.vWayPoints[i].x += 5.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(24, 14));
                                stEnemy.vWayPoints[i].y += 6.0f / 32.0f;
                            }
                            break;
                        case 6:
                            if (stEnemy.iAngle == 0)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].x -= 6.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 270)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].y -= 6.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 180)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].x += 6.0f / 32.0f;
                            }
                            else if (stEnemy.iAngle == 90)
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(14, 14));
                                stEnemy.vWayPoints[i].y += 2.0f / 32.0f;
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
            }
            else if (szTokens[0].CompareTo("*DOOR") == 0)
            {
                S_DoorInfo stParams = new S_DoorInfo();

                stParams.vPos.x = int.Parse(szTokens[1]);
                stParams.vPos.y = int.Parse(szTokens[2]);
                stParams.fLength = (int.Parse(szTokens[3]))/32.0f;

                //command "*ANGLE"
                iLineIndex++;
                szTokens = szLines[iLineIndex].Trim('\r', '\n').Split(szSeparator, StringSplitOptions.RemoveEmptyEntries);
                stParams.bHorizontal = int.Parse(szTokens[1])!=90 && int.Parse(szTokens[1]) != 270;

                if(!stParams.bHorizontal) stParams.vPos = AdjustPosition(stParams.vPos, new Vector2(52, 36));
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
                
            }
            //header loaded
        }
        return true;
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

                    oObj.transform.position = new Vector3(vPos.x, vPos.y, -1.5f - fBumpHeight);
                    oObj.transform.localScale = new Vector3(16.0f / 32.0f, 8.0f / 32.0f, 1.0f);
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
                    oObj.transform.position = new Vector3(vPos.x, vPos.y, -1.5f - 0.08f);
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
            case 77:
                {
                    Vector2 vPos = AdjustPositionNoFlip(new Vector2(x * 32.0f - 0.0f, y * 32.0f + (32.0f + 12.0f)), new Vector2(32.0f, 64.0f));
                    Decoration oDObj = Instantiate(oDecorationObjBase, this.transform);
                    oDObj.Init(10, vPos);
                    break;
                }
        }
    }

    void LoadRow(int y, int substeps, float pixelsamplepos)
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
                    aMapHighres[y * substeps + y2, x * substeps + x2] = oTileTexture.GetPixel(posx, posy) == Color.black ? 0 : iTile;
                    //aMapHighres[y * substeps + y2, x * substeps + x2] = iTile;
                }
            }
        }
    }

    bool LoadMap(int n)
    {
        int substeps = 6;

        if (n == 0)
        {
            aMap = new int[iHeight, iWidth];

            //load tile numbers
            string szFilenameNoExt = szMapfile.Remove(szMapfile.LastIndexOf('.'));
            TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
            if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
            if (f == null) return false;
            for (int y = 0; y < iHeight; y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    aMap[iHeight - 1 - y, x] = f.bytes[y * iWidth + x];
                }
            }

            aMapHighres = new int[iHeight * substeps, iWidth * substeps];
        }
        else if (n < 1 + iHeight)
        {
            //generate new high res map based on the textures of the tiles
            float pixelsamplepos = 32.0f / substeps;

            //for (int y = 0; y < iHeight; y++)
            //{
                LoadRow(n-1, substeps, pixelsamplepos);
            //}
        }
        else if (n == 1 + iHeight)
        {
            //generate final mesh, set tile material
            Material oMaterial = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMaterial, typeof(Material)) as Material;
            oMaterialWalls = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMateralWalls, typeof(Material)) as Material;
            oMeshGen.GenerateMeshInit1(aMapHighres, 1.0f/substeps, fWallHeight, fBumpHeight, oMaterial, oMaterialWalls);
        }
        else if (n <= 3 + iHeight)
        {
            oMeshGen.GenerateMeshInit2(n - (2 + iHeight));
        }
        else if (n >= 4 + iHeight)
        {
            return oMeshGen.GenerateMesh(n - (4 + iHeight));
        }

        return false;
    }

    int iNumTiles;
    Rect[] stTileRects;
    Texture2D oTileTexture;

    bool LoadTileSet(string i_szFilename)
    {
        int i, j;

        string szFilenameNoExt = i_szFilename.Remove(i_szFilename.LastIndexOf('.'));
        TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
        if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
        if (f == null) return false;

        oTileTexture = new Texture2D(2, 2);
        if(!oTileTexture.LoadImage(f.bytes, false))
            return false;

        //analyse tileset (get rectangles)
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

    //minimap, made independant of other code, therefore there is some duplicate code here
    static int iMiniWidth, iMiniHeight;
    static string szMiniMapfile;
    static bool LoadDesForMiniMap(string i_szFilename)
    {
        int iPos = i_szFilename.LastIndexOf('.');
        string szFilenameNoExt = i_szFilename;
        if (iPos > 0) szFilenameNoExt = i_szFilename.Remove(iPos);
        TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
        if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
        if (f == null) return false;

        String szFileText = System.Text.Encoding.UTF8.GetString(f.bytes);
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

            if (szTokens[0].CompareTo("*MAPSIZE") == 0)
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
            if (bResult1 && bResult2) return true;
        }

        return false;
    }

    public static Texture2D GetMiniMap(string i_szFilename)
    {
        int x, y;

        Color stColor = new Color(0.01f, 0.01f, 0.01f, 1.0f);
        Color stColor1 = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        Color stColor2 = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        //load des file
        if (!LoadDesForMiniMap(i_szFilename)) return null;

        //load tile numbers
        int[,] aMap = new int[iMiniHeight, iMiniWidth];
        string szFilenameNoExt = szMiniMapfile.Remove(szMiniMapfile.LastIndexOf('.'));
        TextAsset f = (TextAsset)Resources.Load(szLevelPath0 + szFilenameNoExt);
        if (f == null) f = (TextAsset)Resources.Load(szLevelPath1 + szFilenameNoExt);
        if (f == null) return null;
        for (y = 0; y < iMiniHeight; y++)
        {
            for (x = 0; x < iMiniWidth; x++)
            {
                aMap[iMiniHeight - 1 - y, x] = f.bytes[y * iMiniWidth + x];
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
                    iTile = 2; //assume. a solid tile on all current tilesets (2=transparrent)
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
