using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;


public class Menu : MonoBehaviour
{
    public static Menu theMenu = null;
    public static bool bLevelSelected = false;
    public static bool bLevelUnSelected = false;
    public static bool bLevelPlay = false;
    public static bool bYourBestReplay = false;
    public static bool bWorldBestReplay1 = false;
    public static bool bWorldBestReplay2 = false;
    public static bool bWorldBestReplay3 = false;
    public static bool bQuit = false;
    public static bool bPauseInput = false;
    LevelInfo stLevel;
    int iAllowSelection;

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

    C_LevelInMenu[] aMenuLevels; //official 55 levels
    //C_LevelInMenu[] aMenuLevels2; //additional official levels
    C_LevelInMenu[] aMenuCustomLevels;

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
        aLevels[22].szLevelDescription = "Race22 - Hi desert";
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
        aLevels[36].szLevelDescription = "Mission11 - Stapler";
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
        aLevels[44].szLevelDescription = "Mission19 - Medusa 2";
        aLevels[45].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[45].szLevelName = "1mission20"; //1mission06_mod
        aLevels[45].szLevelDisplayName = "20";
        aLevels[45].szLevelDescription = "Mission20 - The dragon";
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
        aLevels[51].szLevelDescription = "Mission26 - Kangaroo";
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

    public Material oSkyBoxMat1;
    public Material oSkyBoxMat2;
    public Material oSkyBoxMat3;
    public Material oSkyBoxMat4;
    public Material oSkyBoxMat5;

    public CameraController oCameraHolder;

    C_Item2InMenu oMenuQuit, oMenuCredits, oMenuControls;
    GameObject oCreditsQuad;
    C_Item2InMenu oMenuQuality1, oMenuQuality2, oMenuQuality3;
    int iQuality = 2;
    C_Item2InMenu oMenuSnapMovement;
    C_Item2InMenu oMenuNext1, oMenuPrev1;//, oMenuNext2, oMenuPrev2;

    Material oMaterialGrey;
    Material oMiniMapMaterial;
    MeshRenderer oRankQuadRenderer;

    internal Material oMaterialOctagonLocked, oMaterialOctagonUnlocked, oMaterialOctagonHighlighted;
    internal Material oMaterialPentagonLocked, oMaterialPentagonUnlocked, oMaterialPentagonHighlighted;
    internal Material oMaterialOctagonPlay, oMaterialOctagonPlayHighlighted;
    internal Material oMaterialCircle, oMaterialCircleHighlighted;
    internal Material oMaterialRankBronze, oMaterialRankSilver, oMaterialRankGold;
    void Start()
    {
        //done incrementally in Update
    }

    int iMissionUnlocked = 0;
    int iRaceUnlocked = 0;
    public void SetLevelUnlock(int i_iMissionToUnlock, int i_iRaceToUnlock)
    {
        if (i_iMissionToUnlock <= 0) i_iMissionToUnlock = 1; //first time, or maybe network error
        if (i_iMissionToUnlock > iNumMission) i_iMissionToUnlock = iNumMission;
        iMissionUnlocked = i_iMissionToUnlock;
        if (i_iRaceToUnlock <= 0) i_iRaceToUnlock = 1; //first time, or maybe network error
        if (i_iRaceToUnlock > iNumRace) i_iRaceToUnlock = iNumRace;
        iRaceUnlocked = i_iRaceToUnlock;
    }

    C_ItemInMenu oMenuReplayWR1, oMenuReplayWR2, oMenuReplayWR3;
    C_ItemInMenu oMenuReplayYR, oMenuPlay;
    Texture2D oMiniMapTex;

    public GameObject oTMProBaseObj, oTMProBaseObj1, oTMProBaseObj2;
    public GameObject oLevelInfoContainer;
    public GameObject oWRNameText1, oWRNameText2, oWRNameText3;
    private TextMesh oWRNameText1TextMesh, oWRNameText2TextMesh, oWRNameText3TextMesh;
    public GameObject oWRScoreText1, oWRScoreText2, oWRScoreText3;
    private TextMesh oWRScoreText1TextMesh, oWRScoreText2TextMesh, oWRScoreText3TextMesh;
    public GameObject oYLScoreText, oYRScoreText, oLevelText;
    private TextMesh oYLScoreTextTextMesh, oYRScoreTextTextMesh, oLevelTextTextMesh;
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

    public void SetLevelInfoOff()
    {
        oLevelInfoContainer.SetActive(false);
    }

    string szLevelInfoDescription;
    public void SetLevelInfoPass1(LevelInfo i_stLevelInfo)
    {
        //float t1 = Time.realtimeSinceStartup;

        //i_stLevelInfo.szName is in the form "race00", but we need the filename "2race00"
        //we rely on GameLevel.szLevel for that
        oMiniMapTex = GameLevel.GetMiniMap(GameLevel.szLevel, GameLevel.iLevelIndex >= 200, out i_stLevelInfo.bIsTime, out szLevelInfoDescription);
        oMiniMapMaterial.mainTexture = oMiniMapTex;

        //Debug.Log("SetLevelInfoPass1: " + (Time.realtimeSinceStartup - t1) * 1000.0f);
    }

    Vector3 vHeadPosition;
    Vector3 vGazeDirection;
    Vector3 vRotation;
    //float t1 = 0;
    public bool SetLevelInfoPass2(LevelInfo i_stLevelInfo, int n)
    {
        //Debug.Log("SetLevelInfoPass2: " + (Time.realtimeSinceStartup - t1) * 1000.0f +" n "+ n.ToString());
        //t1 = Time.realtimeSinceStartup;

        if (n == 0)
        {
            vHeadPosition = Camera.main.transform.position;
            vGazeDirection = Camera.main.transform.forward;
            oLevelInfoContainer.transform.position = (vHeadPosition + vGazeDirection * 5.40f) + new Vector3(0.0f, -.40f, 0.0f);
            vRotation = Camera.main.transform.eulerAngles; vRotation.z = 0;
            oLevelInfoContainer.transform.eulerAngles = vRotation;
            oLevelInfoContainer.transform.localScale = new Vector3(3.0f, 3.0f, 1.0f);

            if (GameLevel.iLevelIndex >= 200) oLevelTextTextMesh.text = szLevelInfoDescription; //custom levels have description from .des file
            else oLevelTextTextMesh.text = aLevels[GameLevel.iLevelIndex].szLevelDescription; //non custom levels have description from hard array above
            if (i_stLevelInfo.szWRName1.Length <= 17) oWRNameText1TextMesh.text = i_stLevelInfo.szWRName1;
            else oWRNameText1TextMesh.text = i_stLevelInfo.szWRName1.Substring(0, 16) + "...";
            if (i_stLevelInfo.szWRName2.Length <= 17) oWRNameText2TextMesh.text = i_stLevelInfo.szWRName2;
            else oWRNameText2TextMesh.text = i_stLevelInfo.szWRName2.Substring(0, 16) + "...";
            if (i_stLevelInfo.szWRName3.Length <= 17) oWRNameText3TextMesh.text = i_stLevelInfo.szWRName3;
            else oWRNameText3TextMesh.text = i_stLevelInfo.szWRName3.Substring(0, 16) + "...";
            string szScore = (i_stLevelInfo.iWRScore1 / 1000.0f).ToString("N3");
            if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iWRScore1);
            if (i_stLevelInfo.iWRScore1 == -1) szScore = "--";
            oWRScoreText1TextMesh.text = szScore;
            szScore = (i_stLevelInfo.iWRScore2 / 1000.0f).ToString("N3");
            if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iWRScore2);
            if (i_stLevelInfo.iWRScore2 == -1) szScore = "--";
            oWRScoreText2TextMesh.text = szScore;

            szScore = (i_stLevelInfo.iWRScore3 / 1000.0f).ToString("N3");
            if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iWRScore3);
            if (i_stLevelInfo.iWRScore3 == -1) szScore = "--";
            oWRScoreText3TextMesh.text = szScore;

            szScore = (i_stLevelInfo.iLastScoreMs / 1000.0f).ToString("N3");
            if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iLastScoreMs);
            if (i_stLevelInfo.iLastScoreMs == -1) szScore = "--";
            oYLScoreTextTextMesh.text = szScore;

            szScore = (i_stLevelInfo.iBestScoreMs / 1000.0f).ToString("N3");
            if (i_stLevelInfo.bIsTime) szScore = GetTimeString(i_stLevelInfo.iBestScoreMs);
            if (i_stLevelInfo.iBestScoreMs == -1) szScore = "--";
            oYRScoreTextTextMesh.text = szScore;
        }

        if (n == 1)
        {
            int iRank = 5; //no score at all
            if (i_stLevelInfo.iBestScoreMs != -1)
            {
                iRank = 4; //a score less than bronze
                if (i_stLevelInfo.bIsTime)
                {
                    if (i_stLevelInfo.iBestScoreMs < i_stLevelInfo.iLimit1) iRank = 1; //gold
                    else if (i_stLevelInfo.iBestScoreMs < i_stLevelInfo.iLimit2) iRank = 2; //silver
                    else if (i_stLevelInfo.iBestScoreMs < i_stLevelInfo.iLimit3) iRank = 3; //bronze
                }
                else
                {
                    if (i_stLevelInfo.iBestScoreMs >= i_stLevelInfo.iLimit1) iRank = 1; //gold
                    else if (i_stLevelInfo.iBestScoreMs >= i_stLevelInfo.iLimit2) iRank = 2; //silver
                    else if (i_stLevelInfo.iBestScoreMs >= i_stLevelInfo.iLimit3) iRank = 3; //bronze
                }
            }
            Material oMaterial = oMaterialGrey;
            //if (iRank == 4 || iRank == 5) oMaterial = oMaterialGrey;
            if (iRank == 3) oMaterial = oMaterialRankBronze;
            if (iRank == 2) oMaterial = oMaterialRankSilver;
            if (iRank == 1) oMaterial = oMaterialRankGold;
            oRankQuadRenderer.material = oMaterial;
        }

        if (n == 2)
        {
            oLevelInfoContainer.SetActive(true);

            Vector3 vPos = new Vector3(-8.9f, 1.5f, -0.1f);
            if (oMenuReplayWR1 != null) oMenuReplayWR1.Reinit(vPos, "1", "ReplayWR1", 4.0f, 4.0f);
            else oMenuReplayWR1 = new C_ItemInMenu(vPos, "1", "ReplayWR1", 4.0f, 4.0f);
            oMenuReplayWR1.oLevelQuad.SetActive(i_stLevelInfo.iWRScore1 != -1);
            oMenuReplayWR1.oLevelText.SetActive(i_stLevelInfo.iWRScore1 != -1);

            vPos = new Vector3(-8.9f, -1.0f, -0.1f);
            if (oMenuReplayWR2 != null) oMenuReplayWR2.Reinit(vPos, "2", "ReplayWR2", 4.0f, 4.0f);
            else oMenuReplayWR2 = new C_ItemInMenu(vPos, "2", "ReplayWR2", 4.0f, 4.0f);
            oMenuReplayWR2.oLevelQuad.SetActive(i_stLevelInfo.iWRScore2 != -1);
            oMenuReplayWR2.oLevelText.SetActive(i_stLevelInfo.iWRScore2 != -1);

            vPos = new Vector3(-8.9f, -3.5f, -0.1f);
            if (oMenuReplayWR3 != null) oMenuReplayWR3.Reinit(vPos, "3", "ReplayWR3", 4.0f, 4.0f);
            else oMenuReplayWR3 = new C_ItemInMenu(vPos, "3", "ReplayWR3", 4.0f, 4.0f);
            oMenuReplayWR3.oLevelQuad.SetActive(i_stLevelInfo.iWRScore3 != -1);
            oMenuReplayWR3.oLevelText.SetActive(i_stLevelInfo.iWRScore3 != -1);

            vPos = new Vector3(0.5f, 1.5f, -0.1f);
            if (oMenuReplayYR != null) oMenuReplayYR.Reinit(vPos, "", "ReplayYR", 4.0f, 4.0f);
            else oMenuReplayYR = new C_ItemInMenu(vPos, "", "ReplayYR", 4.0f, 4.0f);
            oMenuReplayYR.oLevelQuad.SetActive(i_stLevelInfo.iBestScoreMs != -1);
            oMenuReplayYR.oLevelText.SetActive(i_stLevelInfo.iBestScoreMs != -1);

            vPos = new Vector3(0.5f, -2.5f, -0.1f);
            if (oMenuPlay != null) oMenuPlay.Reinit(vPos, "", "Play", 4.0f, 4.0f);
            else oMenuPlay = new C_ItemInMenu(vPos, "", "Play", 4.0f, 4.0f);

            oWRNameText1.SetActive(true); oWRNameText2.SetActive(true); oWRNameText3.SetActive(true);
            oWRScoreText1.SetActive(true); oWRScoreText2.SetActive(true); oWRScoreText3.SetActive(true);
            oYLScoreText.SetActive(true); oYRScoreText.SetActive(true); oLevelText.SetActive(true);
            oRankQuad.SetActive(true);
            //return true;
        }

        if (n == 3)
        {
            return true;
        }

        //Debug.Log("SetLevelInfoPass2: " + (Time.realtimeSinceStartup - t1) * 1000.0f);
        return false;
    }

    public void InitLevelRanking()
    {
        for (int i = 0; i < aMenuLevels.Length; i++)
            aMenuLevels[i].InitLevelRanking(i);
    }

    float fRotateZAngle = 0.0f;
    int iIncrementalInit = 0;
    void Update()
    {
        //from start
        if (iIncrementalInit == 0)
        {
            aMenuLevels = new C_LevelInMenu[NUM_LEVELS];

            //set random skybox
            int iSkyBox = UnityEngine.Random.Range(1, 5);
            switch (iSkyBox)
            {
                case 1: RenderSettings.skybox = oSkyBoxMat1; break;
                case 2: RenderSettings.skybox = oSkyBoxMat2; break;
                case 3: RenderSettings.skybox = oSkyBoxMat3; break;
                case 4: RenderSettings.skybox = oSkyBoxMat4; break;
                case 5: RenderSettings.skybox = oSkyBoxMat5; break;
            }
        }
        if (iIncrementalInit == 2)
        {

            oMaterialOctagonLocked = Resources.Load("LevelOctagonGrey", typeof(Material)) as Material;
            oMaterialOctagonUnlocked = Resources.Load("LevelOctagon", typeof(Material)) as Material;
            oMaterialOctagonHighlighted = Resources.Load("LevelOctagonHigh", typeof(Material)) as Material;
            oMaterialPentagonLocked = Resources.Load("LevelPentagonGrey", typeof(Material)) as Material;
            oMaterialPentagonUnlocked = Resources.Load("LevelPentagon", typeof(Material)) as Material;
            oMaterialPentagonHighlighted = Resources.Load("LevelPentagonHigh", typeof(Material)) as Material;
            oMaterialOctagonPlay = Resources.Load("LevelOctagonPlay", typeof(Material)) as Material;
            oMaterialOctagonPlayHighlighted = Resources.Load("LevelOctagonPlayHigh", typeof(Material)) as Material;
            oMaterialCircle = Resources.Load("LevelCircle", typeof(Material)) as Material;
            oMaterialCircleHighlighted = Resources.Load("LevelCircleHigh", typeof(Material)) as Material;
        }
        if (iIncrementalInit == 3)
        {
            oWRScoreText1TextMesh = oWRScoreText1.GetComponent<TextMesh>();
            oWRScoreText2TextMesh = oWRScoreText2.GetComponent<TextMesh>();
            oWRScoreText3TextMesh = oWRScoreText3.GetComponent<TextMesh>();
            oWRNameText1TextMesh = oWRNameText1.GetComponent<TextMesh>();
            oWRNameText2TextMesh = oWRNameText2.GetComponent<TextMesh>();
            oWRNameText3TextMesh = oWRNameText3.GetComponent<TextMesh>();
            oYLScoreTextTextMesh = oYLScoreText.GetComponent<TextMesh>();
            oYRScoreTextTextMesh = oYRScoreText.GetComponent<TextMesh>();
            oLevelTextTextMesh = oLevelText.GetComponent<TextMesh>();
        }
        if (iIncrementalInit == 4)
        {
            oMaterialRankBronze = Resources.Load("RankBronze", typeof(Material)) as Material;
            oMaterialRankSilver = Resources.Load("RankSilver", typeof(Material)) as Material;
            oMaterialRankGold = Resources.Load("RankGold", typeof(Material)) as Material;

            oMaterialGrey = Resources.Load("LandingZone", typeof(Material)) as Material;
            oMiniMapMaterial = Resources.Load("MiniMap", typeof(Material)) as Material;
            oRankQuadRenderer = oRankQuad.GetComponent<MeshRenderer>();
        }
        if (iIncrementalInit == 5)
        {
            //create a quad a circle for gazeing in VR or mouse cursor in non VR
            oGazeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oGazeQuad.transform.parent = transform;
            MonoBehaviour.DestroyImmediate(oGazeQuad.GetComponent<Collider>());
            oCursorMaterial = Resources.Load("Cursor", typeof(Material)) as Material;
            oCursorMaterialWait = Resources.Load("Cursor2", typeof(Material)) as Material;
            oGazeQuad.GetComponent<MeshRenderer>().material = oCursorMaterial;
            oGazeQuad.transform.localScale = new Vector3(3.8f, 3.8f, 1);
        }
        if (iIncrementalInit == 6)
        {
            //original 55 levels
            float fStartAngle = -45;
            float fAngleRange = 90;
            Vector3 vPos, vAroundPoint = new Vector3(0, 0, -9.0f);
            for (int i = 0; i < iNumRace; i++)
            {
                vPos = new Vector3(0, (i % 3) * 1.00f - 2.40f, 1.20f);
                float fRotateAngle = fStartAngle + i * (fAngleRange / (iNumRace - 1));
                aMenuLevels[i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, aLevels[i], i);
            }
            int iStartOffs = iNumRace;
            for (int i = 0; i < iNumMission; i++)
            {
                vPos = new Vector3(0, (i % 3) * 1.00f + 2.40f, 1.20f);
                float fRotateAngle = fStartAngle + i * (fAngleRange / (iNumMission - 1));
                aMenuLevels[iStartOffs + i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, aLevels[iStartOffs + i], iStartOffs + i);
            }
        }
        if (iIncrementalInit == 7)
        {
            Vector3 vAroundPoint = new Vector3(0, 0, -9.0f);

            //menu options
            oMenuQuit = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 31, "Quit", "Quit", 30.0f, 12.0f);
            oMenuControls = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 38, "Controls", "Controls", 30.0f, 9.0f);
            oMenuCredits = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 45, "Credits", "Credits", 30.0f, 9.0f);

            CameraController.bSnapMovement = PlayerPrefs.GetInt("MyUseSnapMovement", 0) != 0;
            oMenuSnapMovement = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 53.5f, "Snap", "Snap", 30.0f, 9.0f);

            iQuality = PlayerPrefs.GetInt("MyUnityGraphicsQuality", 2);
            QualitySettings.SetQualityLevel(iQuality, true);
            oMenuQuality1 = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 62, "Med", "Qual1", 30.0f, 9.0f);
            oMenuQuality2 = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 69, "High", "Qual2", 30.0f, 9.0f);
            oMenuQuality3 = new C_Item2InMenu(new Vector3(0, /**/-6.0f, 1.20f), vAroundPoint, 76, "Ultra", "Qual3", 30.0f, 9.0f);
        }
        if (iIncrementalInit == 8)
        {
            //next/prev buttons
            Vector3 vPos = new Vector3(0.0f, /**/1.0f, -0.1f);
            if (oMenuNext1 != null) oMenuNext1.DestroyObj();
            oMenuNext1 = new C_Item2InMenu(vPos, new Vector3(0, 0, -9.0f), 50, "", "Next1", 18.0f, 9.0f);
            vPos = new Vector3(1000.0f, /**/1.0f, -0.1f);
            if (oMenuPrev1 != null) oMenuPrev1.DestroyObj();
            oMenuPrev1 = new C_Item2InMenu(vPos, new Vector3(1000, 0, -9.0f), -50, "<", "Prev1", 18.0f, 9.0f);

            //additional official levels with hiscore
            //...
        }
        if (iIncrementalInit == 9)
        {
            //custom levels
            float fStartAngle = -45;
            string s = Application.persistentDataPath;
            DirectoryInfo info = new DirectoryInfo(s);
            FileInfo[] fileInfo = info.GetFiles("*.des");
            aMenuCustomLevels = new C_LevelInMenu[fileInfo.Length];
            Vector3 vAroundPoint = new Vector3(1000, 0, -9.0f);
            int iNum = (fileInfo.Length > 9 * 5 ? 9 * 5 : fileInfo.Length); //limit to 45, if its a problem fix later
            for (int i = 0; i < iNum; i++)
            {
                Vector3 vPos = new Vector3(1000, (i % 9) * 1.05f - 4.70f, 1.20f);
                float fRotateAngle = fStartAngle + (i / 9) * 23.0f;
                S_Levels level = new S_Levels();
                level.iLevelType = (int)LevelType.MAP_MISSION;
                level.szLevelDescription = ""; //set when level info is set
                level.szLevelName = fileInfo[i].Name;
                int iPos = fileInfo[i].Name.LastIndexOf('.');
                level.szLevelDisplayName = fileInfo[i].Name;
                if (iPos > 0) level.szLevelDisplayName = fileInfo[i].Name.Remove(iPos);
                aMenuCustomLevels[i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, level, 200 + i);
            }

            //level text
            GameObject oCustomPathText;
            oCustomPathText = Instantiate(Menu.theMenu.oTMProBaseObj2, Menu.theMenu.transform);
            oCustomPathText.transform.position = new Vector3(1000.0f, fileInfo.Length < 22 ? 0.0f : 50.0f, 1.20f);
            oCustomPathText.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            //oCustomPathText.transform.RotateAround(vAroundPoint, Vector3.up, 0.0f);
            oCustomPathText.GetComponent<TextMeshPro>().text = "Custom levels (" + s + "), download editor from www.galaxy-forces-vr.com";
            oCustomPathText.SetActive(true);
        }
        if (iIncrementalInit == 10)
        {
            //change fov if non VR since that default setting shows to wide fov
            // and is not behaving reliably
            if (!XRDevice.isPresent)
                Camera.main.fieldOfView = 45.0f;
            //prevent selection if trigger was hold when menu is started
            iAllowSelection = !(Input.GetButton("Button0") || Input.GetButton("Button1") || Input.GetMouseButton(0)) ? 0 : 30;
        }
        iIncrementalInit++;
        if(iIncrementalInit<11) return;

        Material oMatTemp;
        if(aMenuLevels[0]==null)
        {
            Debug.LogError("Update - NULL");
        }

        //exit if input shall be ignored
        if (Menu.bPauseInput) return;

        //get input from joysticks
        float fAxisX = Input.GetAxisRaw("Horizontal");
        float fAdjust = 0;
        if (fAxisX > 0.4f) fAdjust = 1000;
        if (fAxisX < -0.4f) fAdjust = -1000;

        //do a raycast into the world based on the user's
        // head position and orientation
        Vector3 vHeadPosition = Camera.main.transform.position;
        Vector3 vGazeDirection = Camera.main.transform.forward;

        //reset highlighting
        if (oMenuReplayWR1 != null && oMenuReplayWR1.oLevelQuad != null) oMenuReplayWR1.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayWR2 != null && oMenuReplayWR2.oLevelQuad != null) oMenuReplayWR2.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayWR3 != null && oMenuReplayWR3.oLevelQuad != null) oMenuReplayWR3.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuReplayYR != null && oMenuReplayYR.oLevelQuad != null) oMenuReplayYR.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonUnlocked;
        if (oMenuPlay != null && oMenuPlay.oLevelQuad != null) oMenuPlay.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonPlay;

        if (oMenuNext1 != null && oMenuNext1.oLevelQuad != null) oMenuNext1.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonPlay;
        if (oMenuPrev1 != null && oMenuPrev1.oLevelQuad != null) oMenuPrev1.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialOctagonPlay;

        if (oMenuQuit != null && oMenuQuit.oLevelQuad != null) oMenuQuit.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialCircle;
        if (oMenuControls != null && oMenuControls.oLevelQuad != null) oMenuControls.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialCircle;
        if (oMenuCredits != null && oMenuCredits.oLevelQuad != null) oMenuCredits.oLevelQuad.GetComponent<MeshRenderer>().material = oMaterialCircle;
        if (oMenuQuality1 != null && oMenuQuality1.oLevelQuad != null) oMenuQuality1.oLevelQuad.GetComponent<MeshRenderer>().material = (iQuality == 1) ? oMaterialCircleHighlighted : oMaterialCircle;
        if (oMenuQuality2 != null && oMenuQuality2.oLevelQuad != null) oMenuQuality2.oLevelQuad.GetComponent<MeshRenderer>().material = (iQuality == 2) ? oMaterialCircleHighlighted : oMaterialCircle;
        if (oMenuQuality3 != null && oMenuQuality3.oLevelQuad != null) oMenuQuality3.oLevelQuad.GetComponent<MeshRenderer>().material = (iQuality == 4) ? oMaterialCircleHighlighted : oMaterialCircle;
        if (oMenuSnapMovement != null && oMenuSnapMovement.oLevelQuad != null) oMenuSnapMovement.oLevelQuad.GetComponent<MeshRenderer>().material = CameraController.bSnapMovement ? oMaterialCircleHighlighted : oMaterialCircle;

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
                        if (i < iNumRace)
                        {
                            oMatTemp = oMaterialPentagonHighlighted;
                            if (i >= iRaceUnlocked) oMatTemp = oMaterialPentagonLocked;
                        }
                        else if (i - iNumRace < iMissionUnlocked) oMatTemp = oMaterialOctagonHighlighted;
                        //else oMatTemp = oMaterialOctagonLocked;
                    }
                    else
                    {
                        if (i < iNumRace)
                        {
                            oMatTemp = oMaterialPentagonUnlocked;
                            if (i >= iRaceUnlocked) oMatTemp = oMaterialPentagonLocked;
                        }
                        else if (i - iNumRace < iMissionUnlocked) oMatTemp = oMaterialOctagonUnlocked;
                        //else oMatTemp = oMaterialOctagonLocked;
                    }
                    aMenuLevels[i].oLevelQuadMeshRenderer.material = oMatTemp;
                }
                for (int i = 0; i < aMenuCustomLevels.Length; i++)
                {
                    oMatTemp = oMaterialOctagonUnlocked;
                    if (i == iIndex-200)
                    {
                        
                        if (aMenuCustomLevels[i].oLevel.iLevelType == (int)LevelType.MAP_RACE) oMatTemp = oMaterialPentagonHighlighted;
                        else oMatTemp = oMaterialOctagonHighlighted;
                    }
                    else
                    {
                        if (aMenuCustomLevels[i].oLevel.iLevelType == (int)LevelType.MAP_RACE) oMatTemp = oMaterialPentagonUnlocked;
                    }
                    aMenuCustomLevels[i].oLevelQuadMeshRenderer.material = oMatTemp;
                }

                bHitLevel = true;
            }
            else if (oHitInfo.collider.name.CompareTo("Play") == 0)
            {
                oMenuPlay.oLevelQuadMeshRenderer.material = oMaterialOctagonPlayHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayYR") == 0)
            {
                oMenuReplayYR.oLevelQuadMeshRenderer.material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR1") == 0)
            {
                oMenuReplayWR1.oLevelQuadMeshRenderer.material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR2") == 0)
            {
                oMenuReplayWR2.oLevelQuadMeshRenderer.material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("ReplayWR3") == 0)
            {
                oMenuReplayWR3.oLevelQuadMeshRenderer.material = oMaterialOctagonHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Quit") == 0)
            {
                oMenuQuit.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Controls") == 0)
            {
                oMenuControls.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Credits") == 0)
            {
                oMenuCredits.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Qual1") == 0)
            {
                oMenuQuality1.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Qual2") == 0)
            {
                oMenuQuality2.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Qual3") == 0)
            {
                oMenuQuality3.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Snap") == 0)
            {
                oMenuSnapMovement.oLevelQuadMeshRenderer.material = oMaterialCircleHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Next1") == 0)
            {
                oMenuNext1.oLevelQuadMeshRenderer.material = oMaterialOctagonPlayHighlighted;
            }
            else if (oHitInfo.collider.name.CompareTo("Prev1") == 0)
            {
                oMenuPrev1.oLevelQuadMeshRenderer.material = oMaterialOctagonPlayHighlighted;
            }


            //manage selection
            if (Input.GetButton("Button0") || Input.GetButton("Button1") || Input.GetMouseButton(0) )
            {
                if (iAllowSelection==0)
                {
                    bool bPlaySelectSound = false;
                    if (oHitInfo.collider.name.StartsWith("Coll"))
                    {
                        char[] szName = oHitInfo.collider.name.ToCharArray();
                        string szId = new string(szName, 4, szName.Length - 4);
                        int iIndex = int.Parse(szId);

                        if (iIndex < iRaceUnlocked || (iIndex >= iNumRace && iIndex < iNumRace+iMissionUnlocked))
                        {
                            string szLevel = aLevels[iIndex].szLevelName;
                            GameLevel.iLevelIndex = iIndex;
                            GameLevel.szLevel = szLevel;
                            bLevelSelected = true;
                            iAllowSelection = 30; //trigger once only...
                            bPlaySelectSound = true;
                        }
                        else if (iIndex >= 200)
                        {
                            string szLevel = aMenuCustomLevels[iIndex-200].oLevel.szLevelName;
                            GameLevel.iLevelIndex = iIndex;
                            GameLevel.szLevel = szLevel;
                            bLevelSelected = true;
                            iAllowSelection = 30; //trigger once only...
                            bPlaySelectSound = true;
                        }
                    }
                    else if (oHitInfo.collider.name.CompareTo("Play") == 0)
                    {
                        bLevelPlay = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Next1") == 0)
                    {
                        fAdjust = 1000;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Prev1") == 0)
                    {
                        fAdjust = -1000;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayYR") == 0)
                    {
                        bYourBestReplay = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR1") == 0)
                    {
                        bWorldBestReplay1 = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR2") == 0)
                    {
                        bWorldBestReplay2 = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR3") == 0)
                    {
                        bWorldBestReplay3 = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Quit") == 0)
                    {
                        bQuit = true;
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Controls") == 0)
                    {
                        if (oCreditsQuad == null)
                        {
                            oCreditsQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            oCreditsQuad.transform.parent = Menu.theMenu.transform;
                            oCreditsQuad.transform.localPosition = new Vector3(0, -1.5f, 1.20f);
                            oCreditsQuad.transform.localScale = new Vector3(40.0f, 40.0f, 1.0f);
                            Vector3 vAroundPoint = new Vector3(0, 0, -9.0f);
                            oCreditsQuad.transform.RotateAround(vAroundPoint, Vector3.up, 68);
                        }
#if DISABLESTEAMWORKS
                        oCreditsQuad.GetComponent<MeshRenderer>().material = Resources.Load("Controls", typeof(Material)) as Material;
#else
                        oCreditsQuad.GetComponent<MeshRenderer>().material = Resources.Load("Controls_valve", typeof(Material)) as Material;
#endif

                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Credits") == 0)
                    {
                        if(oCreditsQuad==null)
                        {
                            oCreditsQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            oCreditsQuad.transform.parent = Menu.theMenu.transform;
                            oCreditsQuad.transform.localPosition = new Vector3(0, -1.5f, 1.20f);
                            oCreditsQuad.transform.localScale = new Vector3(40.0f, 40.0f, 1.0f);
                            Vector3 vAroundPoint = new Vector3(0, 0, -9.0f);
                            oCreditsQuad.transform.RotateAround(vAroundPoint, Vector3.up, 68);
                        }
                        oCreditsQuad.GetComponent<MeshRenderer>().material = Resources.Load("Credits", typeof(Material)) as Material;

                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Qual1") == 0)
                    {
                        iQuality = 1;
                        PlayerPrefs.SetInt("MyUnityGraphicsQuality", iQuality);
                        PlayerPrefs.Save();
                        QualitySettings.SetQualityLevel(iQuality, true);
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Qual2") == 0)
                    {
                        iQuality = 2;
                        PlayerPrefs.SetInt("MyUnityGraphicsQuality", iQuality);
                        PlayerPrefs.Save();
                        QualitySettings.SetQualityLevel(iQuality, true);
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Qual3") == 0)
                    {
                        iQuality = 4;
                        PlayerPrefs.SetInt("MyUnityGraphicsQuality", iQuality);
                        PlayerPrefs.Save();
                        QualitySettings.SetQualityLevel(iQuality, true);
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Snap") == 0)
                    {
                        CameraController.bSnapMovement = !CameraController.bSnapMovement;
                        PlayerPrefs.SetInt("MyUseSnapMovement", CameraController.bSnapMovement?1:0);
                        PlayerPrefs.Save();
                        iAllowSelection = 30;
                        bPlaySelectSound = true;
                    }
                    else if (oHitInfo.collider.name.CompareTo("Back") == 0)
                    {

                    }

                    if (bPlaySelectSound) GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
                }
            }
            else
            {
                if (iAllowSelection > 0) iAllowSelection--;
            }
        }
        else
        {
            //no hit, place cursor at max distance

            //first, unselect level if click outside levelinfo
            if (Input.GetButton("Button0") || Input.GetButton("Button1") || Input.GetMouseButton(0))
            {
                if (iAllowSelection==0)
                {
                    bLevelUnSelected = true;
                    iAllowSelection = 30;
                }
            }
            else
            {
                if (iAllowSelection > 0) iAllowSelection--;
            }

            //set at max distance
            oGazeQuad.transform.position = vHeadPosition+ vGazeDirection*17.0f;
            //rotate the cursor to camera rotation
            oGazeQuad.transform.rotation = Camera.main.transform.rotation;
        }

        //nothing highlighted?
        if(!bHitLevel)
        {
            for (int i = 0; i < iNumRace + iNumMission; i++)
            {
                if (i < iRaceUnlocked) oMatTemp = oMaterialPentagonUnlocked;
                else if (i < iNumRace) oMatTemp = oMaterialPentagonLocked;
                else if (i - iNumRace < iMissionUnlocked) oMatTemp = oMaterialOctagonUnlocked;
                else oMatTemp = oMaterialOctagonLocked;
                aMenuLevels[i].oLevelQuadMeshRenderer.material = oMatTemp;
            }
            for (int i = 0; i < aMenuCustomLevels.Length; i++)
            {
                oMatTemp = oMaterialOctagonUnlocked;
                aMenuCustomLevels[i].oLevelQuadMeshRenderer.material = oMatTemp;
            }
        }

        //next/prev
        if (fAdjust != 0)
        {
            float x = oCameraHolder.transform.position.x + fAdjust;
            if (x >= 0.0f && x <= 1000.0f)
                oCameraHolder.transform.position = new Vector3(x, 0, -5.0f);
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
        public MeshRenderer oLevelQuadMeshRenderer;
        public GameObject oRankQuad;
        GameObject oLevelText;

        Vector3 vPos;
        float fRotateAngle;
        Vector3 vAroundPoint;
        public S_Levels oLevel;

        public C_LevelInMenu(Vector3 i_vPos, Vector3 i_vAroundPoint, float i_fRotateAngle, S_Levels i_oLevel, int i_iLevelId)
        {
            vPos = i_vPos;
            fRotateAngle = i_fRotateAngle;
            vAroundPoint = i_vAroundPoint;
            oLevel = i_oLevel;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = "Coll"+i_iLevelId.ToString();
            oLevelQuad.transform.position = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(10.0f, 10.0f, 1.0f);
            oLevelQuad.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.value*100.0f); //vary 100 deg around z
            oLevelQuad.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);
            oLevelQuadMeshRenderer = oLevelQuad.GetComponent<MeshRenderer>();
            oLevelQuadMeshRenderer.material = (i_oLevel.iLevelType == (int)LevelType.MAP_RACE) ?
                Menu.theMenu.oMaterialPentagonUnlocked : Menu.theMenu.oMaterialOctagonUnlocked;

            //quad for level ranking star
            //set in InitLevelRanking() when received from server

            //level text
            if (i_iLevelId >= 200)
            {
                oLevelText = Instantiate(Menu.theMenu.oTMProBaseObj1, Menu.theMenu.transform);
                oLevelText.transform.position = new Vector3(vPos.x + .63f, vPos.y - .185f, vPos.z - .16f);
            }
            else
            {
                oLevelText = Instantiate(Menu.theMenu.oTMProBaseObj, Menu.theMenu.transform);
                oLevelText.transform.position = new Vector3(vPos.x - .63f, vPos.y - .345f, vPos.z - .16f);
            }
            oLevelText.transform.localScale = new Vector3(1.85f, 1.85f, 1.0f);
            oLevelText.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);
            TextMeshPro tm = oLevelText.GetComponent<TextMeshPro>();
            tm.text = i_oLevel.szLevelDisplayName;
            oLevelText.SetActive(true);
        }

        public void InitLevelRanking(int i_iLevelId)
        {
            if (oRankQuad != null) Destroy(oRankQuad);

            if (GameManager.theGM.oHigh.bIsDone && i_iLevelId < GameManager.theGM.oHigh.oLevelList.Count)
            {
                LevelInfo stLevelInfo = GameManager.theGM.oHigh.oLevelList[i_iLevelId];
                int iRank = 5; //no score at all
                if (stLevelInfo.iBestScoreMs != -1)
                {
                    iRank = 4; //a score less than bronze
                    if (stLevelInfo.bIsTime)
                    {
                        if (stLevelInfo.iBestScoreMs < stLevelInfo.iLimit1) iRank = 1; //gold
                        else if (stLevelInfo.iBestScoreMs < stLevelInfo.iLimit2) iRank = 2; //silver
                        else if (stLevelInfo.iBestScoreMs < stLevelInfo.iLimit3) iRank = 3; //bronze
                    }
                    else
                    {
                        if (stLevelInfo.iBestScoreMs >= stLevelInfo.iLimit1) iRank = 1; //gold
                        else if (stLevelInfo.iBestScoreMs >= stLevelInfo.iLimit2) iRank = 2; //silver
                        else if (stLevelInfo.iBestScoreMs >= stLevelInfo.iLimit3) iRank = 3; //bronze
                    }
                }
                Material oMaterial = null;
                if (iRank == 3) oMaterial = Menu.theMenu.oMaterialRankBronze;
                if (iRank == 2) oMaterial = Menu.theMenu.oMaterialRankSilver;
                if (iRank == 1) oMaterial = Menu.theMenu.oMaterialRankGold;

                if (oMaterial != null)
                {
                    oRankQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    oRankQuad.transform.parent = Menu.theMenu.transform;
                    oRankQuad.transform.position = new Vector3(vPos.x + .35f, vPos.y - .35f, vPos.z - .17f);
                    oRankQuad.transform.localScale = new Vector3(4.0f, 4.0f, 1.0f);
                    oRankQuad.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.value * 100.0f); //vary 100 deg around z
                    oRankQuad.transform.RotateAround(vAroundPoint, Vector3.up, fRotateAngle);
                    oRankQuad.GetComponent<MeshRenderer>().material = oMaterial;
                }
            }
        }
    }

    public class C_ItemInMenu
    {
        public GameObject oLevelQuad;
        public MeshRenderer oLevelQuadMeshRenderer;
        public GameObject oLevelText;

        Vector3 vPos;

        /*public void DestroyObj()
        {
            Destroy(oLevelQuad);
            Destroy(oLevelText);
        }*/

        public void Reinit(Vector3 i_vPos, string i_szText, string i_szCollID, float i_fScale, float i_fScaleText)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad.transform.parent = Menu.theMenu.oLevelInfoContainer.transform;
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = i_szCollID;
            oLevelQuad.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(i_fScale * 0.4f, i_fScale * 0.4f, 1.0f);
            oLevelQuad.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already
            oLevelQuadMeshRenderer = oLevelQuad.GetComponent<MeshRenderer>();
            oLevelQuadMeshRenderer.material = Menu.theMenu.oMaterialPentagonUnlocked;

            //create text
            oLevelText.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z - 0.1f);
            oLevelText.transform.localScale = new Vector3(i_fScaleText * 0.08f, i_fScaleText * 0.08f, 1.0f);
            oLevelText.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already

            TextMesh oLevelTextTextMesh = oLevelText.GetComponent<TextMesh>();
            /*oLevelTextTextMesh.fontStyle = FontStyle.Bold;
            oLevelTextTextMesh.fontSize = 40;
            oLevelTextTextMesh.anchor = TextAnchor.MiddleCenter;*/
            oLevelTextTextMesh.text = i_szText;
        }

        public C_ItemInMenu(Vector3 i_vPos, string i_szText, string i_szCollID, float i_fScale, float i_fScaleText)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.oLevelInfoContainer.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = i_szCollID;
            oLevelQuad.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(i_fScale * 0.4f, i_fScale * 0.4f, 1.0f);
            oLevelQuad.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already
            oLevelQuadMeshRenderer = oLevelQuad.GetComponent<MeshRenderer>();
            oLevelQuadMeshRenderer.material = Menu.theMenu.oMaterialPentagonUnlocked;

            //create text
            oLevelText = new GameObject();
            oLevelText.transform.parent = Menu.theMenu.oLevelInfoContainer.transform;
            oLevelText.name = "TextMesh";
            oLevelText.AddComponent<TextMesh>();
            oLevelText.transform.localPosition = new Vector3(vPos.x, vPos.y, vPos.z - 0.1f);
            oLevelText.transform.localScale = new Vector3(i_fScaleText * 0.08f, i_fScaleText * 0.08f, 1.0f);
            oLevelText.transform.rotation = Menu.theMenu.oLevelInfoContainer.transform.rotation; //why doesn't this come from the parent already

            TextMesh oLevelTextTextMesh = oLevelText.GetComponent<TextMesh>();
            oLevelTextTextMesh.fontStyle = FontStyle.Bold;
            oLevelTextTextMesh.fontSize = 40;
            oLevelTextTextMesh.anchor = TextAnchor.MiddleCenter;
            oLevelTextTextMesh.text = i_szText;
        }
    }

    public class C_Item2InMenu
    {
        public GameObject oLevelQuad;
        public MeshRenderer oLevelQuadMeshRenderer;
        GameObject oLevelText;

        Vector3 vPos;

        public void DestroyObj()
        {
            Destroy(oLevelQuad);
            Destroy(oLevelText);
        }

        public C_Item2InMenu(Vector3 i_vPos, Vector3 i_vAroundPoint, float i_fRotateAngle, string i_szText, string i_szCollID, float i_fScale, float i_fScaleText)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = i_szCollID;
            /**/oLevelQuad.transform.position = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(i_fScale * 0.4f, i_fScale * 0.4f, 1.0f);
            oLevelQuad.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);
            if (i_szText.CompareTo("<") == 0)
            {
                //oLevelQuad.transform.eulerAngles = oLevelQuad.transform.eulerAngles + new Vector3(0, 0, 180);
                oLevelQuad.transform.Rotate(Vector3.back, 180);
                i_szText = "";
            }
            oLevelQuadMeshRenderer = oLevelQuad.GetComponent<MeshRenderer>();
            oLevelQuadMeshRenderer.material = Menu.theMenu.oMaterialCircle;

            //create text
            oLevelText = Instantiate(Menu.theMenu.oTMProBaseObj, Menu.theMenu.transform);
            /**/oLevelText.transform.position = new Vector3(vPos.x - .58f, vPos.y - .17f, vPos.z - .12f);
            oLevelText.transform.localScale = new Vector3(1.6f, 1.6f, 1.0f);
            oLevelText.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);
            oLevelText.GetComponent<TextMeshPro>().text = i_szText;
            oLevelText.SetActive(true);
        }
    }

}
