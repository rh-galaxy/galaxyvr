﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class Menu : MonoBehaviour
{
    public static Menu theMenu = null;
    public static bool bLevelSelected = false;
    public static bool bLevelPlay = false;
    public static bool bYourBestReplay = false;
    public static bool bWorldBestReplay = false;
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
    }
    S_Levels[] aLevels = new S_Levels[NUM_LEVELS];

    C_LevelInMenu[] aMenuLevels = new C_LevelInMenu[NUM_LEVELS];
    C_ItemInMenu oMenuReplayWR, oMenuReplayYR, oMenuPlay;

    GameObject oGazeQuad;

    Material oCursorMaterial, oCursorMaterialWait;

    private void Awake()
    {
        theMenu = this;

        aLevels[0].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[0].szLevelName = "2race00";
        aLevels[0].szLevelDisplayName = "R00";
        aLevels[1].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[1].szLevelName = "2race01";
        aLevels[1].szLevelDisplayName = "R01";
        aLevels[2].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[2].szLevelName = "2race02";
        aLevels[2].szLevelDisplayName = "R02";
        aLevels[3].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[3].szLevelName = "2race03";
        aLevels[3].szLevelDisplayName = "R03";
        aLevels[4].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[4].szLevelName = "2race04";
        aLevels[4].szLevelDisplayName = "R04";
        aLevels[5].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[5].szLevelName = "2race05";
        aLevels[5].szLevelDisplayName = "R05";
        aLevels[6].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[6].szLevelName = "2race06";
        aLevels[6].szLevelDisplayName = "R06";
        aLevels[7].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[7].szLevelName = "2race07";
        aLevels[7].szLevelDisplayName = "R07";
        aLevels[8].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[8].szLevelName = "2race08";
        aLevels[8].szLevelDisplayName = "R08";
        aLevels[9].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[9].szLevelName = "2race09";
        aLevels[9].szLevelDisplayName = "R09";
        aLevels[10].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[10].szLevelName = "2race10";
        aLevels[10].szLevelDisplayName = "R10";
        aLevels[11].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[11].szLevelName = "2race11";
        aLevels[11].szLevelDisplayName = "R11";
        aLevels[12].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[12].szLevelName = "2race12";
        aLevels[12].szLevelDisplayName = "R12";
        aLevels[13].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[13].szLevelName = "2race13";
        aLevels[13].szLevelDisplayName = "R13";
        aLevels[14].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[14].szLevelName = "2race14";
        aLevels[14].szLevelDisplayName = "R14";
        aLevels[15].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[15].szLevelName = "2race15";
        aLevels[15].szLevelDisplayName = "R15";
        aLevels[16].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[16].szLevelName = "2race16";
        aLevels[16].szLevelDisplayName = "R16";
        aLevels[17].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[17].szLevelName = "2race17";
        aLevels[17].szLevelDisplayName = "R17";
        aLevels[18].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[18].szLevelName = "2race18";
        aLevels[18].szLevelDisplayName = "R18";
        aLevels[19].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[19].szLevelName = "2race19"; //2race00_zero
        aLevels[19].szLevelDisplayName = "R19";
        aLevels[20].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[20].szLevelName = "2race20"; //2race04_jupiter
        aLevels[20].szLevelDisplayName = "R20";
        aLevels[21].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[21].szLevelName = "2race21"; //2race10_mod
        aLevels[21].szLevelDisplayName = "R21";
        aLevels[22].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[22].szLevelName = "2race22";
        aLevels[22].szLevelDisplayName = "R22";
        aLevels[23].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[23].szLevelName = "2race23";
        aLevels[23].szLevelDisplayName = "R23";
        aLevels[24].iLevelType = (int)LevelType.MAP_RACE;
        aLevels[24].szLevelName = "2race24";
        aLevels[24].szLevelDisplayName = "R24";

        aLevels[25].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[25].szLevelName = "1mission00";
        aLevels[25].szLevelDisplayName = "M00";
        aLevels[26].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[26].szLevelName = "1mission01";
        aLevels[26].szLevelDisplayName = "M01";
        aLevels[27].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[27].szLevelName = "1mission02";
        aLevels[27].szLevelDisplayName = "M02";
        aLevels[28].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[28].szLevelName = "1mission03";
        aLevels[28].szLevelDisplayName = "M03";
        aLevels[29].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[29].szLevelName = "1mission04";
        aLevels[29].szLevelDisplayName = "M04";
        aLevels[30].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[30].szLevelName = "1mission05";
        aLevels[30].szLevelDisplayName = "M05";
        aLevels[31].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[31].szLevelName = "1mission06";
        aLevels[31].szLevelDisplayName = "M06";
        aLevels[32].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[32].szLevelName = "1mission07";
        aLevels[32].szLevelDisplayName = "M07";
        aLevels[33].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[33].szLevelName = "1mission08";
        aLevels[33].szLevelDisplayName = "M08";
        aLevels[34].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[34].szLevelName = "1mission09";
        aLevels[34].szLevelDisplayName = "M09";
        aLevels[35].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[35].szLevelName = "1mission10";
        aLevels[35].szLevelDisplayName = "M10";
        aLevels[36].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[36].szLevelName = "1mission11";
        aLevels[36].szLevelDisplayName = "M11";
        aLevels[37].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[37].szLevelName = "1mission12";
        aLevels[37].szLevelDisplayName = "M12";
        aLevels[38].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[38].szLevelName = "1mission13";
        aLevels[38].szLevelDisplayName = "M13";
        aLevels[39].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[39].szLevelName = "1mission14";
        aLevels[39].szLevelDisplayName = "M14";
        aLevels[40].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[40].szLevelName = "1mission15";
        aLevels[40].szLevelDisplayName = "M15";
        aLevels[41].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[41].szLevelName = "1mission16";
        aLevels[41].szLevelDisplayName = "M16";
        aLevels[42].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[42].szLevelName = "1mission17";
        aLevels[42].szLevelDisplayName = "M17";
        aLevels[43].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[43].szLevelName = "1mission18"; //1mission01_mod
        aLevels[43].szLevelDisplayName = "M18";
        aLevels[44].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[44].szLevelName = "1mission19"; //1mission04_mod
        aLevels[44].szLevelDisplayName = "M19";
        aLevels[45].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[45].szLevelName = "1mission20"; //1mission06_mod
        aLevels[45].szLevelDisplayName = "M20";
        aLevels[46].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[46].szLevelName = "1mission21"; //1mission_narrow
        aLevels[46].szLevelDisplayName = "M21";
        aLevels[47].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[47].szLevelName = "1mission22"; //1mission_hard
        aLevels[47].szLevelDisplayName = "M22";
        aLevels[48].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[48].szLevelName = "1mission23";
        aLevels[48].szLevelDisplayName = "M23";
        aLevels[49].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[49].szLevelName = "1mission24";
        aLevels[49].szLevelDisplayName = "M24";
        aLevels[50].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[50].szLevelName = "1mission25";
        aLevels[50].szLevelDisplayName = "M25";
        aLevels[51].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[51].szLevelName = "1mission26";
        aLevels[51].szLevelDisplayName = "M26";
        aLevels[52].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[52].szLevelName = "1mission27";
        aLevels[52].szLevelDisplayName = "M27";
        aLevels[53].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[53].szLevelName = "1mission28";
        aLevels[53].szLevelDisplayName = "M28";
        aLevels[54].iLevelType = (int)LevelType.MAP_MISSION;
        aLevels[54].szLevelName = "1mission29";
        aLevels[54].szLevelDisplayName = "M29";
    }

    void Start()
    {
        //int iLen = NUM_LEVELS / 2;
        float fStartAngle = -65;
        float fAngleRange = 130;
        for (int i = 0; i < iNumRace; i++)
        {
            Vector3 vPos = new Vector3(0, (i % 2) * 10.0f - 17.0f, 12.0f);
            Vector3 vAroundPoint = new Vector3(0, 0, -90);
            float fRotateAngle = fStartAngle + i * (fAngleRange / (iNumRace - 1));
            aMenuLevels[i] = new C_LevelInMenu(vPos, vAroundPoint, fRotateAngle, aLevels[i], i);
        }
        int iStartOffs = iNumRace;
        for (int i = 0; i < iNumMission; i++)
        {
            Vector3 vPos = new Vector3(0, (i % 2) * 10.0f + 17.0f, 12.0f);
            Vector3 vAroundPoint = new Vector3(0, 0, -90);
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
    }

    Texture2D oMiniMapTex;
    GameObject oMiniMapQuad = null;
    public void SetLevelInfo(LevelInfo i_stLevelInfo)
    {
        Vector3 vPos = new Vector3(-11, 4.0f, 12.0f);
        if (oMenuReplayWR != null) oMenuReplayWR.DestroyObj();
        oMenuReplayWR = new C_ItemInMenu(vPos, "WR", "ReplayWR");
        vPos.x = 0;
        if (oMenuReplayYR != null) oMenuReplayYR.DestroyObj();
        oMenuReplayYR = new C_ItemInMenu(vPos, "YR", "ReplayYR");
        vPos.x = 11;
        if (oMenuPlay != null) oMenuPlay.DestroyObj();
        oMenuPlay = new C_ItemInMenu(vPos, "P", "Play");

        //i_stLevelInfo.szName is in the form "race00", but we need the filename "2race00"
        //we rely on GameLevel.szLevel for that
        oMiniMapTex = GameLevel.GetMiniMap(GameLevel.szLevel);

        if(oMiniMapQuad==null) oMiniMapQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Material oMaterial = Resources.Load("MiniMap", typeof(Material)) as Material;
        oMaterial.mainTexture = oMiniMapTex;
        oMiniMapQuad.GetComponent<MeshRenderer>().material = oMaterial;
        oMiniMapQuad.transform.parent = Menu.theMenu.transform;
        oMiniMapQuad.transform.position = new Vector3(22.0f, vPos.y, vPos.z);
        oMiniMapQuad.transform.localScale = new Vector3(10.0f, 10.0f, 1.0f);
    }

    float fRotateZAngle = 0.0f;
    void Update()
    {
        //do a raycast into the world based on the user's
        // head position and orientation
        Vector3 vHeadPosition = Camera.main.transform.position;
        Vector3 vGazeDirection = Camera.main.transform.forward;

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
            string szLevel = null;

            if (Input.GetButton("Fire1"))
            {
                if (bAllowSelection)
                {
                    if (oHitInfo.collider.name.StartsWith("Coll"))
                    {
                        char[] szName = oHitInfo.collider.name.ToCharArray();
                        string szId = new string(szName, 4, szName.Length - 4);
                        int iIndex = int.Parse(szId);
                        szLevel = aLevels[iIndex].szLevelName;

                        GameLevel.iLevelIndex = iIndex;
                        GameLevel.szLevel = szLevel;
                        bLevelSelected = true;
                        bAllowSelection = false; //trigger once only...
                    }
                    else if (oHitInfo.collider.name.CompareTo("Play") == 0)
                    {
                        bLevelPlay = true;
                        bAllowSelection = false;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayYR") == 0)
                    {
                        bYourBestReplay = true;
                        bAllowSelection = false;
                    }
                    else if (oHitInfo.collider.name.CompareTo("ReplayWR") == 0)
                    {
                        bWorldBestReplay = true;
                        bAllowSelection = false;
                    }
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
            oGazeQuad.transform.position = vHeadPosition+ vGazeDirection*180.0f;
            //rotate the cursor to camera rotation
            oGazeQuad.transform.rotation = Camera.main.transform.rotation;

            bLevelSelected = false;
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
            oLevelQuad.transform.position = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(10.0f, 10.0f, 1.0f);
            oLevelQuad.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.value*100.0f); //vary 100 deg around z
            oLevelQuad.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);

            string szMaterial = (i_oLevel.iLevelType == (int)LevelType.MAP_RACE) ? "LevelPentagon" : "LevelOctagon";
            Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
            oLevelQuad.GetComponent<MeshRenderer>().material = oMaterial;

            //create text
            oLevelText = new GameObject();
            oLevelText.transform.parent = Menu.theMenu.transform;
            oLevelText.name = "TextMesh";
            oLevelText.AddComponent<TextMesh>();
            oLevelText.transform.position = new Vector3(vPos.x, vPos.y, vPos.z-0.1f);
            oLevelText.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
            oLevelText.transform.RotateAround(i_vAroundPoint, Vector3.up, i_fRotateAngle);

            oLevelText.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
            oLevelText.GetComponent<TextMesh>().fontSize = 40;
            oLevelText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            oLevelText.GetComponent<TextMesh>().text = i_oLevel.szLevelDisplayName;
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

        public C_ItemInMenu(Vector3 i_vPos, string szText, string szCollID)
        {
            vPos = i_vPos;

            //create a quad with a text on, in the pos of each menu object
            oLevelQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oLevelQuad.transform.parent = Menu.theMenu.transform;
            oLevelQuad.AddComponent<BoxCollider>();
            BoxCollider oCollider = oLevelQuad.GetComponent<BoxCollider>(); oCollider.name = szCollID;
            oLevelQuad.transform.position = new Vector3(vPos.x, vPos.y, vPos.z);
            oLevelQuad.transform.localScale = new Vector3(10.0f, 10.0f, 1.0f);
            oLevelQuad.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.value * 100.0f); //vary 100 deg around z

            string szMaterial = "LevelOctagon";
            Material oMaterial = Resources.Load(szMaterial, typeof(Material)) as Material;
            oLevelQuad.GetComponent<MeshRenderer>().material = oMaterial;

            //create text
            oLevelText = new GameObject();
            oLevelText.transform.parent = Menu.theMenu.transform;
            oLevelText.name = "TextMesh";
            oLevelText.AddComponent<TextMesh>();
            oLevelText.transform.position = new Vector3(vPos.x, vPos.y, vPos.z - 0.1f);
            oLevelText.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);

            oLevelText.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
            oLevelText.GetComponent<TextMesh>().fontSize = 40;
            oLevelText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            oLevelText.GetComponent<TextMesh>().text = szText;
        }
    }

}
