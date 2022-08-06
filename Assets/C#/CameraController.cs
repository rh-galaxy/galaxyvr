using System.Collections;
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
            for (int i=0; i< transform.childCount; i++) //this is ok, since destroy doesn't take it away immediatly
                Destroy(transform.GetChild(i).gameObject);
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
    public void GetMouseMovementSmooth(out float o_fX, out float o_fY)
    {
        o_fX = 0;
        o_fY = 0;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            Vector2 v = mouse.delta.ReadValue() * Time.deltaTime * 7.0f;
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
    float fX = 0.0f, fY = 0, fZ = 0;
    float fSnapTimer = 0;
    bool bFirst = true;
    void LateUpdate()
    {
        //emulate headset movement
        Keyboard keyboard = Keyboard.current;
        if ((keyboard!=null && keyboard.fKey.isPressed) || GameManager.bNoVR)
        {
            //using mouse smoothing to avoid jerkyness
            float fMouseX, fMouseY;
            GetMouseMovementSmooth(out fMouseX, out fMouseY);
            if (keyboard.gKey.isPressed) fZ += fMouseX * 3.0f;
            else fY += fMouseX * 3.0f;
            fX -= fMouseY * 3.0f;

            if (keyboard != null && keyboard.rKey.isPressed) { fX = 0.0f; fY = 0; fZ = 0; }

            transform.eulerAngles = new Vector3(fX, fY, fZ);
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
        UnityEngine.XR.InputDevice handRDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        UnityEngine.XR.InputDevice handLDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        //switch hand/use gamepad?
        {
            bool triggerRSupported = handRDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerR);
            bool button1RSupported = handRDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool button1R);
            bool button2RSupported = handRDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool button2R);
            bool triggerLSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerL);
            bool button1LSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool button1L);
            bool button2LSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool button2L);
            if (triggerR > 0.5f || button1R || button2R)
            {
                bPointMovementInMenu = true;
                iRightHanded = 1;
            }
            else if (triggerL > 0.5f || button1L || button2L)
            {
                bPointMovementInMenu = true;
                iRightHanded = 2;
            }

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
                    bPointMovementInMenu = false;
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
            bool posRSupported = handRDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 posR);
            vHeadPosition = transform.TransformPoint(posR); //to world coords
            bool rotRSupported = handRDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotR);
            vGazeDirection = rotR * Vector3.forward;
            vGazeDirection = transform.TransformDirection(vGazeDirection);
            qRotation = Quaternion.LookRotation(vGazeDirection);
        }
        if (iRightHanded == 2)
        {
            bool posLSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 posL);
            vHeadPosition = transform.TransformPoint(posL); //to world coords
            bool rotLSupported = handLDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotL);
            vGazeDirection = rotL * Vector3.forward;
            vGazeDirection = transform.TransformDirection(vGazeDirection);
            qRotation = Quaternion.LookRotation(vGazeDirection);
        }
    }
}
