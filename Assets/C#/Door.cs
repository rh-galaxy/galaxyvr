using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject oBase1;
    public GameObject oBase2;
    public GameObject oDoor1;
    public GameObject oDoor2;

    S_DoorInfo stDoorInfo;

    //animation
    bool bIsOpening;
    float fOpenPos;
    float fStateTime;

    void Start()
    {

    }

    void SetOpenPos()
    {
        float fHalfMove = (stDoorInfo.fLength - fOpenPos) / 2.0f;
        oDoor1.transform.localPosition = new Vector3(0.0f, -0.1f - fHalfMove/2, 0.0f);
        oDoor2.transform.localPosition = new Vector3(0.0f, -0.1f + (stDoorInfo.fLength + 1.125f) - fHalfMove/2, 0.0f);

        oDoor1.transform.localScale = new Vector3(0.18f, 1.10f + fHalfMove, 3.2f);
        oDoor2.transform.localScale = new Vector3(0.18f, 1.10f + fHalfMove, 3.2f);

        //fOpenPos
    }

    public void Init(S_DoorInfo i_stDoorInfo, GameLevel i_oMap)
    {
        stDoorInfo = i_stDoorInfo;
        fOpenPos = 0;

        gameObject.SetActive(true);
        if (stDoorInfo.bHorizontal) transform.Rotate(Vector3.back, 270, Space.Self);
        transform.localPosition = new Vector3(stDoorInfo.vPos.x, stDoorInfo.vPos.y, 1.0f);

        //never changes
        oBase1.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        oBase2.transform.localPosition = new Vector3(0.0f, stDoorInfo.fLength + 1.125f, 0.0f);

        //variable
        SetOpenPos();
    }

    void FixedUpdate()
    {
        if (bIsOpening)
        {
            fOpenPos += Time.fixedDeltaTime * stDoorInfo.fOpeningSpeed;
            if (fOpenPos >= stDoorInfo.fLength)
            {
                fOpenPos = stDoorInfo.fLength;
                fStateTime += Time.fixedDeltaTime;
                if (fStateTime >= stDoorInfo.fOpenedForTime)
                {
                    fStateTime = 0;
                    bIsOpening = false;
                }
            }
        }
        else
        {
            fOpenPos -= Time.fixedDeltaTime * stDoorInfo.fClosingSpeed;
            if (fOpenPos <= 0)
            {
                fOpenPos = 0;
                fStateTime += Time.fixedDeltaTime;
                if (fStateTime >= stDoorInfo.fClosedForTime)
                {
                    if (stDoorInfo.iNumButtons == 0)
                    {
                        fStateTime = 0;
                        bIsOpening = true; //interval opening
                    }
                }
            }
        }

        SetOpenPos();
    }
}