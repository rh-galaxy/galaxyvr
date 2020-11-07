using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZObject : MonoBehaviour
{
    public GameObject U0;
    public GameObject U1;
    public GameObject W0;
    public GameObject W1;

    public void Init(int i_iType, float i_fRotation, Vector2 i_vPos)
    {
        gameObject.SetActive(true);
        U0.SetActive(i_iType == 0);
        U1.SetActive(i_iType == 1);
        W0.SetActive(i_iType == 2);
        W1.SetActive(i_iType == 3);

        if(i_iType < 2)
        {
            transform.position = new Vector3(i_vPos.x, i_vPos.y, -.30f);
        }
        else
        {
            transform.position = new Vector3(i_vPos.x, i_vPos.y, -.35f);
        }
        transform.Rotate(Vector3.forward, i_fRotation);
    }

    void Update()
    {
        
    }
}
