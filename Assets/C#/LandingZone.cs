using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingZone : MonoBehaviour
{
    internal int iId;
    Vector2 vPos;

    int iWidth, iHeight;
    int iZoneSize; //width in tiles

    //other
    internal bool bHomeBase;
    internal bool bExtraLife;
    bool bShowAntenna, bShowHouse;
    List<int> aCargoList; //array of cargo weights (small = 1..5,6..10,11..15,16..20+ = huge)

    public GameObject oZone;
    GameObject[] oZoneCargoList;
    GameObject oZoneAttentionMarker = null;

    Material oMaterialZone, oMaterialHome, oMaterialCargo;

    void Start()
    {

    }

    public void Init(int i_iId, Vector2 i_vPos, int i_iWidth, float i_fDepth, bool i_bHomeBase,
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

        //zone cargo
        for (int i = 0; i < aCargoList.Count; i++)
        {
            int iCargoType = 3 - (aCargoList[i] - 1) / 5; //1..5=small container, 6..10, 11..15, 16..20+ = large container
            if (iCargoType < 0) iCargoType = 0;
            //if(iCargoType>3) iCargoType=3;

            GameObject oBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oBox.transform.parent = GameLevel.theMap.transform;
            MonoBehaviour.DestroyImmediate(oBox.GetComponent<BoxCollider>());

            oBox.transform.position = new Vector3(vPos.x + ((iAdjustX - ((i / 3) * 28)) - 13) / 32.0f, vPos.y + (6 + ((i % 3) * 7)) / 32.0f, /**/0.6f);
            oBox.transform.localScale = new Vector3(aBoxSizeX[iCargoType] / 32.0f, 6.0f / 32.0f, 0.5f);

            oMaterial = Resources.Load("Pickups", typeof(Material)) as Material;
            oBox.GetComponent<MeshRenderer>().material = oMaterial;

            oZoneCargoList[i] = oBox;
        }

        //zone landing pad
        //oZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        oZone.transform.parent = GameLevel.theMap.transform;
        MonoBehaviour.DestroyImmediate(oZone.GetComponent<BoxCollider>());
        oZone.AddComponent<BoxCollider2D>();
        oZone.name = "LandingZone" + iId.ToString();
        oZone.transform.position = new Vector3(vPos.x, vPos.y, 0);
        oZone.transform.localScale = new Vector3(iZoneSize * 1.0f, 4.0f / 32.0f, i_fDepth);

        oMaterialZone = Resources.Load("LandingZone", typeof(Material)) as Material;
        oZone.GetComponent<MeshRenderer>().material = oMaterialZone;

        //attention marker
        if(GameLevel.theMap.iLevelType == (int)LevelType.MAP_MISSION)
        {
            oZoneAttentionMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oZoneAttentionMarker.transform.parent = GameLevel.theMap.transform;
            MonoBehaviour.DestroyImmediate(oZoneAttentionMarker.GetComponent<BoxCollider>());
            oZoneAttentionMarker.transform.position = new Vector3(vPos.x, vPos.y, -((i_fDepth + 0.4f) / 2.0f));
            oZoneAttentionMarker.transform.localScale = new Vector3(iZoneSize * 1.0f, 4.0f / 32.0f, 0.4f);

            oMaterialHome = Resources.Load("LandingZoneHome", typeof(Material)) as Material;
            oMaterialCargo = Resources.Load("LandingZoneCargo", typeof(Material)) as Material;
        }

        gameObject.SetActive(true);
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

    int iCntr = 0;
    void Update()
    {
        iCntr++;
        if(iCntr%50==0 && oZoneAttentionMarker!=null)
        {
            //update material on attention marker
            if (bHomeBase) oZoneAttentionMarker.GetComponent<MeshRenderer>().material = oMaterialHome;
            else if (GetTotalCargo() > 0) oZoneAttentionMarker.GetComponent<MeshRenderer>().material = oMaterialCargo;
            else oZoneAttentionMarker.GetComponent<MeshRenderer>().material = oMaterialZone;
        }
    }
}
