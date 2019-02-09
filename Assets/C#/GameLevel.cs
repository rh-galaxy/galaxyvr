using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.XR;

public enum LevelType { MAP_MISSION, MAP_RACE, MAP_DOGFIGHT, MAP_MISSION_COOP }; //MAP_DOGFIGHT, MAP_MISSION_COOP not supported for now

//TODO move LandingZone code to a new file when we have all the 3D objects
// also make LandingZone : MonoBehaviour

public class C_LandingZone //: MonoBehaviour
{
    public int iId;
    public Vector2 vPos;

    int iWidth, iHeight;
    int iZoneSize; //width in tiles
    
    //other
    internal bool bHomeBase;
    internal bool bExtraLife;
    bool bShowAntenna, bShowHouse;
    List<int> aCargoList; //array of cargo weights (small = 1..5,6..10,11..15,16..20+ = huge)

    GameObject oZone;
    GameObject[] oZoneCargoList;

    public C_LandingZone(int i_iId, Vector2 i_vPos, int i_iWidth, float i_fDepth, bool i_bHomeBase,
        bool i_bShowAntenna, bool i_bShowHouse, List<int> i_aCargoList, bool i_bExtraLife)
    {
        Material oMaterial;

        vPos = i_vPos;
        iZoneSize = i_iWidth;
        iId = i_iId;

        bHomeBase = i_bHomeBase;
        bShowAntenna = i_bShowAntenna;
        bShowHouse = i_bShowHouse;
        aCargoList = i_aCargoList;
        bExtraLife = i_bExtraLife;

        oZoneCargoList = new GameObject[aCargoList.Count];

        int iAdjustX = (iZoneSize * 32) / 2;
        int iBaseX = bShowHouse ? -50 + iAdjustX : -78 + iAdjustX;
        //float[] aBoxSizeX = { 13.0f, 11.0f, 9.0f, 7.0f };
        float[] aBoxSizeX = { 26.0f, 22.0f, 18.0f, 14.0f };

        for (int i = 0; i < aCargoList.Count; i++)
        {
            int iCargoType = 3 - (aCargoList[i] - 1) / 5; //1..5=small container, 6..10, 11..15, 16..20+ = large container
            if (iCargoType < 0) iCargoType = 0;
            //if(iCargoType>3) iCargoType=3;

            GameObject oBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oBox.transform.parent = GameLevel.theMap.transform;
            MonoBehaviour.DestroyImmediate(oBox.GetComponent<BoxCollider>());

            oBox.transform.position = new Vector3(vPos.x + ((iAdjustX - ((i / 3) * 28)) -13) / 32.0f, vPos.y + (6 + ((i % 3) * 7)) / 32.0f, /**/0.6f);
            oBox.transform.localScale = new Vector3(aBoxSizeX[iCargoType] / 32.0f, 6.0f / 32.0f, 0.5f);

            oMaterial = Resources.Load("Pickups", typeof(Material)) as Material;
            oBox.GetComponent<MeshRenderer>().material = oMaterial;

            oZoneCargoList[i] = oBox;
        }

        oZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        oZone.transform.parent = GameLevel.theMap.transform;
        MonoBehaviour.DestroyImmediate(oZone.GetComponent<BoxCollider>());
        oZone.AddComponent<BoxCollider2D>();
        oZone.name = "LandingZone" + iId.ToString();

        oZone.transform.position = new Vector3(vPos.x, vPos.y, 0);
        oZone.transform.localScale = new Vector3(iZoneSize * 1.0f, 4.0f / 32.0f, i_fDepth);

        oMaterial = Resources.Load("LandingZone", typeof(Material)) as Material;
        oZone.GetComponent<MeshRenderer>().material = oMaterial;
    }

    int GetZoneSize()
    {
        return iZoneSize;
    }

    public int GetTotalCargo()
    {
        int iCargo = 0;
        if (aCargoList != null)
        {
            for (int i = 0; i < aCargoList.Count; i++)
            {
                iCargo += aCargoList[i];
            }
        }
        return iCargo;
    }

    public void TakeExtraLife()
    {
        bExtraLife = false;
    }

    public int PopCargo(bool bPeekOnly)
    {
        int iCargo = -1;
        int iPos = aCargoList.Count - 1;
        if (iPos >= 0)
        {
            iCargo = aCargoList[iPos];
            if (!bPeekOnly)
            {
                oZoneCargoList[iPos].SetActive(false);
                aCargoList.RemoveAt(iPos);
            }
        }

        return iCargo;
    }

    public void PushCargo(int i_iWeight)
    {
        aCargoList.Add(i_iWeight);
        if (aCargoList.Count <= oZoneCargoList.Length)
            oZoneCargoList[aCargoList.Count - 1].SetActive(true);
        //else
        //    i_iWeight = i_iWeight; //should not happen
    }

    void Draw()
    {
        /*dAntennaFrame += i_dTime * 0.0046; //4.6 fps anim
        if (dAntennaFrame >= NUM_ANTENNARECTS) m_dAntennaFrame = 0.0; //is m_dAntennaFrame-='length' really, but then it can happen that it'll be more than 'length' when too much time have passed since last time (frame)...*/
        //VR: have a rotating beacon on the top instead?

        /*int i, x, y;

        if (bShowHouse) ODraw(m_pclImage, &s_stHouseRect, x - 70 + iAdjustX, y + 21);
        if (bShowAntenna) ODraw(m_pclImage, &s_stAntennaRects[(int)m_dAntennaFrame], x, y + 29);
        if (bExtraLife)
        {
            int iBaseX = m_bShowAntenna ? -8 : -4;
            ODraw(m_pclImage, &s_stExtraLifeRect, x + iBaseX, y + s_stExtraLifeRect.height);
        }*/
    }
}

struct S_TilesetInfo
{
    public S_TilesetInfo(string i_szMaterial, bool i_bRedBricks, string i_szMaterialWalls)
    {
        szMaterial = i_szMaterial;
        bRedBricks = i_bRedBricks;

        szMateralWalls = i_szMaterialWalls;
    }
    public string szMaterial;
    public bool bRedBricks;
    public string szMateralWalls;
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

public class GameLevel : MonoBehaviour
{
    //should realy try to get rid of the statics and make the GameManager hold the object instead
    // for now this means there can be only one GameLevel...
    public static GameLevel theMap = null;
    public static string szLevel;
    public static bool bMapLoaded = false;
    public static Replay theReplay = null;
    public static bool bRunReplay = false;

    //achievements, level complete
    bool bRunGameOverTimer = false;
    float fGameOverTimer = 0.0f;
    internal bool bGameOver = false;
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
        new S_TilesetInfo("Cave_Alien", true, "Walls_Alien"),
        new S_TilesetInfo("Cave_Evil", true, "Walls_Grey"),
        new S_TilesetInfo("Cave_Cave", true, "Walls_Grey"),
        new S_TilesetInfo("Cave_Cryptonite", true, "Walls_Cryptonite"),
        new S_TilesetInfo("Cave_Frost", true, "Walls_Frost"),
        new S_TilesetInfo("Cave_Lava", false, "Walls_Lava") };

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
    List<C_LandingZone> aLandingZoneList;

    public CheckPoint oCheckPointObjBase;
    internal List<CheckPoint> aCheckPointList;

    public Enemy oEnemyObjBase;
    internal List<Enemy> aEnemyList;

    public GameObject oBulletObjBase;

    //static noncollidable objects
    List<GameObject> aDecorationList; //single bricks, brick wall left,center,right
    public ZObject oZObjBase;


    MeshGenerator oMeshGen;
    Material oMaterialWalls; //set when walls are created, and used also in creating the map border

    GameLevel()
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

        //make back plane and border
        backPlane.transform.localPosition = new Vector3(0, 0, 6.0f);
        backPlane.transform.localScale = new Vector3(iWidth / 10.0f, 1.0f, iHeight / 10.0f);
        backPlane.GetComponent<MeshRenderer>().material = oMaterialWalls; //from when mesh was created
        Vector3 vSize = GetMapSize();
        GameObject oObj;
        //left
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(-vSize.x / 2 - 0.5f, 0, 2.0f);
        oObj.transform.localScale = new Vector3(1.0f, vSize.y, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialWalls;
        //right
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(vSize.x / 2 + 0.5f, 0, 2.0f);
        oObj.transform.localScale = new Vector3(1.0f, vSize.y, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialWalls;
        //top
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, vSize.y / 2 + 0.5f, 2.0f);
        oObj.transform.localScale = new Vector3(vSize.x + 2.0f, 1.0f, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialWalls;
        //bottom
        oObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MonoBehaviour.DestroyImmediate(oObj.GetComponent<BoxCollider>());
        oObj.transform.parent = transform;
        oObj.transform.position = new Vector3(0, -vSize.y / 2 - 0.5f, 2.0f);
        oObj.transform.localScale = new Vector3(vSize.x + 2.0f, 1.0f, vSize.z + 6);
        oObj.GetComponent<MeshRenderer>().material = oMaterialWalls;

        //change fov if non VR since that default setting shows to wide fov
        // and is not behaving reliably
        if (!XRDevice.isPresent)
            Camera.main.fieldOfView = 38.0f;

        Debug.Log("Start done");
    }

    public bool LoadInSegments(int n)
    {
        if (n == 0)
        {
            Debug.Log("Loading Level: " + szLevel);
            LoadDesPass1(szLevel);
            oMeshGen = GetComponent<MeshGenerator>();

            //load and generate map
            string szPngTileset = szTilefile.Remove(szTilefile.LastIndexOf('.')) + ".png";
            LoadTileSet(szPngTileset);

            return false;
        }
        else
        {
            bool bFinished = LoadMap(n - 1);
            if (bFinished) bMapLoaded = true;
            return bFinished;
        }

    }

    void Update()
    {
        //finalize the loading in the first few frames after Start()
        if (iFinalizeCounter <= 4)
        {
            oMeshGen.GenerateMeshFinalize(iFinalizeCounter);
            iFinalizeCounter++;
        }
        else if (iFinalizeCounter == 5)
        {
            for (int y = 0; y < iHeight; y++)
            {
                for (int x = 0; x < iWidth; x++)
                {
                    ReplaceAndAddObjectPass2(x, y);
                }
            }
            iFinalizeCounter++;
        }
        //end of init code

        if (!bMapLoaded) return;

        //race finished
        if(player.bAchieveFinishedRaceLevel)  {
            bRunGameOverTimer = true;
        }
        //mission finished
        if(GetMissionFinished())
        {
            bRunGameOverTimer = true;
            bAchieveFinishedMissionLevel = true;
        }
        if (iLevelType == (int)LevelType.MAP_MISSION && player.iNumLifes==0) bRunGameOverTimer = true;

        if (bRunGameOverTimer) {
            fGameOverTimer += Time.deltaTime;
            if (fGameOverTimer > 5.0f) bGameOver = true;
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

    public C_LandingZone GetLandingZone(int i_iId)
    {
        for (int i = 0; i < aLandingZoneList.Count; i++)
            if (aLandingZoneList[i].iId == i_iId) return aLandingZoneList[i];
        return null;
    }

    public Vector3 GetMapSize()
    {
        return new Vector3(iWidth * 1.0f, iHeight * 1.0f, fWallHeight);
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

        aLandingZoneList = new List<C_LandingZone>();
        aCheckPointList = new List<CheckPoint>();
        aEnemyList = new List<Enemy>();
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
                bool bAntenna = szTokens[6].CompareTo("ANTENNA") == 0;
                bool bHouse = szTokens[7].CompareTo("WAREHOUSE") == 0;
                if (szTokens.Length > 8) iNumCargo = int.Parse(szTokens[8]);

                for (int i = 0; i < iNumCargo; i++)
                {
                    aCargoList.Add(int.Parse(szTokens[9 + i]));
                }

                Vector2 vPos = AdjustPosition(new Vector2(x, y), new Vector2(32 * w, 4));
                C_LandingZone oZone = new C_LandingZone(iZoneCounter++, vPos, w, fWallHeight,
                    bHomeBase, bAntenna, bHouse, aCargoList, bExtraLife);
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
                        case 0: break;
                        case 1: break;
                        case 2: break;
                        case 3: break;
                        case 4:
                            {
                                stEnemy.vWayPoints[i] = AdjustPosition(stEnemy.vWayPoints[i], new Vector2(64, 32)); break;
                            }
                        case 5: break;
                        case 6: break;
                    }
                }

                Enemy oEnemy = Instantiate(oEnemyObjBase, this.transform);
                oEnemy.Init(stEnemy, this);
                aEnemyList.Add(oEnemy);
            }
            else if (szTokens[0].CompareTo("*DOOR") == 0)
            {
                /*S_DoorInfo stParams;
                int x, y, iAngle;
                sscanf(szLine, "%s %d %d %d", szCommand, &x, &y, &stParams.iSize);

                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*ANGLE"
                sscanf(szLine, "%s %d", szCommand, &iAngle);
                stParams.bHorizontal = ((iAngle / 90) % 2) != 0;
                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*OPENTIME"
                sscanf(szLine, "%s %d", szCommand, &stParams.iOpenedForTime);
                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*CLOSETIME"
                sscanf(szLine, "%s %d", szCommand, &stParams.iClosedForTime);
                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*OPENSPEED"
                sscanf(szLine, "%s %d", szCommand, &stParams.iOpeningSpeed);
                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*CLOSESPEED"
                sscanf(szLine, "%s %d", szCommand, &stParams.iClosingSpeed);

                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*NUMBUTTONS"
                sscanf(szLine, "%s %d", szCommand, &stParams.iNumButtons);

                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*BUTTONSX"
                sscanf(szLine, "%s %d %d %d", szCommand, &stParams.stButtonPos[0].x, &stParams.stButtonPos[1].x, &stParams.stButtonPos[2].x);
                C_Global::GetNextLineAndCommand(szFile + iLineIndex, szLine, szCommand, &iLineIndex); //command "*BUTTONSY"
                sscanf(szLine, "%s %d %d %d", szCommand, &stParams.stButtonPos[0].y, &stParams.stButtonPos[1].y, &stParams.stButtonPos[2].y);

                //add door to list
                C_Door* pclDoor = new C_Door(this, x, y, &stParams, DOOR_BASEID + m_iNumDoors, io_pclExtra->bMaster, io_pclExtra->pclNetMsg);
                io_pclExtra->pclDoorList->push_back(pclDoor);
                m_iNumDoors++; //network id
                */
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
            case 62: aMap[y, x] = 17; break;
            //tree2
            case 63: aMap[y, x] = 24; break;
            //barrels
            //case 64: aMap[y, x] = 17; break;

            //radio tower
            case 76: aMap[y, x] = 0; break;
            case 77: aMap[y, x] = 17; break;
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

            //generate new high res map based on the textures of the tiles
            int pixelsamplepos = 32 / substeps;
            aMapHighres = new int[iHeight * substeps, iWidth * substeps];
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
                            int posx = (int)r.position.x + x2 * pixelsamplepos + pixelsamplepos / 2;
                            int posy = (int)r.position.y + y2 * pixelsamplepos + pixelsamplepos / 2;
                            //sample every pixelsamplepos pixel in the tile texture
                            aMapHighres[y * substeps + y2, x * substeps + x2] = oTileTexture.GetPixel(posx, posy) == Color.black ? 0 : iTile;
                            //aMapHighres[y * substeps + y2, x * substeps + x2] = aMap[y, x];
                        }
                    }
                }
            }
        }
        else if (n == 1)
        {
        }
        else if (n == 2)
        {
            //generate final mesh, set tile material
            Material oMaterial = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMaterial, typeof(Material)) as Material;
            oMaterialWalls = Resources.Load(m_stTilesetInfos[iTilesetInfoIndex].szMateralWalls, typeof(Material)) as Material;
            oMeshGen.GenerateMeshInit(aMapHighres, 1.0f/substeps, fWallHeight, fBumpHeight, oMaterial, oMaterialWalls);
        }
        else if (n >= 3)
        {
            return oMeshGen.GenerateMesh(n-3);
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
