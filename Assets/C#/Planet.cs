using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public GameObject oPlanet1;
    public GameObject oPlanet2;
    public GameObject oPlanet3;
    public GameObject oPlanet4;
    public GameObject oPlanet5;

    int iPlanet;

    void Start()
    {
        
    }

    public void Init(int i_iPlanet)
    {
        iPlanet = i_iPlanet;

        switch (iPlanet)
        {
            case 1: oPlanet1.SetActive(true); break;
            case 2: oPlanet2.SetActive(true); break;
            case 3: oPlanet3.SetActive(true); break;
            case 4: oPlanet4.SetActive(true); break;
            case 5: oPlanet5.SetActive(true); break;
        }
    }

    void Update()
    {
        switch (iPlanet)
        {
            case 1: oPlanet1.transform.RotateAround(oPlanet1.transform.position, Vector3.up, 0.3f * Time.deltaTime); break;
            case 2: oPlanet2.transform.RotateAround(oPlanet2.transform.position, Vector3.up, 0.2f * Time.deltaTime); break;
            case 3: oPlanet3.transform.RotateAround(oPlanet3.transform.position, Vector3.up, 0.4f * Time.deltaTime); break;
            case 4: oPlanet4.transform.RotateAround(oPlanet4.transform.position, Vector3.up, 0.3f * Time.deltaTime); break;
            case 5: oPlanet5.transform.RotateAround(oPlanet5.transform.position, Vector3.up, 0.1f * Time.deltaTime); break;
        }
    }
}
