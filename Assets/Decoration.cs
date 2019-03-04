using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    public GameObject oBarrels;
    public GameObject oTree;
    public GameObject oTreeBlack;
    public GameObject oTreeWhite;

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
