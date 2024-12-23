﻿using System.Collections;
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
    public GameObject oRadioTower;
    public GameObject oTreeCactus;

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
        oRadioTower.SetActive(i_iType == 10);
        oTreeCactus.SetActive(i_iType == 11);

        oHouseLeft.SetActive(i_iType == 4 || i_iType == 7);
        oHouseMid.SetActive(i_iType == 5 || i_iType == 8);
        oHouseRight.SetActive(i_iType == 6 || i_iType == 9);
        Material oBlueHouse = Resources.Load("Material_house_blue", typeof(Material)) as Material;
        if (i_iType == 7) oHouseLeftRender.GetComponent<MeshRenderer>().material = oBlueHouse;
        if (i_iType == 8) oHouseMidRender.GetComponent<MeshRenderer>().material = oBlueHouse;
        if (i_iType == 9) oHouseRightRender.GetComponent<MeshRenderer>().material = oBlueHouse;

        transform.position = new Vector3(i_vPos.x, i_vPos.y, -0.0f);
    }

    void Update()
    {
        
    }
}
