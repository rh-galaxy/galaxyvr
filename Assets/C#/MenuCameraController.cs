using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    //mouse movement smoothing to distribute movement every frame when framerate
    //is above input rate at the cost of a delay in movement
    // frame movement example
    // before: 5 0 0 5 0 0
    // now: 2 2 1 2 2 1 
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

    void Start()
    {

    }

    float fX = 0, fY = 0, fZ = 0;
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
    }
}