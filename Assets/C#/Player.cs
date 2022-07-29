using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

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
    const float fGravityScale = 0.045f;

    //motion movement
    internal bool bMotionMovementEnabled = false;
    internal Vector2 vSteerToPoint;

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
    internal int iCargoNumUsed = 0;
    int iCargoSpaceUsed = 0;
    int[] aHold = new int[MAXSPACEINHOLDUNITS];
    int[] aHoldZoneId = new int[MAXSPACEINHOLDUNITS];

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

    float fMovementTimer = 0.0f;
    float fFireTimer = 0.0f;
    float fFullThrottleTimer = 0.0f;

    bool bInited;
    private void Awake()
    {
        bInited = false;
        oThrusterEmission = oThruster.emission;
        oThrusterEmission.enabled = false;
        oWallsCollEmission = oWallsColl.emission;
        oWallsCollEmission.enabled = false;
        asm = GameObject.Find("AudioStateMachineDND").GetComponent<AudioStateMachine>();

        FULL_HEALTH = GameLevel.theReplay.bEasyMode ? 7.5f : 1.5f;

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
            if (aSource.clip!=null && aSource.clip.name.Equals("engine"))
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
        else if(fDamageTimer<0.21f) oShipMaterial.color = oShipColorDamage;
        else oShipMaterial.color = oShipColorNormal;
        oShipBodyMR.material = oShipMaterial;

        //update status gui
        if (oMap.iLevelType == (int)LevelType.MAP_RACE)
            status.SetForRace(fShipHealth / FULL_HEALTH, fTotalTime, "[" + iCurLap.ToString() + "/" + iTotalLaps.ToString() + "] " + iCurCP.ToString());
        else
            status.SetForMission(fShipHealth / FULL_HEALTH, iNumLifes, iCargoSpaceUsed / MAXSPACEINHOLDWEIGHT,
                iCargoNumUsed==3 || iCargoSpaceUsed==MAXSPACEINHOLDWEIGHT, fFuel / MAXFUELINTANK, GetScore()/1000);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        //must exist one impact point

        int iNum = collision.contactCount;
        ContactPoint2D c = collision.GetContact(0);
        float fImpulse = c.normalImpulse * 100.0f;
        float fTangentImpulse = c.tangentImpulse * 100.0f; //sliding impact?
        float fSpeedX = c.relativeVelocity.x *10;
        float fSpeedY = c.relativeVelocity.y *10;
        //then it might be a second impact point (that's all we take into concideration
        // any more and the ship is in deep trouble)
        if (iNum > 1)
        {
            c = collision.GetContact(1);
            fImpulse += c.normalImpulse * 100.0f;
            fTangentImpulse += c.tangentImpulse * 100.0f; //sliding impact?
            fSpeedX += c.relativeVelocity.x *10;
            fSpeedY += c.relativeVelocity.y *10;
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

            oASGeneral.PlayOneShot(oClipLand);
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

    ////////////////////////////////////////////////////////////////////////
    //code used in point movement
    struct S_CurState
    {
        public Vector2 vPos;
        public Vector2 vVel;
        public float fPointing;

        public Vector2 l0; //original pos
        public Vector2 l1; //goal

        public int iLastMove;
        public float fTimestep;
    }
    struct S_CurMove
    {
        public S_CurState stValue;
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;
    }

    struct S_Move
    {
        public S_Move(bool i_bLeft, bool i_bRight, bool i_bThrottle)
        {
            bLeft = i_bLeft;
            bRight = i_bRight;
            bThrottle = i_bThrottle;
        }
        public bool bLeft, bRight, bThrottle;
    }
    S_Move[] ALLMOVES =
        {new S_Move(false, false, false), new S_Move(false, false, true), new S_Move(true, false, false),
        new S_Move(true, false, true), new S_Move(false, true, false), new S_Move(false, true, true) };

    int[,] MOVE_TO_MOVE_SCORE = {
        { 0, 100, 100, 100, 100, 100}, //from 0 to 
        { 100, 0, 100, 100, 100, 100}, //from 1 to 
        { 100, 100, 0, 100, 400, 400},
        { 100, 100, 100, 0, 400, 400},
        { 100, 100, 400, 400, 0, 100},
        { 100, 100, 400, 400, 100, 0}
    };

    void GenerateMoves(S_CurState i_stCurrent, float i_fTime, out S_CurMove[] o_stMoves)
    {
        int i;
        o_stMoves = new S_CurMove[6];
        for (i = 0; i < 6; i++)
        {
            S_CurState stNew = i_stCurrent;

            stNew.fPointing -= ALLMOVES[i].bRight ? SHIP_STEERSPEED * i_fTime : 0;
            stNew.fPointing += ALLMOVES[i].bLeft ? SHIP_STEERSPEED * i_fTime : 0;
            if (stNew.fPointing < 0) stNew.fPointing += 360;
            if (stNew.fPointing > 360) stNew.fPointing -= 360;

            float fThrust = ALLMOVES[i].bThrottle ? SHIP_THRUST : 0;
            
            Vector2 a = new Vector2(Mathf.Cos(stNew.fPointing * (Mathf.PI / 180)) * fThrust, Mathf.Sin(stNew.fPointing * (Mathf.PI / 180)) * fThrust);
            a += oCustomGravity.force;
            a -= stNew.vVel * oRb.drag;

            stNew.vPos += (stNew.vVel * i_fTime) + (0.5f * a * i_fTime * i_fTime);
            stNew.vVel += a * i_fTime;

            o_stMoves[i].stValue = stNew;
            o_stMoves[i].a = Strategy5(stNew);
            o_stMoves[i].b = Strategy1(stNew);
            o_stMoves[i].c = Strategy2(stNew);
            o_stMoves[i].d = Strategy4(stNew);
            o_stMoves[i].e = MOVE_TO_MOVE_SCORE[stNew.iLastMove, i] * (1.0f/ Strategy5(stNew));
        }
    }

    float[] TIME_FACTOR = { 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 3.0f, 5.0f, 5.0f, 5.0f, 5.0f };
    int EvaluateMoves(S_CurState i_stCurrent, int i_iLevel, bool i_bShallow)
    {
        int i, iMove = 0;
        float fBest = 1000000000, fWorst = -1000000000;
        float[] dTest = new float[6];
        float a, b, c, d, e;
        S_CurMove[] aMoves;
        int iWorst;

        S_CurState stCurrent = i_stCurrent;
        stCurrent.fTimestep = 0.07f * TIME_FACTOR[i_iLevel];
        GenerateMoves(stCurrent, 0.07f * TIME_FACTOR[i_iLevel], out aMoves);

        for (i = 0; i < 6; i++)
        {
            a = aMoves[i].a;
            b = aMoves[i].b;
            c = aMoves[i].c;
            d = aMoves[i].d;
            e = aMoves[i].e;
            //dTest[i] = a * 0.0f + b * 1000.0f + c * 1.0f + d * 10.0f + e * 0.05f;
            dTest[i] = a * 0.0f + b * 10000.0f + c * 2.0f + d * 10.0f + e * 0.1f;

            if (dTest[i] > fWorst)
            {
                fWorst = dTest[i];
                iWorst = i;
            }
        }

        { //full search
            for (i = 0; i < 6; i++)
            {
                //recurse...
                stCurrent = aMoves[i].stValue;
                stCurrent.iLastMove = i;
                if (i_iLevel < /**/1)
                {
                    dTest[i] += EvaluateMoves(stCurrent, i_iLevel + 1, true);
                }

                if (dTest[i] < fBest)
                {
                    fBest = dTest[i];
                    iMove = i;
                }
            }
        }

        if (i_iLevel > 0) return (int)fBest;
        else return iMove;
    }

    //score for how far from the line
    float Strategy0(S_CurState stValues)
    {
        return CheckPoint.PointDistanceToLineSeg(stValues.vPos, stValues.l0, stValues.l1);
    }
    //score for how much closer to goal
    float Strategy1(S_CurState stValues)
    {
        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        float fDistBefore = ((stValues.vPos - (stValues.vVel * stValues.fTimestep)) - stValues.l1).magnitude;

        float fScore = (fDistNow - fDistBefore); //negative value is closer
        return fScore;
    }
    float CalcLineAngle(Vector2 a, Vector2 b)
    {
        float difx = b.x - a.x;
        float dify = b.y - a.y;

        float fResult = 360.0f - (Mathf.Atan2(-dify, difx) * (180.0f / Mathf.PI));
        if (fResult >= 360.0f) fResult -= 360.0f;
        else if (fResult < 0.0f) fResult += 360.0f;

        return fResult;
    }
    //score for pointing toward goal (more important further from goal)
    float Strategy2(S_CurState stValues)
    {
        float fGoalAngle = CalcLineAngle(stValues.vPos, stValues.l1);
        float fDiff = fGoalAngle - stValues.fPointing;
        if (fDiff < 0) fDiff += 360;
        if (fDiff > 360) fDiff -= 360;
        if (fDiff > 180) fDiff = 360 - fDiff; //the difference of two angles can be max 180.

        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        if (fDistNow > 1.0f) fDistNow = 1.0f;

        float fScore = fDiff / fDistNow;
        return fScore;
    }
    //score for speed
    float Strategy3(S_CurState stValues)
    {
        float fVel = stValues.vVel.magnitude;

        float fScore = -fVel;
        return fScore;
    }
    //score for speed vs dist to goal
    float Strategy4(S_CurState stValues)
    {
        float fVel = stValues.vVel.magnitude;

        float fDistNow = (stValues.vPos - stValues.l1).magnitude;
        if (fDistNow > 1.0f) fDistNow = 1.0f;

        float fScore = -fVel / (1.0f/fDistNow);
        return fScore;
    }
    //score for dist to goal
    float Strategy5(S_CurState stValues)
    {
        float fDistNow = (stValues.vPos - stValues.l1).magnitude;

        float fScore = fDistNow;
        return fScore;
    }
    ////////////////////////////////////////////////////////////////////////


    Vector2 vForceDir = new Vector2(0, 0);
    int iLastInput = 0; //bool bitfield
    float fReplayMessageTimer = 0;
    float[] fMeanSpeeds = new float[16];
    internal float fMeanSpeed = 0.0f;
    internal int iNumEnemiesNear = 0;
    internal int iNumBulletsNear = 0;
    float fCurrentSpeedSeg = 0;
    int iBestMove = 0;

    Vector2 vEnemyFirePos = new Vector2(0, 0);

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
            || oRb.position.y < -vMapSize.y / 20.0f || oRb.position.y > vMapSize.y / 20.0f) fShipHealth = 0.0f;
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
        if(iCurSec!= iLastSec)
            fMeanSpeeds[iCurSec] = 0.0f;
        fMeanSpeeds[iCurSec] += fDist;
        fMeanSpeed = 0.0f;
        for(int i=0; i< fMeanSpeeds.Length; i++)
        {
            if(i != iCurSec) fMeanSpeed += fMeanSpeeds[i];
        }
        fMeanSpeed = fMeanSpeed/(fMeanSpeeds.Length-1) * 10;

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

                    oASGeneral.PlayOneShot(oClipFire);
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

            //get input from joysticks
            float fX = 0;
            float fY = 0;
            float fTrg2 = 0;
            try
            {
                fX = SteamVR_Actions.default_Steering.axis.x;
                if (!bMotionMovementEnabled) fY = SteamVR_Actions.default_Steering.axis.y;
                fTrg2 = SteamVR_Actions.default_Throttle.axis;
            }
            catch { }

            if (fTrg2 > 0.3f) bThrottle = true;
            try { if (SteamVR_Actions.default_Throttle2.GetState(SteamVR_Input_Sources.Any)) bThrottle = true; } catch { }

            if (fX > 0.3f) bRight = true;
            if (fX < -0.3f) bLeft = true;
            if (fY < -0.75f && bLeft == false && bRight == false) bAdjust = true;
            if (fY < -0.85f) bAdjust = true; //safety if for some reason there is trouble getting adjust activated, if all the way down activate always

            //keyboard and joystick for fire (is a trigger once event)
            if (!bMotionMovementEnabled) try { if (SteamVR_Actions.default_Fire.GetState(SteamVR_Input_Sources.Any)) bNewFireState = true; } catch { }

            if (bMotionMovementEnabled)
            {
                fFireTimer += Time.fixedDeltaTime;
                fMovementTimer += Time.fixedDeltaTime;
                if(!bLanded) fFullThrottleTimer -= Time.fixedDeltaTime;
                else fFullThrottleTimer = 0.28f;
                
                //auto landing
                if (!bThrottle && fDirection!=90.0f)
                {
                    for (int i = 0; i < oMap.aLandingZoneList.Count; i++)
                    {
                        float w = oMap.aLandingZoneList[i].iZoneSize * 0.1f;
                        if (transform.position.y > oMap.aLandingZoneList[i].vPos.y &&
                            transform.position.y < oMap.aLandingZoneList[i].vPos.y + 0.4f &&
                            transform.position.x > (oMap.aLandingZoneList[i].vPos.x - w / 2) &&
                            transform.position.x < (oMap.aLandingZoneList[i].vPos.x + w / 2))
                        {
                            bAdjust = true;
                        }
                    }
                }

                //auto fire
                if (fFireTimer > 0.2f)
                {
                    for (int i = 0; i < oMap.aEnemyList.Count; i++)
                    {
                        if (oMap.aEnemyList[i] == null) continue;
                        vEnemyFirePos.x = oMap.aEnemyList[i].vPos.x;
                        vEnemyFirePos.y = oMap.aEnemyList[i].vPos.y;

                        float d1 = (oRb.position - vEnemyFirePos).sqrMagnitude;
                        if (d1 > (570.0f / 320.0f)) continue;

                        float d2 = (vSteerToPoint - vEnemyFirePos).sqrMagnitude;
                        if (d2 < 0.035f)
                        {
                            bNewFireState = true;
                            fFireTimer = 0.0f;
                            break;
                        }
                    }
                    for (int i = 0; i < oMap.aDoorList.Count; i++)
                    {
                        if (bNewFireState) break;
                        int numButtons = oMap.aDoorList[i].oButtons.GetLength(0);
                        for (int j = 0; j < numButtons; j++)
                        {
                            vEnemyFirePos.x = oMap.aDoorList[i].oButtons[j].transform.position.x;
                            vEnemyFirePos.y = oMap.aDoorList[i].oButtons[j].transform.position.y;

                            float d1 = (oRb.position - vEnemyFirePos).sqrMagnitude;
                            if (d1 > (570.0f / 320.0f)) continue;

                            float d2 = (vSteerToPoint - vEnemyFirePos).sqrMagnitude;
                            if (d2 < 0.04f)
                            {
                                bNewFireState = true;
                                fFireTimer = -0.4f; //extra time between shots if aiming at door
                                break;
                            }
                        }
                    }
                }

                if (bThrottle && !bAdjust && !bRight && !bLeft)
                {
                    //new move descicion
                    if (fMovementTimer > 0.07f)
                    {
                        fMovementTimer = 0;
                        if (fFullThrottleTimer > 0)
                        {
                            iBestMove = 1;
                            bThrottle = true;
                        }
                        else
                        {
                            S_CurState stCurrent;
                            stCurrent.vPos = oRb.position;
                            stCurrent.vVel = oRb.velocity;
                            stCurrent.fPointing = fDirection;
                            stCurrent.fTimestep = 0.1f;
                            stCurrent.l0 = oRb.position;
                            stCurrent.l1 = vSteerToPoint;
                            stCurrent.iLastMove = iBestMove;
                            iBestMove = EvaluateMoves(stCurrent, 0, true);

                            bThrottle = ALLMOVES[iBestMove].bThrottle;
                            bLeft = ALLMOVES[iBestMove].bLeft;
                            bRight = ALLMOVES[iBestMove].bRight;
                        }
                    }
                    //keep old move
                    else
                    {
                        bThrottle = ALLMOVES[iBestMove].bThrottle;
                        bLeft = ALLMOVES[iBestMove].bLeft;
                        bRight = ALLMOVES[iBestMove].bRight;
                    }
                }
            }

            //keyboard
            if (!bMotionMovementEnabled) if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space)) bNewFireState = true;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) bThrottle = true;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) bAdjust = true;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) { bLeft = true; bAdjust = false; }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) { bRight = true; bAdjust = false; }

            if (!bFire)
            {
                if (bNewFireState && !bFireTriggered) bFireTriggered = true;
            }
            bFire = bNewFireState;
            int iInput = (bThrottle ? 8 : 0) + (bLeft ? 4 : 0) + (bRight ? 2 : 0) + (bAdjust ? 1 : 0); //bFire not included since bullets are added separately below

            fReplayMessageTimer += Time.fixedDeltaTime;
            if (iInput != iLastInput || fReplayMessageTimer>0.5)
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
                    oASEngine.volume = 0.40f;
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

                oASGeneral.PlayOneShot(oClipFire);

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
        if (bMotionMovementEnabled)
        {
            //compensate for player velocity
            //not done
            //compensate for enemy velocity
            //not done

            //we fire at where the hand points
            vEnemyFirePos = vSteerToPoint;

            float fD = Vector2.SignedAngle(Vector2.right, vEnemyFirePos - oRb.position);
            int iNumCounts = 0;
            if (fD < 0)
                iNumCounts = (int)(fD / 360.0f) - 1;
            fD -= iNumCounts * 360.0f;

            fSin = Mathf.Sin(fD * (Mathf.PI / 180.0f));
            fCos = Mathf.Cos(fD * (Mathf.PI / 180.0f));
        }
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
            iResultScore -= (int)((fTotalTimeMission*1000) / 2); //long time is bad in mission
        }
        if (GameLevel.theReplay.bEasyMode && iResultScore>0) iResultScore /= 2;
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
                        oASGeneral.PlayOneShot(oClipCPEnd);

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
                    oASGeneral.PlayOneShot(oClipCP);

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

        oASGeneral.PlayOneShot(oClipExplosion);

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

        if(bSetToReplay)
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
            iScore += aHold[iCargoNumUsed];

            //add flying score text
            S_FlyingScoreInfo stFlyingScoreInfo;
            stFlyingScoreInfo.iScore = aHold[iCargoNumUsed];
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
