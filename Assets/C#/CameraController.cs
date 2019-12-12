using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool bMapMode;
    public GameObject oPlayer;
    public GameLevel oMap;

    private Vector3 vCamOffset;
    private Vector3 vMapSize;

    internal static bool bSnapMovement = false;
    internal static bool bPointMovement = true;
    internal static Vector3 vCamPos = new Vector3(0, 0, -4.5f);

    int iRightHanded;
    public GameObject oRayQuad;
    GameObject oGazeQuad = null;
    Material oCursorMaterial;

    internal Vector3 vHeadPosition;
    internal Vector3 vGazeDirection;
    internal Quaternion qRotation;


    public void InitForGame(GameLevel i_oMap, GameObject i_oPlayer)
    {
        bMapMode = true;
        oPlayer = i_oPlayer;
        oMap = i_oMap;
        vCamPos = new Vector3(0, 0, -10.0f); //set it away from the player, transform.position will then be set first Update.

        vCamOffset = new Vector3(0, 0.3f, -0.70f);
        vMapSize = oMap.GetMapSize();

        if (oGazeQuad != null) oGazeQuad.SetActive(false);
    }
    public void InitForMenu()
    {
        bMapMode = false;
        vCamPos = new Vector3(0, 0, -4.3f);
        transform.position = vCamPos;

        if (oGazeQuad != null) oGazeQuad.SetActive(!bPointMovement);
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

    private void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
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
        oGazeQuad.SetActive(!bPointMovement);

        iRightHanded = -1;
    }

    // Update is called once per frame
    float fX = 5.0f, fY = 0, fZ = 0;
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

            if (Input.GetKey(KeyCode.R)) { fX = 5.0f; fY = 0; fZ = 0; }

            transform.eulerAngles = new Vector3(fX, fY, fZ);
        }

        if(bMapMode)
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
        else
        {
            //end
            oRayQuad.SetActive(bPointMovement);
        }
    }

    public void SetMovementMode(bool bMotionController)
    {
        bPointMovement = bMotionController;
        GameManager.theGM.myAvatar.gameObject.transform.Find("controller_right").gameObject.SetActive(bPointMovement);
        GameManager.theGM.myAvatar.gameObject.transform.Find("controller_left").gameObject.SetActive(false);
        oRayQuad.SetActive(bPointMovement);
        oGazeQuad.SetActive(!bPointMovement && !bMapMode);
        iRightHanded = bPointMovement ? 1 : 0;
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
        oRayQuad.transform.localScale = new Vector3(0.05f, ((vHitPoint - vOrigin).magnitude - 0.14f)/2.0f, 0.05f);
    }

    private void Update()
    {
        //handle touch controller
        //first init
        if (iRightHanded == -1 && bFadeDone)
        {
            Transform R = GameManager.theGM.myAvatar.gameObject.transform.Find("controller_right");
            Transform L = GameManager.theGM.myAvatar.gameObject.transform.Find("controller_left");
            if (R != null && L != null)
            {
                //init as right handed
                R.gameObject.SetActive(bPointMovement);
                L.gameObject.SetActive(false);
                iRightHanded = bPointMovement ? 1 : 0;

                oRayQuad.SetActive(bPointMovement);
                oGazeQuad.SetActive(!bPointMovement && !bMapMode);
            }
        }
        //switch hand?
        if (bPointMovement && bFadeDone)
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5f && iRightHanded != 1)
            {
                GameManager.theGM.myAvatar.gameObject.transform.Find("controller_right").gameObject.SetActive(true);
                GameManager.theGM.myAvatar.gameObject.transform.Find("controller_left").gameObject.SetActive(false);
                iRightHanded = 1;
            }
            else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.5f && iRightHanded != 2)
            {
                GameManager.theGM.myAvatar.gameObject.transform.Find("controller_right").gameObject.SetActive(false);
                GameManager.theGM.myAvatar.gameObject.transform.Find("controller_left").gameObject.SetActive(true);
                iRightHanded = 2;
            }
        }

        //update pointing movement
        vHeadPosition = Camera.main.transform.position;
        vGazeDirection = Camera.main.transform.forward;
        qRotation = Camera.main.transform.rotation;
        if (iRightHanded == 1)
        {
            vHeadPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            /**/
            vHeadPosition = vHeadPosition + /**/(vCamPos - new Vector3(0, 0, 1.2f)); //to world coords
            qRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            vGazeDirection = qRotation * Vector3.forward;
        }
        if (iRightHanded == 2)
        {
            vHeadPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            /**/
            vHeadPosition = vHeadPosition + /**/(vCamPos - new Vector3(0, 0, 1.2f)); //to world coords
            qRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            vGazeDirection = qRotation * Vector3.forward;
        }
    }
}
