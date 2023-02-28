using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR;
using UnityEngine.InputSystem;
using Valve.VR;

public class CameraController : MonoBehaviour
{
    public static CameraController instance = null;

    bool bMapMode;
    GameObject oPlayer;
    GameLevel oMap;

    public Transform cameraRig;

    private Vector3 vCamOffset;
    private Vector3 vMapSize;

    internal bool bLayDown = false;
    internal static bool bSnapMovement = false;
    internal static bool bPointMovement = false;
    bool bPointMovementInMenu = false;
    internal static Vector3 vCamPos = new Vector3(0, 0, -4.5f);

    int iRightHanded = 0;
    public GameObject oRayQuad;
    GameObject oGazeQuad = null;
    Material oCursorMaterial;

    internal Vector3 vHeadPosition;
    internal Vector3 vGazeDirection;
    internal Quaternion qRotation;

    SteamVR_Action_Pose poseActionR;
    SteamVR_Action_Pose poseActionL;

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
         //   for (int i=0; i< transform.childCount; i++) //this is ok, since destroy doesn't take it away immediatly
         //       Destroy(transform.GetChild(i).gameObject);
            Destroy(gameObject);
            return;
        }

        //the rest is done once only...
        DontDestroyOnLoad(gameObject);

        iYAdjust = PlayerPrefs.GetInt("MyYAdjustInt", 0);
        iZAdjust = PlayerPrefs.GetInt("MyZAdjustInt", 0);
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

        poseActionR = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose_right_tip");
        poseActionL = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose_left_tip");

        oRayQuad.SetActive(false);
    }

    private int iYAdjust = 0, iZAdjust = 0;
    private float fYAdjustStep = 0.24f;
    private float fZAdjustStep = 0.20f;
    public void CycleYAdjust()
    {
        iYAdjust++;
        if (iYAdjust > 10) iYAdjust = -10; //21 steps

        vCamOffset = new Vector3(0, iYAdjust * fYAdjustStep + 0.3f, iZAdjust * fZAdjustStep - 1.90f);
        if (!bMapMode)
        {
            vCamPos = new Vector3(0, iYAdjust * fYAdjustStep, iZAdjust * fZAdjustStep - 4.3f);
            transform.position = vCamPos;
        }

        PlayerPrefs.SetInt("MyYAdjustInt", iYAdjust);
        PlayerPrefs.Save();
    }
    public void CycleZAdjust()
    {
        iZAdjust++;
        if (iZAdjust > 2) iZAdjust = -4; //7 steps

        vCamOffset = new Vector3(0, iYAdjust * fYAdjustStep + 0.3f, iZAdjust * fZAdjustStep - 1.90f);
        if (!bMapMode)
        {
            vCamPos = new Vector3(0, iYAdjust * fYAdjustStep, iZAdjust * fZAdjustStep - 4.3f);
            transform.position = vCamPos;
        }

        PlayerPrefs.SetInt("MyZAdjustInt", iZAdjust);
        PlayerPrefs.Save();
    }
    public void InitForGame(GameLevel i_oMap, GameObject i_oPlayer)
    {
        bMapMode = true;
        oPlayer = i_oPlayer;
        oMap = i_oMap;
        vCamPos = new Vector3(0, 0, -10.0f); //set it away from the player, transform.position will then be set first Update

        vCamOffset = new Vector3(0, iYAdjust * fYAdjustStep + 0.3f, iZAdjust * fZAdjustStep - 1.90f);
        vMapSize = oMap.GetMapSize();
    }
    public void InitForMenu()
    {
        bMapMode = false;
        vCamPos = new Vector3(0, iYAdjust * fYAdjustStep, iZAdjust * fZAdjustStep - 4.3f);
        transform.position = vCamPos;
    }

    //mouse movement smoothing to distribute movement every frame when framerate
    //is above input rate at the cost of a delay in movement
    // frame movement example
    // before: 5 0 0 5 0 0
    // now: 2 2 1 2 2 1 
    float fCurX = 0, fCurY = 0;
    float fStepX = 0, fStepY = 0;
    public void GetMouseMovementSmooth(out float o_fX, out float o_fY, out float o_fXScreen, out float o_fYScreen)
    {
        o_fX = 0;
        o_fY = 0;
        o_fXScreen = 0;
        o_fYScreen = 0;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            //InputSystem
            Vector2 v = mouse.position.ReadValue();
            o_fXScreen = v.x;
            o_fYScreen = v.y;
            v = mouse.delta.ReadValue() * Time.deltaTime * 7.0f;
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
            Camera.main.fieldOfView = 52.0f;
        }

        //emulate headset movement
        Keyboard keyboard = Keyboard.current;
        if ((keyboard!=null && keyboard.fKey.isPressed) || GameManager.bNoVR)
        {
            //using mouse smoothing to avoid jerkyness
            float fMouseX, fMouseY, fMouseXScreen, fMouseYScreen;
            GetMouseMovementSmooth(out fMouseX, out fMouseY, out fMouseXScreen, out fMouseYScreen);
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.middleButton.isPressed)
            {
                if (keyboard.gKey.isPressed) fZ_cam += fMouseX * 3.0f;
                else fY_cam += fMouseX * 3.0f;
                fX_cam -= fMouseY * 3.0f;
            }
            if (mouse!=null)
            {
                Vector3 scr = new Vector3(fMouseXScreen, fMouseYScreen, 5.0f);
                mousePoint = Camera.main.ScreenToWorldPoint(scr, Camera.MonoOrStereoscopicEye.Mono);
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

    bool bFadeDone = true;
    public void Fade(bool bDone)
    {
        bFadeDone = bDone;
        if (!bDone)
        {
            //begin
            oRayQuad.SetActive(false);
        }
    }

    public void SetMovementMode(bool bMotionController)
    {
        bPointMovement = bMotionController;
    }
    public void SetLayDownView(bool bLayDownView)
    {
        bLayDown = bLayDownView;
        if(bLayDown) transform.Rotate(75.0f, 0, 0);
        else transform.Rotate(-75.0f, 0, 0);
    }

    public void SetPointingInfo(Vector3 vHitPoint, Quaternion qHitDir, Vector3 vOrigin, Quaternion qOriginDir)
    {
        //move the cursor to the point where the raycast hit
        oGazeQuad.transform.position = vHitPoint;
        //rotate the cursor to hug the surface
        oGazeQuad.transform.rotation = qHitDir;

        //ray from origin to the point where the raycast hit
        //direction of ray
        oRayQuad.transform.SetPositionAndRotation((vHitPoint + vOrigin) / 2.0f, qOriginDir);
        oRayQuad.transform.Rotate(new Vector3(90, 0, 0));
        oRayQuad.transform.localScale = new Vector3(0.05f, ((vHitPoint - vOrigin).magnitude - 0.07f)/2.0f, 0.05f);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Gamepad gamepad = Gamepad.current;

        //switch hand/use gamepad?
        {
            try
            {
                if ((SteamVR_Actions.default_Throttle.GetAxis(SteamVR_Input_Sources.RightHand) > 0.5f)
                    || SteamVR_Actions.default_Fire.GetState(SteamVR_Input_Sources.RightHand))
                {
                    bPointMovementInMenu = true;
                    iRightHanded = 1;
                }
                else if ((SteamVR_Actions.default_Throttle.GetAxis(SteamVR_Input_Sources.LeftHand) > 0.5f)
                    || SteamVR_Actions.default_Fire.GetState(SteamVR_Input_Sources.LeftHand))
                {
                    bPointMovementInMenu = true;
                    iRightHanded = 2;
                }

                if ((SteamVR_Actions.default_Throttle.GetAxis(SteamVR_Input_Sources.Gamepad) > 0.5f)
                    || SteamVR_Actions.default_Fire.GetState(SteamVR_Input_Sources.Gamepad))
                {
                    bPointMovementInMenu = false;
                    iRightHanded = 0;
                }
            }
            catch { }

            if (gamepad != null)
            {
                if (gamepad.rightTrigger.ReadValue() > 0.5f || gamepad.buttonSouth.isPressed || gamepad.buttonEast.isPressed)
                {
                    bPointMovementInMenu = false;
                    iRightHanded = 0;
                }
            }
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed || mouse.leftButton.isPressed)
                {
                    bPointMovementInMenu = true;
                    iRightHanded = 0;
                }
            }

            if (bFadeDone)
            {
                oRayQuad.SetActive(bPointMovement || (!bMapMode && bPointMovementInMenu));
                if (oGazeQuad != null) oGazeQuad.SetActive(!bPointMovementInMenu && !bMapMode);
            }
        }

        //update pointing movement
        vHeadPosition = Camera.main.transform.position;
        vGazeDirection = Camera.main.transform.forward;
        qRotation = Camera.main.transform.rotation;
        if (iRightHanded == 1)
        {
            vHeadPosition = cameraRig.position + poseActionR[SteamVR_Input_Sources.RightHand].localPosition;
            qRotation = cameraRig.rotation * poseActionR[SteamVR_Input_Sources.RightHand].localRotation;
            vGazeDirection = qRotation * Vector3.forward;
        }
        if (iRightHanded == 2)
        {
            vHeadPosition = cameraRig.position + poseActionL[SteamVR_Input_Sources.LeftHand].localPosition;
            qRotation = cameraRig.rotation * poseActionL[SteamVR_Input_Sources.LeftHand].localRotation;
            vGazeDirection = qRotation * Vector3.forward;
        }
        if (iRightHanded == 0 && bPointMovementInMenu) //on mouse only (gamepad uses head to point in menu, and cannot point in game)
        {
            vHeadPosition = new Vector3(vHeadPosition.x, vHeadPosition.y-0.5f, vHeadPosition.z); //below head
            Ray r = new Ray(vHeadPosition, (mousePoint - vHeadPosition).normalized);
            qRotation = Quaternion.LookRotation(r.direction);
            vGazeDirection = qRotation * Vector3.forward;
        }
    }
}
