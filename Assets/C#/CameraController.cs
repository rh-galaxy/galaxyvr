﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController instance = null;

    bool bMapMode;
    GameObject oPlayer;
    GameLevel oMap;

    private Vector3 vCamOffset;
    private Vector3 vMapSize;

    internal static bool bSnapMovement = false;
    internal static Vector3 vCamPos = new Vector3(0, 0, -4.5f);

    public GameObject oRayQuad;
    GameObject oGazeQuad = null;
    Material oCursorMaterial;

    internal Vector3 vHeadPosition;
    internal Vector3 vGazeDirection;
    internal Quaternion qRotation;

    private void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            //enforce singleton pattern, meaning there can only ever be one instance of a CameraController.
            Destroy(gameObject);
            return;
        }

        //the rest is done once only...
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //create a quad a circle for gazeing in VR or mouse cursor in non VR
        oGazeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        oGazeQuad.transform.parent = transform;
        MonoBehaviour.DestroyImmediate(oGazeQuad.GetComponent<Collider>());
        oCursorMaterial = Resources.Load("Cursor", typeof(Material)) as Material;
        oGazeQuad.GetComponent<MeshRenderer>().material = oCursorMaterial;
        oGazeQuad.transform.localScale = new Vector3(.38f, .38f, 1);
        oGazeQuad.SetActive(false);

        oRayQuad.SetActive(false);
    }

    public void InitForGame(GameLevel i_oMap, GameObject i_oPlayer)
    {
        bMapMode = true;
        oPlayer = i_oPlayer;
        oMap = i_oMap;
        vCamPos = new Vector3(0, 0, -10.0f); //set it away from the player, transform.position will then be set first Update

        vCamOffset = new Vector3(0, 0.3f, -1.90f);
        vMapSize = oMap.GetMapSize();
    }
    public void InitForMenu()
    {
        bMapMode = false;
        vCamPos = new Vector3(0, 0, -4.3f);
        transform.position = vCamPos;
    }

    //mouse movement smoothing to distribute movement every frame when framerate
    //is above input rate at the cost of a delay in movement
    // frame movement example
    // before: 5 0 0 5 0 0
    // now: 2 2 1 2 2 1 
    float fCurX = 0, fCurY = 0;
    float fStepX = 0, fStepY = 0;
    public void GetMouseMovementSmooth(out float o_fDeltaX, out float o_fDeltaY, out float o_fScreenX, out float o_fScreenY)
    {
        o_fDeltaX = 0;
        o_fDeltaY = 0;
        o_fScreenX = 0;
        o_fScreenY = 0;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            //InputSystem
            Vector2 v = mouse.position.ReadValue();
            o_fScreenX = v.x;
            o_fScreenY = v.y;
            v = mouse.delta.ReadValue() * 0.09f; //should not apply Time.deltaTime here since a shorter or longer deltaTime already modifies the mouse delta
            fCurX += v.x;
            fCurY += v.y;

            float fStep = Time.deltaTime / 0.050f; //% of movement to distribute each time called
            if (fStep > 1.0f) fStep = 1.0f;  //if frametime too long we must not move faster
            if (fStep <= 0.0f) fStep = 1.0f; //if the timer has too low resolution compared to the framerate

            fStepX = fCurX * fStep;
            fStepY = fCurY * fStep;

            if (Mathf.Abs(fCurX) > Mathf.Abs(fStepX))
            {
                fCurX -= fStepX;
                o_fDeltaX = fStepX;
            }
            else
            {
                o_fDeltaX = fCurX;
                fCurX = 0;
            }

            if (Mathf.Abs(fCurY) > Mathf.Abs(fStepY))
            {
                fCurY -= fStepY;
                o_fDeltaY = fStepY;
            }
            else
            {
                o_fDeltaY = fCurY;
                fCurY = 0;
            }
        }
        else
        {
            //reset so old values does nothing when mouse pressed again
            fCurX = 0;
            fCurY = 0;
        }
    }

    // Update is called once per frame
    float fX_cam = 0.0f, fY_cam = 0, fZ_cam = 0;
    Vector3 mousePoint;
    float fSnapTimer = 0;
    bool bFirst = true;
    void LateUpdate()
    {
        /**/
        if (GameManager.bNoVR)
        {
            Camera.main.stereoTargetEye = StereoTargetEyeMask.None;
            Camera.main.fieldOfView = 55.0f;
        }

        //emulate headset movement
        Keyboard keyboard = Keyboard.current;
        if (GameManager.bNoVR)
        {
            //using mouse smoothing to avoid jerkyness
            float fMouseDeltaX, fMouseDeltaY, fMouseScreenX, fMouseScreenY;
            GetMouseMovementSmooth(out fMouseDeltaX, out fMouseDeltaY, out fMouseScreenX, out fMouseScreenY);
            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.middleButton.isPressed || mouse.leftButton.isPressed))
            {
                float ratio = (float)Screen.width / 1920.0f;
                float multiply = 2.0f / ratio;

                if (keyboard.gKey.isPressed) fZ_cam += fMouseDeltaX * multiply;
                else fY_cam += fMouseDeltaX * multiply;
                fX_cam -= fMouseDeltaY * multiply;
            }

            if (keyboard != null && keyboard.rKey.isPressed) { fX_cam = 0.0f; fY_cam = 0; fZ_cam = 0; }

            transform.eulerAngles = new Vector3(fX_cam, fY_cam, fZ_cam);
        }

        if (bMapMode)
        {
            Vector3 v = oPlayer.transform.position;
            float fLeftLimit = -(vMapSize.x / 20.0f) + 0.5f;
            float fRightLimit = (vMapSize.x / 20.0f) - 0.5f;
            if (v.x < fLeftLimit) v.x = fLeftLimit;
            if (v.x > fRightLimit) v.x = fRightLimit;
            float fTopLimit = (vMapSize.y / 20.0f) - 0.3f;
            float fBottomLimit = -(vMapSize.y / 20.0f) + 0.5f;
            if (v.y < fBottomLimit) v.y = fBottomLimit;
            if (v.y > fTopLimit) v.y = fTopLimit;
            v += vCamOffset; //add this after limiting

            if (bSnapMovement)
            {
                fSnapTimer += Time.deltaTime;
                float fDist = (vCamPos - v).magnitude;
                //move if too far from current pos, or too long time since last move
                if (bFirst || (fDist > 0.80f) || (fDist > 0.43f && fSnapTimer > 4.4f))
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

    private void Update()
    {
        //update pointing movement
        vHeadPosition = Camera.main.transform.position;
        vGazeDirection = Camera.main.transform.forward;
        qRotation = Camera.main.transform.rotation;
    }
}
