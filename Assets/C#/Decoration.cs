using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    public GameObject oBarrels;
    public GameObject oTree;
    public GameObject oTreeBlack;
    public GameObject oTreeWhite;
    public GameObject oHouseLeft;
    public GameObject oHouseMid;
    public GameObject oHouseRight;
    public GameObject oHouseLeftRender;
    public GameObject oHouseMidRender;
    public GameObject oHouseRightRender;

    void Start()
    {
        
    }

    public void Init(int i_iType, Vector2 i_vPos)
    {
        gameObject.SetActive(true);
        oBarrels.SetActive(i_iType == 0);
        oTree.SetActive(i_iType == 1);
        oTreeBlack.SetActive(i_iType == 2);
        oTreeWhite.SetActive(i_iType == 3);

        oHouseLeft.SetActive(i_iType == 4 || i_iType == 7);
        oHouseMid.SetActive(i_iType == 5 || i_iType == 8);
        oHouseRight.SetActive(i_iType == 6 || i_iType == 9);
        Material oGreenHouse = Resources.Load("Material_green_house", typeof(Material)) as Material;
        if (i_iType == 7) oHouseLeftRender.GetComponent<MeshRenderer>().material = oGreenHouse;
        if (i_iType == 8) oHouseMidRender.GetComponent<MeshRenderer>().material = oGreenHouse;
        if (i_iType == 9) oHouseRightRender.GetComponent<MeshRenderer>().material = oGreenHouse;

        if (i_iType == 0)
        {
            transform.position = new Vector3(i_vPos.x, i_vPos.y, -0.0f);
        }
        else
        {
            transform.position = new Vector3(i_vPos.x, i_vPos.y, -0.0f);
        }
    }

    void Update()
    {
        
    }
}
