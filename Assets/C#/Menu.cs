using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;


public class Menu : MonoBehaviour
{
    public static Menu theMenu = null;
    public static bool bLevelSelected = false;
    public static bool bLevelPlay = false;
    public static bool bYourBestReplay = false;
    public static bool bWorldBestReplay1 = false;
    public static bool bWorldBestReplay2 = false;
    public static bool bWorldBestReplay3 = false;
    public static bool bQuit = false;
    LevelInfo stLevel;
    bool bAllowSelection;

    const int iNumRace = 25;
    const int iNumMission = 30;
    const int NUM_LEVELS = iNumRace + iNumMission;

    public struct S_Levels
    {
        public int iLevelType;
        public string szLevelName;
        public string szLevelDisplayName;
        public string szLevelDescription;
    }
    S_Levels[] aLevels = new S_Levels[NUM_LEVELS];

    C_LevelInMenu[] aMenuLevels;

    GameObject oGazeQuad;
    Material oCursorMaterial, oCursorMaterialWait;

    private void Awake()
    {
        theMenu = this;

        aLevels[0].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[0].szLevelName = "2race00";
        aLevels[0].szLevelDisplayName = "00";
        aLevels[0].szLevelDescription = "Race00 - Race the eight";
        aLevels[1].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[1].szLevelName = "2race01";
        aLevels[1].szLevelDisplayName = "01";
        aLevels[1].szLevelDescription = "Race01 - Loop de loop";
        aLevels[2].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[2].szLevelName = "2race02";
        aLevels[2].szLevelDisplayName = "02";
        aLevels[2].szLevelDescription = "Race02 - Double Infinity";
        aLevels[3].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[3].szLevelName = "2race03";
        aLevels[3].szLevelDisplayName = "03";
        aLevels[3].szLevelDescription = "Race03 - Snake Race";
        aLevels[4].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[4].szLevelName = "2race04";
        aLevels[4].szLevelDisplayName = "04";
        aLevels[4].szLevelDescription = "Race04 - The big S";
        aLevels[5].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[5].szLevelName = "2race05";
        aLevels[5].szLevelDisplayName = "05";
        aLevels[5].szLevelDescription = "Race05 - Optimization oppotunity";
        aLevels[6].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[6].szLevelName = "2race06";
        aLevels[6].szLevelDisplayName = "06";
        aLevels[6].szLevelDescription = "Race06 - C u back there";
        aLevels[7].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[7].szLevelName = "2race07";
        aLevels[7].szLevelDisplayName = "07";
        aLevels[7].szLevelDescription = "Race07 - Back and forth";
        aLevels[8].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[8].szLevelName = "2race08";
        aLevels[8].szLevelDisplayName = "08";
        aLevels[8].szLevelDescription = "Race08 - Platforms";
        aLevels[9].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[9].szLevelName = "2race09";
        aLevels[9].szLevelDisplayName = "09";
        aLevels[9].szLevelDescription = "Race09 - The hand";
        aLevels[10].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[10].szLevelName = "2race10";
        aLevels[10].szLevelDisplayName = "10";
        aLevels[10].szLevelDescription = "Race10 - The face";
        aLevels[11].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[11].szLevelName = "2race11";
        aLevels[11].szLevelDisplayName = "11";
        aLevels[11].szLevelDescription = "Race11 - The baby";
        aLevels[12].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[12].szLevelName = "2race12";
        aLevels[12].szLevelDisplayName = "12";
        aLevels[12].szLevelDescription = "Race12 - Anvil on top";
        aLevels[13].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[13].szLevelName = "2race13";
        aLevels[13].szLevelDisplayName = "13";
        aLevels[13].szLevelDescription = "Race13 - The boot";
        aLevels[14].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[14].szLevelName = "2race14";
        aLevels[14].szLevelDisplayName = "14";
        aLevels[14].szLevelDescription = "Race14 - Introducing doors";
        aLevels[15].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[15].szLevelName = "2race15";
        aLevels[15].szLevelDisplayName = "15";
        aLevels[15].szLevelDescription = "Race15 - Face with doors";
        aLevels[16].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[16].szLevelName = "2race16";
        aLevels[16].szLevelDisplayName = "16";
        aLevels[16].szLevelDescription = "Race16 - Spiral journey";
        aLevels[17].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[17].szLevelName = "2race17";
        aLevels[17].szLevelDisplayName = "17";
        aLevels[17].szLevelDescription = "Race17 - Krypton";
        aLevels[18].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[18].szLevelName = "2race18";
        aLevels[18].szLevelDisplayName = "18";
        aLevels[18].szLevelDescription = "Race18 - No boundaries";
        aLevels[19].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[19].szLevelName = "2race19"; //2race00_zero
        aLevels[19].szLevelDisplayName = "19";
        aLevels[19].szLevelDescription = "Race19 - Zero G";
        aLevels[20].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[20].szLevelName = "2race20"; //2race04_jupiter
        aLevels[20].szLevelDisplayName = "20";
        aLevels[20].szLevelDescription = "Race20 - Jupiter trip";
        aLevels[21].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[21].szLevelName = "2race21"; //2race10_mod
        aLevels[21].szLevelDisplayName = "21";
        aLevels[21].szLevelDescription = "Race21 - G-force and drag mod";
        aLevels[22].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[22].szLevelName = "2race22";
        aLevels[22].szLevelDisplayName = "22";
        aLevels[22].szLevelDescription = "Race22 - Hi lava";
        aLevels[23].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[23].szLevelName = "2race23";
        aLevels[23].szLevelDisplayName = "23";
        aLevels[23].szLevelDescription = "Race23 - Complex cave";
        aLevels[24].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[24].szLevelName = "2race24";
        aLevels[24].szLevelDisplayName = "24";
        aLevels[24].szLevelDescription = "Race24 - Last race";

        aLevels[25].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[25].szLevelName = "1mission00";
        aLevels[25].szLevelDisplayName = "00";
        aLevels[25].szLevelDescription = "Mission00 - Assignment one";
        aLevels[26].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[26].szLevelName = "1mission01";
        aLevels[26].szLevelDisplayName = "01";
        aLevels[26].szLevelDescription = "Mission01 - Enemy one";
        aLevels[27].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[27].szLevelName = "1mission02";
        aLevels[27].szLevelDisplayName = "02";
        aLevels[27].szLevelDescription = "Mission02 - Yin Yang";
        aLevels[28].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[28].szLevelName = "1mission03";
        aLevels[28].szLevelDisplayName = "03";
        aLevels[28].szLevelDescription = "Mission03 - The harp";
        aLevels[29].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[29].szLevelName = "1mission04";
        aLevels[29].szLevelDisplayName = "04";
        aLevels[29].szLevelDescription = "Mission04 - Medusa";
        aLevels[30].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[30].szLevelName = "1mission05";
        aLevels[30].szLevelDisplayName = "05";
        aLevels[30].szLevelDescription = "Mission05 - Forks";
        aLevels[31].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[31].szLevelName = "1mission06";
        aLevels[31].szLevelDisplayName = "06";
        aLevels[31].szLevelDescription = "Mission06 - The iron foot";
        aLevels[32].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[32].szLevelName = "1mission07";
        aLevels[32].szLevelDisplayName = "07";
        aLevels[32].szLevelDescription = "Mission07 - Quantum Pi";
        aLevels[33].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[33].szLevelName = "1mission08";
        aLevels[33].szLevelDisplayName = "08";
        aLevels[33].szLevelDescription = "Mission08 - The whale";
        aLevels[34].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[34].szLevelName = "1mission09";
        aLevels[34].szLevelDisplayName = "09";
        aLevels[34].szLevelDescription = "Mission09 - The genie";
        aLevels[35].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[35].szLevelName = "1mission10";
        aLevels[35].szLevelDisplayName = "10";
        aLevels[35].szLevelDescription = "Mission10 - One shot";
        aLevels[36].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[36].szLevelName = "1mission11";
        aLevels[36].szLevelDisplayName = "11";
        aLevels[36].szLevelDescription = "Mission11 - Lunar gravity";
        aLevels[37].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[37].szLevelName = "1mission12";
        aLevels[37].szLevelDisplayName = "12";
        aLevels[37].szLevelDescription = "Mission12 - Snow cave";
        aLevels[38].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[38].szLevelName = "1mission13";
        aLevels[38].szLevelDisplayName = "13";
        aLevels[38].szLevelDescription = "Mission13 - Eight platforms";
        aLevels[39].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[39].szLevelName = "1mission14";
        aLevels[39].szLevelDisplayName = "14";
        aLevels[39].szLevelDescription = "Mission14 - The mummy";
        aLevels[40].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[40].szLevelName = "1mission15";
        aLevels[40].szLevelDisplayName = "15";
        aLevels[40].szLevelDescription = "Mission15 - The waist";
        aLevels[41].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[41].szLevelName = "1mission16";
        aLevels[41].szLevelDisplayName = "16";
        aLevels[41].szLevelDescription = "Mission16 - The legs";
        aLevels[42].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[42].szLevelName = "1mission17";
        aLevels[42].szLevelDisplayName = "17";
        aLevels[42].szLevelDescription = "Mission17 - Short trips";
        aLevels[43].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[43].szLevelName = "1mission18"; //1mission01_mod
        aLevels[43].szLevelDisplayName = "18";
        aLevels[43].szLevelDescription = "Mission18 - Astroid gravity";
        aLevels[44].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[44].szLevelName = "1mission19"; //1mission04_mod
        aLevels[44].szLevelDisplayName = "19";
        aLevels[44].szLevelDescription = "Mission19 - 04 mod";
        aLevels[45].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[45].szLevelName = "1mission20"; //1mission06_mod
        aLevels[45].szLevelDisplayName = "20";
        aLevels[45].szLevelDescription = "Mission20 - 06 mod";
        aLevels[46].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[46].szLevelName = "1mission21"; //1mission_narrow
        aLevels[46].szLevelDisplayName = "21";
        aLevels[46].szLevelDescription = "Mission21 - Narrow snake";
        aLevels[47].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[47].szLevelName = "1mission22"; //1mission_hard
        aLevels[47].szLevelDisplayName = "22";
        aLevels[47].szLevelDescription = "Mission22 - Hard";
        aLevels[48].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[48].szLevelName = "1mission23";
        aLevels[48].szLevelDisplayName = "23";
        aLevels[48].szLevelDescription = "Mission23 - The snail";
        aLevels[49].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[49].szLevelName = "1mission24";
        aLevels[49].szLevelDisplayName = "24";
        aLevels[49].szLevelDescription = "Mission24 - Goggles";
        aLevels[50].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[50].szLevelName = "1mission25";
        aLevels[50].szLevelDisplayName = "25";
        aLevels[50].szLevelDescription = "Mission25 - Trouble";
        aLevels[51].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[51].szLevelName = "1mission26";
        aLevels[51].szLevelDisplayName = "26";
        aLevels[51].szLevelDescription = "Mission26 - More 06";
        aLevels[52].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[52].szLevelName = "1mission27";
        aLevels[52].szLevelDisplayName = "27";
        aLevels[52].szLevelDescription = "Mission27 - One way";
        aLevels[53].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[53].szLevelName = "1mission28";
        aLevels[53].szLevelDisplayName = "28";
        aLevels[53].szLevelDescription = "Mission28 - Desolation";
        aLevels[54].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[54].szLevelName = "1mission29";
        aLevels[54].szLevelDisplayName = "29";
        aLevels[54].szLevelDescription = "Mission29 - Last mission";
    }

    C_Item2InMenu oMenuQuit;

    Material oMaterialOctagonLocked, oMaterialOctagonUnlocked, oMaterialOctagonHighlighted;
    Material oMaterialPentagonUnlocked, oMaterialPentagonHighlighted;
    void Start()
    {
        aMenuLevels = new C_LevelInMenu[NUM_LEVELS];

        oMaterialOctagonLocked = Resources.Load("LevelOctagonGrey", typeof(Material)) as Material;
        oMaterialOctagonUnlocked = Resources.Load("LevelOctagon", typeof(Material)) as Material;
        oMaterialOctagonHighlighted = Resources.Load("LevelOctagonHigh", typeof(Material)) as Material;
        oMaterialPentagonUnlocked = Resources.Load("LevelPentagon", typeof(Material)) as Material;
        oMaterialPentagonHighlighted = Resources.Load("LevelPentagonHigh", typeof(Material)) as Material;

        //int iLen = NUM_LEVELS / 2;
        float fStartAngle = -45;
        float fAngleRange = 90;
        Vector3 vAroundPoint = new Vector3(0, 0, -90);
        for (int i = 0; i < iNumRace; i++)
        {
            Vector3 vPos = new Vector3(0, (i % 3) * 10.0f - 24.0f, 12.0f);
            float fRotateAngle = fStartAngle + i * (fAngleRange / (iNumRace - 1));
            aMenuLevels[i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, aLevels[i], i);
        }
        int iStartOffs = iNumRace;
        for (int i = 0; i < iNumMission; i++)
        {
            Vector3 vPos = new Vector3(0, (i % 3) * 10.0f + 24.0f, 12.0f);
            float fRotateAngle = fStartAngle + i * (fAngleRange / (iNumMission - 1));
            aMenuLevels[iStartOffs + i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, aLevels[iStartOffs+i], iStartOffs + i);
        }

        //create a quad with a text on, in the pos of each menu object
        oGazeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        oGazeQuad.transform.parent = transform;
        MonoBehaviour.DestroyImmediate(oGazeQuad.GetComponent<Collider>());

        oCursorMaterial = Resources.Load("Cursor", typeof(Material)) as Material;
        oCursorMaterialWait = Resources.Load("Cursor2", typeof(Material)) as Material;
        oGazeQuad.GetComponent<MeshRenderer>().material = oCursorMaterial;

        oGazeQuad.transform.localScale = new Vector3(3.8f, 3.8f, 1);

        //change fov if non VR since that default setting shows to wide fov
        // and is not behaving reliably
        if (!XRDevice.isPresent)
            Camera.main.fieldOfView = 38.0f;

        //prevent selection if trigger was hold when menu is started
        bAllowSelection = !Input.GetButton("Fire1");

        oMenuQuit = new C_Item2InMenu(new Vector3(0, -60, 12.0f), vAroundPoint, 45, "Quit", "Quit", 25.0f, 12.0f);
    }

    int iMissionsUnlocked = 0;
    public void SetMissionUnlock(int i_iMissionsToUnlock)
    {
        if (i_iMissionsToUnlock <= 0) i_iMissionsToUnlock = 1; //first time, or maybe network error
        if (i_iMissionsToUnlock > iNumMission) i_iMissionsToUnlock = iNumMission;
        iMissionsUnlocked = i_iMissionsToUnlock;
    }

    C_ItemInMenu oMenuReplayWR1, oMenuReplayWR2, oMenuReplayWR3;
    C_ItemInMenu oMenuReplayYR, oMenuPlay;
    Texture2D oMiniMapTex;

    public GameObject oTMProBaseObj;
    public GameObject oLevelInfoContainer;
    public GameObject oWRNameText1, oWRNameText2, oWRNameText3;
    public GameObject oWRScoreText1, oWRScoreText2, oWRScoreText3;
    public GameObject oYRScoreText, oLevelText;
    public GameObject oRankQuad;

    string GetTimeString(int i_iTimeMs)
    {
        int iMs = i_iTimeMs % 1000;
        int iTotalSeconds = i_iTimeMs/1000;
        int iSeconds = iTotalSeconds % 60;
        int iMinutes = iTotalSeconds / 60;
        string szTime = iMinutes + ":" + iSeconds.ToString("00") + "." + iMs.ToString("000");

        return szTime;
    }

    public void SetLevelInfo(LevelInfo i_stLevelInfo, bool i_bOff)
    {
        if (i_bOff)
        {
            oLevelInfoContainer.SetActive(false);
            return;
        }
        oLevelInfoContainer.transform.position = oGazeQuad.transform.position + new Vector3(0.0f, 2.0f, -18.0f);
        Vector3 vRotation = oGazeQuad.transform.eulerAngles; vRotation.z = 0;
        oLevelInfoContainer.transform.eulerAngles = vRotation;
        oLevelInfoContainer.transform.localScale = new Vector3(3.0f, 3.0f, 1.0f);
        oLevelInfoContainer.SetActive(true);
        oLevelText.GetComponent<TextMesh>().text = aLevels[GameLevel.iLevelIndex].szLevelDescription;
        oWRNameText1.GetComponent<TextMesh>().text = i_stLevelInfo.szBestName1;
        oWRNameText2.GetComponent<TextMesh>().text = i_stLevelInfo.szBestName2;
        oWRNameText3.GetComponent<TextMesh>().text = i_stLevelInfo.szBestName3;
        string szScore = (i_stLevelInfo.iBestScore1 / 1000.0f).ToString("N3");
        if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iBestScore1);
        if (i_stLevelInfo.iBestScore1 == -1) szScore = "--";
        oWRScoreText1.GetComponent<TextMesh>().text = szScore;
        szScore = (i_stLevelInfo.iBestScore2 / 1000.0f).ToString("N3");
        if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iBestScore2);
        if (i_stLevelInfo.iBestScore2 == -1) szScore = "--";
        oWRScoreText2.GetComponent<TextMesh>().text = szScore;
        szScore = (i_stLevelInfo.iBestScore3 / 1000.0f).ToString("N3");
        if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iBestScore3);
        if (i_stLevelInfo.iBestScore3 == -1) szScore = "--";
        oWRScoreText3.GetComponent<TextMesh>().text = szScore;

        szScore = (i_stLevelInfo.iScoreMs / 1000.0f).ToString("N3");
        if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iScoreMs);
        if (i_stLevelInfo.iScoreMs == -1) szScore = "--";
        oYRScoreText.GetComponent<TextMesh>().text = szScore;

        int iRank = 5; //no score at all
        if(i_stLevelInfo.iScoreMs!=-1)
        {
            iRank = 4; //a score less than bronze
            if (i_stLevelInfo.bIsTime)
            {
                if (i_stLevelInfo.iScoreMs < i_stLevelInfo.iLimit1) iRank = 1; //gold
                else if (i_stLevelInfo.iScoreMs < i_stLevelInfo.iLimit2) iRank = 2; //silver
                else if (i_stLevelInfo.iScoreMs < i_stLevelInfo.iLimit3) iRank = 3; //bronze
            }
            else
            {
                if (i_stLevelInfo.iScoreMs >= i_stLevelInfo.iLimit1) iRank = 1; //gold
                else if (i_stLevelInfo.iScoreMs >= i_stLevelInfo.iLimit2) iRank = 2; //silver
                else if (i_stLevelInfo.iScoreMs >= i_stLevelInfo.iLimit3) iRank = 3; //bronze
            }
        }
        Material oMaterial = null;
        if (iRank == 4 || iRank == 5) oMaterial = Resources.Load("LandingZone", typeof(Material)) as Material;
        if (iRank == 3) oMaterial = Resources.Load("RankBronze", typeof(Material)) as Material;
        if (iRank == 2) oMaterial = Resources.Load("RankSilver", typeof(Material)) as Material;
        if (iRank == 1) oMaterial = Resources.Load("RankGold", typeof(Material)) as Material;
        oRankQuad.GetComponent<MeshRenderer>().material = oMaterial;

        Vector3 vPos = new Vector3(-8.8f, 1.5f, -0.1f);
        if (oMenuReplayWR1 != null) oMenuReplayWR1.DestroyObj();
        if (i_stLevelInfo.iBestScore1 != -1) oMenuReplayWR1 = new C_ItemInMenu(vPos, "1", "ReplayWR1", 4.0f, 4.0f);
        vPos = new Vector3(-8.8f, -1.0f, -0.1f);
        if (oMenuReplayWR2 != null) oMenuReplayWR2.DestroyObj();
        if (i_stLevelInfo.iBestScore2 != -1) oMenuReplayWR2 = new C_ItemInMenu(vPos, "2", "ReplayWR2", 4.0f, 4.0f);
        vPos = new Vector3(-8.8f, -3.5f, -0.1f);
        if (oMenuReplayWR3 != null) oMenuReplayWR3.DestroyObj();
        if (i_stLevelInfo.iBestScore3 != -1) oMenuReplayWR3 = new C_ItemInMenu(vPos, "3", "ReplayWR3", 4.0f, 4.0f);

        vPos = new Vector3(0.5f, 1.5f, -0.1f);
        if (oMenuReplayYR != null) oMenuReplayYR.DestroyObj();
        if(i_stLevelInfo.iScoreMs!=-1) oMenuReplayYR = new C_ItemInMenu(vPos, "", "ReplayYR", 4.0f, 4.0f);
        vPos = new Vector3(1.5f, -2.6f, -0.1f); //vPos = new Vector3(0.5f, -2.5f, -0.1f);
        if (oMenuPlay != null) oMenuPlay.DestroyObj();
        oMenuPlay = new C_ItemInMenu(vPos, "PLAY", "Play", 8.5f, 3.4f);

        //i_stLevelInfo.szName is in the form "race00", but we need the filename "2race00"
        //we rely on GameLevel.szLevel for that
        oMiniMapTex = GameLevel.GetMiniMap(GameLevel.szLevel);
        oMaterial = Resources.Load("MiniMap", typeof(Material)) as Material;
        oMaterial.mainTexture = oMiniMapTex;
    }

    float fRotateZAngle = 0.0f;
    void Update()
    {
        Material oMatTemp;
        if(aMenuLevels[0]==null)
        {
            Debug.LogError("Update - NULL");
        }

        //do a raycast into the world based on the user's
        // head position and orientation
        Vector3 vHeadPosition = Camera.main.transform.position;
        Vector3 vGazeDirection = Camera.main.transform.forward;

        //reset highlighting
        if (oMenuReplayWR1 != null && oMenuReplayWR1.oLevelQuad != null) oMenuReplayWR1.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayWR2 != null && oMenuReplayWR2.oLevelQuad != null) oMenuReplayWR2.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayWR3 != null && oMenuReplayWR3.oLevelQuad != null) oMenuReplayWR3.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayYR != null && oMenuReplayYR.oLevelQuad != null) oMenuReplayYR.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuPlay != null && oMenuPlay.oLevelQuad != null) oMenuPlay.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;

        bool bHitLevel = false;
        RaycastHit oHitInfo;
        if (Physics.Raycast(vHeadPosition, vGazeDirection, out oHitInfo, 400.0f))
        {
            //a hit, place cursor on object

            //move the cursor to the point where the raycast hit
            oGazeQuad.transform.position = oHitInfo.point;
            //rotate the cursor to hug the surface
            oGazeQuad.transform.rotation =
                Quaternion.FromToRotation(Vector3.back, oHitInfo.normal);

            //find which object we hit

            //manage highlighting of viewed object
            if (oHitInfo.collider.name.StartsWith("Coll"))
            {
                char[] szName = oHitInfo.collider.name.ToCharArray();
                string szId = new string(szName, 4, szName.Length - 4);
                int iIndex = int.Parse(szId);

                for (int i = 0; i < iNumRace + iNumMission; i++)
                {
                    oMatTemp = oMaterialOctagonLocked;
                    if (i == iIndex)
                    {
                        if (i < iNumRace) oMatTemp = oMaterialPentagonHighlighted;
                        else if (i - iNumRace < iMissionsUnlocked) oMatTemp = oMaterialOctagonHighlighted;
                        //else oMatTemp = oMaterialOctagonLocked;
                    }
                    else
                    {
                        if (i < iNumRace) oMatTemp = oMaterialPentagonUnlocked;
                        else if (i - iNumRace < iMissionsUnlocked) oMatTemp = oMaterialOctagonUnlocked;
                        //else oMatTemp = oMaterialOctagonLocked;
                    }
                    aMenuLevels[i].oLevelQuad.GetComponent<MeshRenderer>().material = oMatTemp;
                }
                bHitLevel = true;
            }
            else if (oHitInfo.collider.name.CompareTo("Play") == 0)
            {
                oMenuPlay.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayYR") == 0)
            {
                oMenuReplayYR.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR1") == 0)
            {
                oMenuReplayWR1.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR2") == 0)
            {
                oMenuReplayWR2.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR3") == 0)
            {
                oMenuReplayWR3.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonHighlighted;
            }            

            //manage selection
            if (Input.GetButton("Fire1"))
            {
                if (bAllowSelection)
                {
                    bool bPlaySelectSound = false;
                    if (oHitInfo.collider.name.StartsWith("Coll"))
                    {
                        char[] szName = oHitInfo.collider.name.ToCharArray();
                        string szId = new string(szName, 4, szName.Length - 4);
                        int iIndex = int.Parse(szId);
                        string szLevel = aLevels[iIndex].szLevelName;

                        if(iIndex < iNumRace+iMissionsUnlocked)
                        {
                            GameLevel.iLevelIndex = iIndex;
                            GameLevel.szLevel = szLevel;
                            bLevelSelected = true;
                            bAllowSelection = false; //trigger once only...
                            bPlaySelectSound = true;
                        }
                    }
                    else if (oHitInfo.collider.name.CompareTo("Play") == 0)
                    {
                        bLevelPlay = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayYR") == 0)
                    {
                        bYourBestReplay = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR1") == 0)
                    {
                        bWorldBestReplay1 = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR2") == 0)
                    {
                        bWorldBestReplay2 = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR3") == 0)
                    {
                        bWorldBestReplay3 = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Quit") == 0)
                    {
                        bQuit = true;
                        bAllowSelection = false;
                        bPlaySelectSound = true;
                    }

                    if(bPlaySelectSound) GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
                }
            }
            else
            {
                bAllowSelection = true;
            }
        }
        else
        {
            //no hit, place cursor at max distance

            //set at max distance
            oGazeQuad.transform.position = vHeadPosition+ vGazeDirection*170.0f;
            //rotate the cursor to camera rotation
            oGazeQuad.transform.rotation = Camera.main.transform.rotation;

            bLevelSelected = false;
        }

        //nothing highlighted?
        if(!bHitLevel)
        {
            for (int i = 0; i < iNumRace + iNumMission; i++)
            {
                if (i < iNumRace) oMatTemp = oMaterialPentagonUnlocked;
                else if (i - iNumRace < iMissionsUnlocked) oMatTemp = oMaterialOctagonUnlocked;
                else oMatTemp = oMaterialOctagonLocked;
                aMenuLevels[i].oLevelQuad.GetComponent<MeshRenderer>().material = oMatTemp;
            }
        }

        //rotate again around Z, not visible on standard cursor, but visible on the waiting cursor
        fRotateZAngle += 130 * Time.deltaTime;
        if (fRotateZAngle > 360) fRotateZAngle -= 360; //keep it 0..360
        oGazeQuad.transform.Rotate(Vector3.back, fRotateZAngle);
    }

    public void SetWaiting(bool i_bWaiting)
    {
        if(i_bWaiting) oGazeQuad.GetComponent<MeshRenderer>().material = oCursorMaterialWait;
        else oGazeQuad.GetComponent<MeshRenderer>().material = oCursorMaterial;
    }

    public class C_LevelInMenu
    {
        public GameObject oLevelQuad;
        GameObject oLevelText;

        Vector3 vPos;

        public C_LevelInMenu(Vector3 i_vPos, Vector3 i_vAroundPoint, float i_fRotateAngle, S_Levels i_oLevel, int i_iLevelId)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = "Coll"+i_iLevelId.ToString();
            oLevelQuad.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(10.0f, 10.0f, 1.0f);
            oLevelQuad.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.value*100.0f); //vary 100 deg around z
            oLevelQuad.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);

            string szMaterial = (i_oLevel.iLevelType == (int)LevelType.MAP_RACE) ? "LevelPentagon" : "LevelOctagon";
            Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
            oLevelQuad.GetComponent<MeshRenderer>().material = oMaterial;

            oLevelText = Instantiate(Menu.theMenu.oTMProBaseObj, Menu.theMenu.transform);
            oLevelText.transform.localPosition = new Vector3(vPos.x-3.9f, vPos.y-3.50f, vPos.z - 1.1f);
            oLevelText.transform.localScale = new Vector3(1.85f, 1.85f, 1.0f);
            oLevelText.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);
            oLevelText.GetComponent<TextMeshPro>().text = i_oLevel.szLevelDisplayName;
            oLevelText.SetActive(true);
        }
    }

    public class C_ItemInMenu
    {
        public GameObject oLevelQuad;
        GameObject oLevelText;

        Vector3 vPos;

        public void DestroyObj()
        {
            Destroy(oLevelQuad);
            Destroy(oLevelText);
        }

        public C_ItemInMenu(Vector3 i_vPos, string szText, string szCollID, float i_fScale, float i_fScaleText)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.oLevelInfoContainer.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = szCollID;
            oLevelQuad.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(i_fScale * 0.4f, i_fScale * 0.4f, 1.0f);
            oLevelQuad.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already

            string szMaterial = "LevelOctagon";
            Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
            oLevelQuad.GetComponent<MeshRenderer>().material = oMaterial;

            //create text
            oLevelText = new GameObject();
            oLevelText.transform.parent = Menu.theMenu.oLevelInfoContainer.transform;
            oLevelText.name = "TextMesh";
            oLevelText.AddComponent<TextMesh>();
            oLevelText.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z - 0.1f);
            oLevelText.transform.localScale = new Vector3(i_fScaleText * 0.08f, i_fScaleText * 0.08f, 1.0f);
            oLevelText.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already
            
            oLevelText.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
            oLevelText.GetComponent<TextMesh>().fontSize = 40;
            oLevelText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            oLevelText.GetComponent<TextMesh>().text = szText;
        }
    }

    public class C_Item2InMenu
    {
        public GameObject oLevelQuad;
        GameObject oLevelText;

        Vector3 vPos;

        public void DestroyObj()
        {
            Destroy(oLevelQuad);
            Destroy(oLevelText);
        }

        public C_Item2InMenu(Vector3 i_vPos, Vector3 i_vAroundPoint, float i_fRotateAngle, string szText, string szCollID, float i_fScale, float i_fScaleText)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = szCollID;
            oLevelQuad.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(i_fScale * 0.4f, i_fScale * 0.4f, 1.0f);
            oLevelQuad.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);

            string szMaterial = "LevelCircle";
            Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
            oLevelQuad.GetComponent<MeshRenderer>().material = oMaterial;

            //create text
            oLevelText = new GameObject();
            oLevelText.transform.parent = Menu.theMenu.transform;
            oLevelText.name = "TextMesh";
            oLevelText.AddComponent<TextMesh>();
            oLevelText.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z - 0.1f);
            oLevelText.transform.localScale = new Vector3(i_fScaleText * 0.08f, i_fScaleText * 0.08f, 1.0f);
            oLevelText.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);

            oLevelText.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
            oLevelText.GetComponent<TextMesh>().fontSize = 40;
            oLevelText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            oLevelText.GetComponent<TextMesh>().text = szText;
        }
    }

}
