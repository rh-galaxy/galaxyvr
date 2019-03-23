using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject oPlayer;
    public GameLevel oMap;

    private Vector3 vCamOffset;
    private Vector3 vMapSize;

    internal static bool bSnapMovement = false;
    internal static Vector3 vCamPos = new Vector3(0,0,0);

    // Start is called before the first frame update
    void Start()
    {
        vCamOffset = new Vector3(0,3,-10);
        vMapSize = oMap.GetMapSize();
    }

    // Update is called once per frame
    float fX = 5, fY = 0, fZ = 0;
    float fSnapTimer = 0;
    bool bFirst = true;
    void LateUpdate()
    {
        //emulate headset movement
        if (Input.GetKey(KeyCode.F) || GameManager.bNoVR)
        {
            if (Input.GetKey(KeyCode.G)) fZ += Input.GetAxisRaw("Mouse X") * 3.0f;
            else fY += Input.GetAxisRaw("Mouse X") * 3.0f;
            fX -= Input.GetAxisRaw("Mouse Y") * 3.0f;
            if (Input.GetKey(KeyCode.R)) { fX = 5; fY = 0; fZ = 0; }

            transform.eulerAngles = new Vector3(fX, fY, fZ);
        }

        Vector3 v = oPlayer.transform.position + vCamOffset;
        float fLeftLimit = -(vMapSize.x / 2.0f) + 5;
        float fRightLimit = (vMapSize.x / 2.0f) - 5;
        if (v.x < fLeftLimit) v.x = fLeftLimit;
        if (v.x > fRightLimit) v.x = fRightLimit;
        float fTopLimit = (vMapSize.y / 2.0f) - 3;
        float fBottomLimit = -(vMapSize.y / 2.0f) + 10;
        if (v.y < fBottomLimit) v.y = fBottomLimit;
        if (v.y > fTopLimit) v.y = fTopLimit;

        if (bSnapMovement)
        {
            fSnapTimer += Time.deltaTime;
            float fDist = (vCamPos - v).magnitude;
            //move if too far from current pos, or too long time since last move
            if (bFirst || (fDist > 10.5f) || (fDist > 5.5f && fSnapTimer > 5.0f))
            {
                bFirst = false;
                fSnapTimer = 0;
                transform.position = v;
                vCamPos = v;
            }
        }
        else
        {
            transform.position = v;
            vCamPos = v;
        }
    }
}
