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

    float fCurX = 0, fCurY = 0;
    float fStepX = 0, fStepY = 0;
    public void GetMouseMovementSmooth(out float o_fX, out float o_fY)
    {
        fCurX += Input.GetAxis("Mouse X");
        fCurY += Input.GetAxis("Mouse Y");

        float fStep = Time.deltaTime / 0.050f; //% of movement to distribute each time called
        if (fStep > 1.0f) fStep = 1.0f;  //if frametime too long we must not move faster
        if (fStep <= 0.0f) fStep = 1.0f; //if the timer has to low resolution compared to the framerate

        fStepX = fCurX * fStep;
        fStepY = fCurY * fStep;

        if (Mathf.Abs(fCurX) > Mathf.Abs(fStepX))
        {
            fCurX -= fStepX;
            o_fX = fStepX;
        }
        else
        {
            o_fX = fCurX;
            fCurX = 0;
        }

        if (Mathf.Abs(fCurY) > Mathf.Abs(fStepY))
        {
            fCurY -= fStepY;
            o_fY = fStepY;
        }
        else
        {
            o_fY = fCurY;
            fCurY = 0;
        }
    }

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
            //using mouse smoothing to avoid jerkyness
            float fMouseX, fMouseY;
            GetMouseMovementSmooth(out fMouseX, out fMouseY);
            if (Input.GetKey(KeyCode.G)) fZ += fMouseX * 3.0f;
            else fY += fMouseX * 3.0f;
            fX -= fMouseY * 3.0f;

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
