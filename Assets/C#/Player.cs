using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameLevel oMap;
    Vector3 vMapSize;

    public GameObject oShip;
    public ParticleSystem oThruster;
    ParticleSystem.EmissionModule oThrusterEmission;
    public ParticleSystem oExplosionParticle;
    public ParticleSystem oWallsColl;
    ParticleSystem.EmissionModule oWallsCollEmission;
    public GameObject oShipBody;
    public Material oShipMaterial;
    public GameStatus status;

    public AudioClip oClipExplosion;
    public AudioClip oClipHit;
    public AudioClip oClipFire;
    public AudioClip oClipCP;
    public AudioClip oClipCPEnd;
    public AudioClip oClipLand;
    public AudioClip oClipLoadCargo;
    public AudioClip oClipUnloadCargo;

    AudioSource oASEngine;
    bool bEngineFadeOut = false;
    AudioSource oASScrape;
    bool bScrapeFadeOut = false;
    AudioSource oASGeneral;
    AudioStateMachine asm;

    PolygonCollider2D oPolygonCollider2D;

    //movement
    Rigidbody2D oRb;
    ConstantForce2D oCustomGravity;
    Vector2 vStartPos;
    float fAcceleration = 0.0f;
    float fDirection = 90.0f;
    internal const float fGravityScale = 0.045f;

    //ship properties
    const float MAXFUELINTANK = 60.0f;
    const float MAXSPACEINHOLDWEIGHT = 50.0f;
    internal const int MAXSPACEINHOLDUNITS = 3;
    internal float FULL_HEALTH = 1.5f;
    const float SHIP_MASS = 4.8f;
    const float SHIP_STEERSPEED = 235.0f; //degree/second
    const float SHIP_THRUST = 1.40f;
    const int NUM_LIFES_MISSION = 5;

    //cargo
    bool bCargoSwingingMode = false;
    internal int iCargoNumUsed = 0;
    internal int iCargoSpaceUsed = 0;
    internal int[] aHold = new int[MAXSPACEINHOLDUNITS];
    internal int[] aHoldZoneId = new int[MAXSPACEINHOLDUNITS];
    internal float[] aHoldHealth = new float[MAXSPACEINHOLDUNITS]; //begins at 1.0 (100%) when cargo loaded, goes down equally on all positions on damage

    //score / time
    int iCurCP = 0;
    int iCurLap = 0;
    float fBestLapTime = 10000000; //init to a high value
    bool bTimeCounting = false;
    float fLastNewLapTime = 0.0f;
    internal float fTotalTime = 0.0f;
    int iTotalLaps = -1;
    internal int iScore = 0;
    float fTotalTimeMission = 0.0f; //always counting
    //achievements
    internal float fAchieveFuelBurnt = 0;
    internal float fAchieveDistance = 0;
    internal int iAchieveShipsDestroyed = 0;
    internal int iAchieveBulletsFired = 0;
    internal bool bAchieveNoDamage = true;
    internal bool bAchieveFullThrottle = true;
    internal bool bAchieveFinishedRaceLevel = false;

    //ship status
    internal int iNumLifes = -1; //no limit
    internal float fShipHealth;
    float fLastShipHealth;

    float fFuel;

    bool bLanded = false;
    float fLandTime = 0.0f;
    int iZoneId = -1;

    bool bAlive = true;
    bool bFreeFromBullets = false;
    float fFreeFromBulletsTimer = 0.0f;
    float fDamageTimer = 1.0f;
    float fExplosionTimer = 0.0f;

    bool bInited = false;
    private void Awake()
    {
        oThrusterEmission = oThruster.emission;
        oThrusterEmission.enabled = false;
        oWallsCollEmission = oWallsColl.emission;
        oWallsCollEmission.enabled = false;
        asm = GameObject.Find("AudioStateMachineDND").GetComponent<AudioStateMachine>();

        FULL_HEALTH = GameLevel.theReplay.bEasyMode ? 7.5f : 1.5f;
        bCargoSwingingMode = GameLevel.theReplay.bCargoSwingingMode;

        fShipHealth = FULL_HEALTH;
        fLastShipHealth = FULL_HEALTH;
    }

    public void Init(string i_szName, int i_iPlayerID, Vector2 i_vStartPos, GameLevel i_oMap)
    {
        bInited = true;

        oMap = i_oMap;
        vMapSize = oMap.GetMapSize();
        vStartPos = i_vStartPos;
        fLandTime = 0.0f;

        iTotalLaps = oMap.iRaceLaps;
        fFuel = MAXFUELINTANK;

        oShipBodyMR = oShipBody.GetComponent<MeshRenderer>();

        foreach (AudioSource aSource in GetComponents<AudioSource>())
        { //we have 3 audiosources
            if (aSource.clip != null && aSource.clip.name.Equals("engine"))
                oASEngine = aSource;
            else if (aSource.clip != null && aSource.clip.name.Equals("scratch"))
                oASScrape = aSource;
            else
                oASGeneral = aSource;
        }

        oPolygonCollider2D = GetComponent<PolygonCollider2D>();
        oCustomGravity = GetComponent<ConstantForce2D>();
        oRb = GetComponent<Rigidbody2D>();
        oRb.rotation = fDirection; //happens after next FixedUpdate
        //therefore we need to set the transform immediately so that it
        // is in the startposition pointing correctly after init
        //all objects must be handled like this
        oRb.transform.eulerAngles = new Vector3(0, 0, fDirection);
        oRb.transform.position = vStartPos;
        oRb.position = vStartPos;

        //Physics2D gravity is set to 0,0 because the ship is the only object affected
        // by gravity, so we set a constant force here instead of having it global
        oRb.drag = oMap.fDrag * 0.85f;
        oRb.mass = SHIP_MASS;
        oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;

        //cannot use because it also becomes the piviot for rotation...
        //make it higher up to make ship tilt faster on landingzone
        //m_oRb.centerOfMass = new Vector2(0.5f, 0.0f); //ship points right at rotation 0

        //init status
        if (oMap.iLevelType == (int)LevelType.MAP_RACE)
        {
            status.Init(true);
        }
        else
        {
            iNumLifes = NUM_LIFES_MISSION; //limit num lifes to begin with
            status.Init(false);

            bAchieveFullThrottle = false;
        }

        vLastPosition = vStartPos;

        for (int i = 0; i < fMeanSpeeds.Length; i++)
        {
            fMeanSpeeds[i] = 0.0f;
        }
    }

    bool bFireTriggered = false, bFire = false;
    bool bThrottle, bLeft, bRight, bAdjust;

    Color oShipColorDamage = new Color(215 / 255.0f, 0, 0, 1.0f);
    Color oShipColorNormal = new Color(138 / 255.0f, 0, 0, 1.0f);
    Color oShipColorBlink = new Color(180 / 255.0f, 0, 0, 0.55f);
    MeshRenderer oShipBodyMR;
    void Update()
    {
        if (!bInited)
            return;

        //update ship color when blinking
        if (bFreeFromBullets && (((int)(fFreeFromBulletsTimer * 1000)) % 300 > 200))
            oShipMaterial.color = oShipColorBlink;
        else if (fDamageTimer < 0.21f) oShipMaterial.color = oShipColorDamage;
        else oShipMaterial.color = oShipColorNormal;
        oShipBodyMR.material = oShipMaterial;

        //update status gui
        if (oMap.iLevelType == (int)LevelType.MAP_RACE)
        {
            status.SetForRace(fShipHealth / FULL_HEALTH, fTotalTime, "[" + iCurLap.ToString() + "/" + iTotalLaps.ToString() + "] " + iCurCP.ToString());
        }
        else
        {
            float fCargoHealthValue = -1f;
            if (bCargoSwingingMode)
            {
                fCargoHealthValue = 0f;
                if (iCargoSpaceUsed > 0)
                {
                    for (int i = 0; i < iCargoNumUsed; i++)
                    {
                        fCargoHealthValue += aHold[i] * aHoldHealth[i];
                    }
                    fCargoHealthValue = fCargoHealthValue / iCargoSpaceUsed;
                }
            }
            status.SetForMission(fShipHealth / FULL_HEALTH, iNumLifes, iCargoSpaceUsed / MAXSPACEINHOLDWEIGHT,
                iCargoNumUsed == 3 || iCargoSpaceUsed == MAXSPACEINHOLDWEIGHT, fFuel / MAXFUELINTANK, GetScore() / 1000, fCargoHealthValue);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        //must exist one impact point

        int iNum = collision.contactCount;
        ContactPoint2D c = collision.GetContact(0);
        float fImpulse = c.normalImpulse * 100.0f;
        float fTangentImpulse = c.tangentImpulse * 100.0f; //sliding impact?
        float fSpeedX = c.relativeVelocity.x * 10;
        float fSpeedY = c.relativeVelocity.y * 10;
        //then it might be a second impact point (that's all we take into concideration
        // any more and the ship is in deep trouble)
        if (iNum > 1)
        {
            c = collision.GetContact(1);
            fImpulse += c.normalImpulse * 100.0f;
            fTangentImpulse += c.tangentImpulse * 100.0f; //sliding impact?
            fSpeedX += c.relativeVelocity.x * 10;
            fSpeedY += c.relativeVelocity.y * 10;
            fSpeedX /= 2;
            fSpeedY /= 2;
        }
        //we ignore fTangentImpulse for now

        if (szOtherObject.StartsWith("LandingZone"))
        {
            fSpeedX = Mathf.Abs(fSpeedX);
            fSpeedY = Mathf.Abs(fSpeedY);

            //get a value where 0 is no diff and 180 is max diff from 90 (pointing up)
            float fGoalAngle = 90.0f;
            float fDiff = fGoalAngle - fDirection;
            if (fDiff < 0) fDiff += 360;
            if (fDiff > 360) fDiff -= 360;
            if (fDiff > 180) fDiff = 360 - fDiff; //the difference of two angles can be max 180.

            //minimum impulse to damage
            fImpulse -= 24.0f;
            if (fImpulse <= 0) fImpulse = 0.0f;

            //too high velocity in x leads to extra damage
            if (fSpeedX > 1.8f)
                fShipHealth -= (fSpeedX - 1.8f) * (fSpeedX - 1.8f) * 0.010f;
            //too much tilted
            if (fDiff > 15 && fImpulse > 0.0f)
                fShipHealth -= (fImpulse / 80.0f) * (fDiff / 180.0f) * 0.5f;
            //too much speed
            if (fSpeedY > 2.5f)
                fShipHealth -= (fImpulse / 80.0f) * 0.08f;
            //unrealistic with bouncing when tilted/half way outside zone
            //so not done

            if (fShipHealth < fLastShipHealth)
            {
                c = collision.GetContact(0);
                oWallsColl.transform.position = new Vector3(c.point.x, c.point.y, .15f);
                oWallsCollEmission.enabled = true;
            }

            oASGeneral.PlayOneShot(oClipLand, 0.8f);
        }

        //map or door, or map decorations
        if (szOtherObject.CompareTo("Map") == 0 || szOtherObject.StartsWith("Slider") ||
            szOtherObject.CompareTo("Balk") == 0 || szOtherObject.StartsWith("Knapp") ||
            szOtherObject.CompareTo("Barrels") == 0 || szOtherObject.StartsWith("Tree") ||
            szOtherObject.StartsWith("House") || szOtherObject.CompareTo("RadioTower") == 0 ||
            szOtherObject.StartsWith("Enemy"))
        {
            if (szOtherObject.StartsWith("Enemy4") || szOtherObject.StartsWith("Enemy5"))
            {
                fShipHealth -= 8.0f; //instant kill
            }
            else
            {
                //minimum impulse to damage (0 - always damage on map)
                fImpulse -= 0.0f;
                if (fImpulse <= 0) fImpulse = 0.0f;

                fShipHealth -= (fImpulse / 80.0f) * 0.5f;

                bScrapeFadeOut = false;
                oASScrape.volume = 1.0f;
                oASScrape.Play();

                c = collision.GetContact(0);
                oWallsColl.transform.position = new Vector3(c.point.x, c.point.y, .15f);
                oWallsCollEmission.enabled = true;
            }
        }

        //collide with enemy body
        //if (szOtherObject.StartsWith("Enemy"))
        //{
        //}

        //enemy bullet, take damage
        if (szOtherObject.StartsWith("BulletE"))
        {
            if (!bFreeFromBullets)
            {
                fShipHealth -= 0.47f; //4 hits if full health
                //play hit sound
                oASGeneral.PlayOneShot(oClipHit);
            }
        }

        //own bullet, do nothing
        if (szOtherObject.StartsWith("BulletP"))
        {
        }
    }

    //OBS this was using Time.fixedDeltaTime but that is wrong for this method since it runs at framerate (90, 80 or even 144), while fixed update is 100Hz.
    //But no big harm done for replays since we have a KILL message for when that happens.
    //Different models of headsets would have had slightly different time to do damage (and KILL).
    //Now fixed, and it will work with current replays
    // fLandTime is problematic, but for framerates recorded at <100 this will probably work
    private void OnCollisionStay2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;
        int iNum = collision.contactCount;
        ContactPoint2D c = collision.GetContact(0);

        if (szOtherObject.StartsWith("LandingZone"))
        {
            //get a value where 0 is no diff and 180 is max diff from 90 (pointing up)
            float fGoalAngle = 90.0f;
            float fDiff = fGoalAngle - fDirection;
            if (fDiff < 0) fDiff += 360;
            if (fDiff > 360) fDiff -= 360;
            if (fDiff > 180) fDiff = 360 - fDiff; //the difference of two angles can be max 180.

            //too much tilted
            if (fDiff > 15)
                fShipHealth -= 0.1f * Time.deltaTime;
            if (fDiff > 80)
                fShipHealth -= 0.5f * Time.deltaTime;

            //damage taken?
            oWallsColl.transform.position = new Vector3(c.point.x, c.point.y, .0f);
            oWallsCollEmission.enabled = !(fShipHealth >= fLastShipHealth);

            //landing stable
            if (fDiff < 3.0f)
            {
                fLandTime += Time.deltaTime;
                if (fLandTime > 0.3f && !bLanded)
                {
                    bLanded = true;
                    iZoneId = int.Parse(szOtherObject.Substring(11));
                }
            }
        }

        //map or door, or map decorations
        if (szOtherObject.CompareTo("Map") == 0 || szOtherObject.StartsWith("Slider") ||
            szOtherObject.CompareTo("Balk") == 0 || szOtherObject.StartsWith("Knapp") ||
            szOtherObject.CompareTo("Barrels") == 0 || szOtherObject.StartsWith("Tree") ||
            szOtherObject.StartsWith("House") || szOtherObject.CompareTo("RadioTower") == 0 ||
            szOtherObject.StartsWith("Enemy"))
        {
            fShipHealth -= 0.5f * Time.deltaTime;

            //damage taken?
            oWallsColl.transform.position = new Vector3(c.point.x, c.point.y, .0f);
            oWallsCollEmission.enabled = !(fShipHealth >= fLastShipHealth);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        if (szOtherObject.StartsWith("LandingZone"))
        {
            fLandTime = 0.0f;
            bLanded = false;
            oWallsCollEmission.enabled = false;
        }

        //map or door, or map decorations
        if (szOtherObject.CompareTo("Map") == 0 || szOtherObject.StartsWith("Slider") ||
            szOtherObject.CompareTo("Balk") == 0 || szOtherObject.StartsWith("Knapp") ||
            szOtherObject.CompareTo("Barrels") == 0 || szOtherObject.StartsWith("Tree") ||
            szOtherObject.StartsWith("House") || szOtherObject.CompareTo("RadioTower") == 0 ||
            szOtherObject.StartsWith("Enemy"))
        {
            bScrapeFadeOut = true;
            oWallsCollEmission.enabled = false;
        }
    }

    public Vector2 GetPosition()
    {
        return oRb.position;
    }

    Vector2 vForceDir = new Vector2(0, 0);
    int iLastInput = 0; //bool bitfield
    float fReplayMessageTimer = 0;
    float[] fMeanSpeeds = new float[16];
    internal float fMeanSpeed = 0.0f;
    internal int iNumEnemiesNear = 0;
    internal int iNumBulletsNear = 0;
    float fCurrentSpeedSeg = 0;

    Vector2 vLastPosition;
    Vector2 vLastVel;
    float fLastDirection;

    void FixedUpdate()
    {
        if (!bInited)
            return;

        float fDist = (oRb.position - vLastPosition).magnitude;
        if (fDist > 0.80f) fDist = 0.0f; //detect when player has jumped to a new position (after death)

        //this is a safety for if the ship is thrown outside the map area by first getting stuck then getting loose
        if (oRb.position.x < -vMapSize.x / 20.0f || oRb.position.x > vMapSize.x / 20.0f
            || oRb.position.y < -vMapSize.y / 20.0f || oRb.position.y > vMapSize.y / 20.0f)
        {
            fShipHealth = 0.0f;
        }
        //this is for sudden velocity increase when getting loose
        if (oRb.velocity.magnitude - vLastVel.magnitude > 0.10f)
        {
            oRb.velocity = vLastVel;
        }
        //^may not be needed, v1.85 fixed collision issues

        //mean speed calculation (used in race music)
        int iLastSec = (int)fCurrentSpeedSeg;
        fCurrentSpeedSeg += Time.fixedDeltaTime;
        if (fCurrentSpeedSeg >= 16.0f) fCurrentSpeedSeg -= 16.0f;
        int iCurSec = (int)fCurrentSpeedSeg;
        if (iCurSec != iLastSec)
            fMeanSpeeds[iCurSec] = 0.0f;
        fMeanSpeeds[iCurSec] += fDist;
        fMeanSpeed = 0.0f;
        for (int i = 0; i < fMeanSpeeds.Length; i++)
        {
            if (i != iCurSec) fMeanSpeed += fMeanSpeeds[i];
        }
        fMeanSpeed = fMeanSpeed / (fMeanSpeeds.Length - 1) * 10;

        //enemies near (used in mission music)
        iNumEnemiesNear = oMap.GetNumEnemiesNearPlayer();
        //or enemy bullets near (used in mission music)
        iNumBulletsNear = oMap.GetNumBulletsNearPlayer();

        //distance achievement
        fAchieveDistance += fDist * 10;

        fLastDirection = oRb.rotation;
        vLastPosition = oRb.position;
        vLastVel = oRb.velocity;
        fTotalTimeMission += Time.fixedDeltaTime;
        if (bTimeCounting) fTotalTime += Time.fixedDeltaTime;

        //////get input, either from replay or from human player
        if (GameLevel.bRunReplay)
        {
            ReplayMessage rm;
            while (GameLevel.theReplay.Get(out rm, 0))
            {
                //we got new input
                if (rm.iType == (byte)MsgType.MOVEMENT)
                {
                    oRb.position = rm.vPos;
                    oRb.velocity = rm.vVel;
                    oRb.rotation = rm.fDirection;
                    bThrottle = (rm.iKeyFlag & 8) != 0;
                    bLeft = (rm.iKeyFlag & 4) != 0;
                    bRight = (rm.iKeyFlag & 2) != 0;
                    bAdjust = (rm.iKeyFlag & 1) != 0;
                }
                if (rm.iType == (byte)MsgType.BULLETP_NEW)
                {
                    //part of CreateBullet()
                    S_BulletInfo stBulletInfo;
                    stBulletInfo.vPos = rm.vPos;
                    stBulletInfo.vVel = rm.vVel;
                    stBulletInfo.fDirection = rm.fDirection;
                    GameObject o = Instantiate(oMap.oBulletObjBase, oMap.transform);
                    o.GetComponent<Bullet>().Init(stBulletInfo, 0/*iOwnerID*/);

                    oASGeneral.PlayOneShot(oClipFire, 0.8f);
                }
                if (rm.iType == (byte)MsgType.PLAYER_KILL)
                {
                    oRb.position = rm.vPos;
                    oRb.velocity = rm.vVel;
                    oRb.rotation = rm.fDirection;
                    Kill(false);
                }
            }
        }
        else
        {
            bool bNewFireState = false;
            bThrottle = bLeft = bRight = bAdjust = false;

            //get input from joystick
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stickG1 = gamepad.rightStick.ReadValue();
                Vector2 stickG2 = gamepad.leftStick.ReadValue();
                float trgG1 = gamepad.rightTrigger.ReadValue();
                float trgG2 = gamepad.leftTrigger.ReadValue();

                if (stickG1.x > 0.5f || stickG2.x > 0.5f) bRight = true;
                if (stickG1.x < -0.5f || stickG2.x < -0.5f) bLeft = true;
                if ((stickG1.y < -0.7f || stickG2.y < -0.7f) && bLeft == false && bRight == false) bAdjust = true;
                if (stickG1.y < -0.85f || stickG2.y < -0.85f) bAdjust = true; //safety if all the way down, don't care if left/right

                if (trgG1 > 0.3f || trgG2 > 0.3f) bThrottle = true;
                if (gamepad.buttonEast.isPressed || gamepad.buttonNorth.isPressed) bThrottle = true; //button B (Y)
                if (gamepad.buttonSouth.isPressed || gamepad.buttonWest.isPressed) bNewFireState = true; //button A (X)
            }

            //keyboard
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null) {
                if (keyboard.enterKey.isPressed || keyboard.spaceKey.isPressed) bNewFireState = true;
                if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) bThrottle = true;
                if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) bAdjust = true;
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) { bLeft = true; bAdjust = false; }
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) { bRight = true; bAdjust = false; }
            }

            if (!bFire)
            {
                if (bNewFireState && !bFireTriggered) bFireTriggered = true;
            }
            bFire = bNewFireState;
            int iInput = (bThrottle ? 8 : 0) + (bLeft ? 4 : 0) + (bRight ? 2 : 0) + (bAdjust ? 1 : 0); //bFire not included since bullets are added separately below

            fReplayMessageTimer += Time.fixedDeltaTime;
            if (iInput != iLastInput || fReplayMessageTimer > 0.5)
            {
                //add input message to replay if input has changed
                ReplayMessage rm = new ReplayMessage();
                rm.vPos = oRb.position;
                rm.vVel = oRb.velocity;
                rm.fDirection = fDirection;
                rm.iKeyFlag = (byte)iInput;
                rm.iID = 0;
                rm.iType = (byte)MsgType.MOVEMENT;
                GameLevel.theReplay.Add(rm);
                iLastInput = iInput;
                fReplayMessageTimer = 0;
            }
        }
        //////end of get input

        //////react to input
        if (fShipHealth <= 0 && !GameLevel.bRunReplay) Kill(true); //only kill if not in replay
        if (fShipHealth != FULL_HEALTH) bAchieveNoDamage = false;
        if (fShipHealth < fLastShipHealth) fDamageTimer = 0.0f;
        fLastShipHealth = fShipHealth;

        fDamageTimer += Time.fixedDeltaTime;
        if (bFreeFromBullets)
        {
            fFreeFromBulletsTimer += Time.fixedDeltaTime;
            if (fFreeFromBulletsTimer > GameLevel.BULLETFREETIME)
            {
                bFreeFromBullets = false;
            }
        }

        if (!bAlive)
        {
            //maybe come to life again
            fExplosionTimer += Time.fixedDeltaTime;
            if (fExplosionTimer > 1.9f)
            {
                if (iNumLifes != 0)
                {
                    //set alive again
                    bAlive = true;
                    this.oCustomGravity.enabled = true;

                    //activate ship
                    oShip.SetActive(true);
                    oPolygonCollider2D.enabled = true;

                    bFreeFromBullets = true;
                    fFreeFromBulletsTimer = 0.0f;
                    fDirection = 90.0f;
                    oRb.rotation = fDirection;
                    oRb.position = vStartPos;
                    oRb.transform.eulerAngles = new Vector3(0, 0, fDirection);
                    oRb.transform.position = vStartPos;
                    oRb.mass = SHIP_MASS;
                    oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;
                    bLanded = false;
                    fFuel = MAXFUELINTANK;
                    fShipHealth = FULL_HEALTH;
                }
                //stop explosion always when it's done
                oExplosionParticle.Stop();
            }
        }
        else
        {
            //get rotation
            fDirection = oRb.rotation;

            //clipping, we cannot trust that m_oRb.rotation is 0..360 that we rely on
            int iNumCounts = 0;
            if (fDirection < 0)
                iNumCounts = (int)(fDirection / 360.0f) - 1;
            else if (fDirection > 360.0f)
                iNumCounts = (int)(fDirection / 360.0f);
            fDirection -= iNumCounts * 360.0f;

            float fTemp = (fDirection * (Mathf.PI / 180.0f));
            float fSin = Mathf.Sin(fTemp);
            float fCos = Mathf.Cos(fTemp);

            fTemp = (fFuel != 0 && bThrottle) ? SHIP_THRUST : 0.0f;
            if (fTemp != fAcceleration)
            {
                if (fTemp != 0)
                {
                    oThrusterEmission.enabled = true;
                    bEngineFadeOut = false;
                    oASEngine.volume = 0.15f;
                    oASEngine.Play();
                }
                else
                {
                    oThrusterEmission.enabled = false;
                    bEngineFadeOut = true;
                }
            }

            fAcceleration = fTemp;
            vForceDir.x = fCos;
            vForceDir.y = fSin;

            oRb.AddForce(vForceDir * fAcceleration * 3.6f, ForceMode2D.Force);

            //steering
            {
                if (bAdjust)
                {
                    if (fDirection != 90.0f)
                    {
                        //get way to turn
                        int iSign = 1, iSignAfter = 1;
                        if (fDirection < 270.0f && fDirection > 90.0f) iSign = -1;
                        fDirection += iSign * ((SHIP_STEERSPEED / 3.0f) * Time.fixedDeltaTime);
                        if (fDirection < 270.0f && fDirection > 90.0f) iSignAfter = -1;
                        if (iSign != iSignAfter)
                            fDirection = 90.0f;
                    }
                }
                else
                { //normal steering
                    fDirection -= bRight ? SHIP_STEERSPEED * Time.fixedDeltaTime : 0.0f;
                    fDirection += bLeft ? SHIP_STEERSPEED * Time.fixedDeltaTime : 0.0f;
                }
                //clipping, to try and keep m_oRb.rotation 0..360
                iNumCounts = 0;
                if (fDirection < 0)
                    iNumCounts = (int)(fDirection / 360.0f) - 1;
                else if (fDirection > 360.0f)
                    iNumCounts = (int)(fDirection / 360.0f);
                fDirection -= iNumCounts * 360.0f;

                oRb.MoveRotation(fDirection);
            }

            //consume fuel
            if (oMap.iLevelType != (int)LevelType.MAP_RACE)
            {
                if (fAcceleration != 0)
                {
                    fFuel -= Time.fixedDeltaTime;
                    if (fFuel > 0) fAchieveFuelBurnt += Time.fixedDeltaTime;
                }
                if (fFuel < 0) fFuel = 0;
            }

            //checkpoints
            if (oMap.iLevelType == (int)LevelType.MAP_RACE) HandleRace();

            //actions on landingzones
            if (bLanded)
            {
                LandingZone oZone = oMap.GetLandingZone(iZoneId);

                //add fuel
                if (oZone.bHomeBase)
                {
                    if (fFuel < MAXFUELINTANK)
                    {
                        //refuel 10 times faster than fuelburn
                        fFuel += Time.fixedDeltaTime * 10;
                        if (fFuel > MAXFUELINTANK) fFuel = MAXFUELINTANK;
                    }
                }

                if (fLandTime > 0.8f)
                {
                    fLandTime = 0.0f;

                    //first take any extra life
                    if (oZone.bExtraLife)
                    {
                        oZone.TakeExtraLife();
                        if (oMap.iLevelType != (int)LevelType.MAP_RACE) iNumLifes++;
                    }
                    else
                    { //then act on cargo
                        if (!oZone.bHomeBase)
                        { //pick up cargo
                            int iCargo = oZone.PopCargo(true);
                            if (iCargo != -1)
                            { //there are cargo left on zone
                                if (SpaceInHold(iCargo))
                                { //player has space in hold
                                    LoadCargo(iCargo, oZone.iId);
                                    oZone.PopCargo(false);
                                }
                            }
                        }
                        else
                        { //unload cargo
                            UnloadCargo();
                        }
                    }
                } //else no action this time
            }

            if (bFireTriggered)
            {
                bFireTriggered = false; //reset so we can fire again
                CreateBullet();

                oASGeneral.PlayOneShot(oClipFire, 0.8f);

                iAchieveBulletsFired++;
            }
        }
        //////end react to input

        //fade out some sounds over ~100ms, to avoid unwanted clicking noise
        if (bEngineFadeOut)
            oASEngine.volume *= 0.8f;
        if (bScrapeFadeOut)
            oASScrape.volume *= 0.8f;
    }

    void CreateBullet()
    {
        S_BulletInfo stBulletInfo;

        float fSin = Mathf.Sin(fDirection * (Mathf.PI / 180.0f));
        float fCos = Mathf.Cos(fDirection * (Mathf.PI / 180.0f));
        stBulletInfo.vPos = oRb.position + new Vector2(fCos * 0.094f, fSin * 0.094f);
        stBulletInfo.vVel = oRb.velocity + new Vector2(fCos * Bullet.BULLETBASEVEL, fSin * Bullet.BULLETBASEVEL);
        stBulletInfo.fDirection = fDirection;

        GameObject o = Instantiate(oMap.oBulletObjBase, oMap.transform);
        o.GetComponent<Bullet>().Init(stBulletInfo, 0/*iOwnerID*/);

        ReplayMessage rm = new ReplayMessage();
        rm.vPos = stBulletInfo.vPos;
        rm.vVel = stBulletInfo.vVel;
        rm.fDirection = stBulletInfo.fDirection;
        rm.iID = 0;
        rm.iType = (byte)MsgType.BULLETP_NEW;
        GameLevel.theReplay.Add(rm);
    }

    public int GetScore()
    {
        int iLifes = iNumLifes;
        if (iLifes < 0) iLifes = 0;

        int iResultScore = (iScore + iLifes * 50) * 1000;
        if (oMap.iLevelType == (int)LevelType.MAP_MISSION)
        {
            iResultScore -= (int)((fTotalTimeMission * 1000) / 2); //long time is bad in mission
        }
        if (GameLevel.theReplay.bEasyMode && iResultScore > 0) iResultScore /= 2;
        return iResultScore;
    }

    void HandleRace()
    {
        Vector2 vPos = new Vector2(oRb.transform.position.x, oRb.transform.position.y);

        //checkpoints
        int i, iSize = oMap.aCheckPointList.Count;
        for (i = 0; i < iSize; i++)
        {
            //start counting if any checkpoint is passed to prevent clearing the map from enemies by going backwards
            if (fTotalTime == 0)
            {
                if (oMap.aCheckPointList[i].GetComponent<CheckPoint>().AtCP(vPos)) bTimeCounting = true;
            }
        }

        if (bTimeCounting && fAcceleration == 0.0f) bAchieveFullThrottle = false;

        if (oMap.aCheckPointList[iCurCP].GetComponent<CheckPoint>().AtCP(vPos))
        {
            oMap.aCheckPointList[iCurCP].GetComponent<CheckPoint>().SetBlinkState(false);
            if (bTimeCounting)
            {
                if (++iCurCP == iSize)
                { //new lap
                    iCurCP = 0;
                    iCurLap++;

                    //finished?
                    if (iCurLap == iTotalLaps)
                    {
                        bTimeCounting = false;
                        oASGeneral.PlayOneShot(oClipCPEnd, 0.7f);

                        bAchieveFinishedRaceLevel = true;
                    }

                    //best lap time calculation
                    float fThisLapTime = fTotalTime - fLastNewLapTime;
                    if (fThisLapTime < fBestLapTime)
                    {
                        fBestLapTime = fThisLapTime;
                    }
                    fLastNewLapTime = fTotalTime;
                }

                if (iCurLap < iTotalLaps)
                {
                    oASGeneral.PlayOneShot(oClipCP, 0.7f);

                    oMap.aCheckPointList[iCurCP].GetComponent<CheckPoint>().SetBlinkState(true);
                }
            }
        }
    }

    public void StopSound()
    {
        //bEngineFadeOut = true;
        //bScrapeFadeOut = true;
        //^will not make the sounds stop since FixedUpdate is run too slowly
        oASEngine.Stop();
        oASScrape.Stop();
        //^may cause clicking but we have to live with that
    }

    void Stop()
    {
        oCustomGravity.enabled = false;
        oRb.velocity = Vector2.zero;
        oRb.angularVelocity = 0;

        if (fAcceleration != 0.0f)
        {
            oThrusterEmission.enabled = false;
            bEngineFadeOut = true;
        }
        fAcceleration = 0.0f;
    }

    void Kill(bool bSetToReplay)
    {
        int i;

        if (!bAlive) return;
        bAlive = false;

        //prevent instant change in the music
        asm.ResetLife();
        //set mean speed to 0 to make one always exit flow
        for (i = 0; i < fMeanSpeeds.Length; i++)
        {
            fMeanSpeeds[i] = 0.0f;
        }

        //take a life if not unlimited
        if (iNumLifes != -1) iNumLifes--;
        iAchieveShipsDestroyed++;

        //inactivate ship
        oShip.SetActive(false);
        oPolygonCollider2D.enabled = false; //make the explosion not moving because of an enemy moving and pushing it
        //start explosion
        oExplosionParticle.Play();
        fExplosionTimer = 0.0f;

        oASGeneral.PlayOneShot(oClipExplosion, 0.8f);

        //to prevent movement during explosion
        Stop();

        //respawn cargo on landingzone taken from
        for (i = iCargoNumUsed - 1; i >= 0; i--)
        {
            LandingZone oZone = oMap.GetLandingZone(aHoldZoneId[i]);
            oZone.PushCargo(aHold[i]);
        }
        //reset cargo
        iCargoNumUsed = iCargoSpaceUsed = 0;

        if (bSetToReplay)
        {
            ReplayMessage rm = new ReplayMessage();
            rm.vPos = oRb.position;
            rm.vVel = Vector2.zero;
            rm.fDirection = fDirection;
            rm.iID = 0;
            rm.iType = (byte)MsgType.PLAYER_KILL;
            GameLevel.theReplay.Add(rm);
        }
    }

    bool SpaceInHold(int iCargo)
    {
        return (iCargoNumUsed < MAXSPACEINHOLDUNITS && (iCargoSpaceUsed + iCargo) <= MAXSPACEINHOLDWEIGHT);
    }

    bool LoadCargo(int iCargo, int iFromZone)
    {
        bool bCargoLoaded = false;

        if (SpaceInHold(iCargo))
        {
            //space in hold, add cargo
            iCargoSpaceUsed += iCargo;
            aHoldZoneId[iCargoNumUsed] = iFromZone;
            aHold[iCargoNumUsed] = iCargo;
            aHoldHealth[iCargoNumUsed] = 1.0f; //100%
            iCargoNumUsed++;
            bCargoLoaded = true;

            //new weight
            oRb.mass += iCargo / 11.0f;
            oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;

            //play load cargo sound
            oASGeneral.PlayOneShot(oClipLoadCargo);
        }

        return bCargoLoaded;
    }

    void UnloadCargo()
    {
        if (iCargoNumUsed > 0)
        {
            iCargoNumUsed--;
            iCargoSpaceUsed -= aHold[iCargoNumUsed];

            //new weight
            oRb.mass -= aHold[iCargoNumUsed] / 11.0f;
            oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;

            //add score for the cargo moved
            iScore += Mathf.RoundToInt(aHold[iCargoNumUsed] * aHoldHealth[iCargoNumUsed]);

            //add flying score text
            S_FlyingScoreInfo stFlyingScoreInfo;
            if(bCargoSwingingMode) stFlyingScoreInfo.szScore = aHold[iCargoNumUsed].ToString() + " @" + Mathf.RoundToInt(aHoldHealth[iCargoNumUsed]*100.0f) + "%";
            else stFlyingScoreInfo.szScore = aHold[iCargoNumUsed].ToString();
            stFlyingScoreInfo.vPos = new Vector3(oRb.position.x, oRb.position.y, -0.2f);
            stFlyingScoreInfo.vVel = new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f), -0.35f);
            FlyingScore o = Instantiate(oMap.oFlyingScoreObjBase, oMap.transform);
            o.Init(stFlyingScoreInfo);

            //play unload cargo sound
            oASGeneral.PlayOneShot(oClipUnloadCargo);
        }
    }

    public int GetCargoLoaded()
    {
        return iCargoSpaceUsed;
    }
}
