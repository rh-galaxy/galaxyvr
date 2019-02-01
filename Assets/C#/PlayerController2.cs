﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController2 : MonoBehaviour
{
    MapGenerator oMap;

    public GameObject oShip;
    public ParticleSystem oThruster;
    public GameObject oExplosion;
    public GameObject oShipBody;
    public Material oShipMaterial;
    public GameStatus status;

    //movement
    Rigidbody2D oRb;
    ConstantForce2D oCustomGravity;
    Vector2 vStartPos;
    float fAcceleration = 0.0f;
    float fDirection = 90.0f;
    const float fGravityScale = 0.055f;

    //ship properties
    const float MAXFUELINTANK = 60.0f;
    const float MAXSPACEINHOLDWEIGHT = 50.0f;
    const int MAXSPACEINHOLDUNITS = 3;
    const float FULL_HEALTH = 1.5f;
    const float SHIP_MASS = 60.0f;
    const float SHIP_STEERSPEED = 235.0f; //degree/second
    const float SHIP_THRUST = 160.0f;
    const int NUM_LIFES_MISSION = 5;

    //cargo
    int iCargoNumUsed = 0;
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
    int iScore = 0;
    //achievements
    internal float fAchieveFuelBurnt = 0;
    internal int iAchieveShipsDestroyed = 0;
    internal bool bAchieveNoDamage = true;
    internal bool bAchieveFullThrottle = true;
    internal bool bAchieveFinishedRaceLevel = false;

    //ship status
    internal int iNumLifes = -1; //no limit
    float fShipHealth = FULL_HEALTH;

    float fFuel;

    bool bLanded = false;
    float fLandTime = 0.0f;
    int iZoneId = -1;
    int iNoStearing = 0;
    float fNoStearingTimer = 0.0f;

    bool bAlive = true;
    bool bFreeFromBullets = false;
    float fFreeFromBulletsTimer = 0.0f;
    float fExplosionTimer = 0.0f;

    bool bInited;
    private void Awake()
    {
        bInited = false;
    }

    public void Init(string i_szName, int i_iPlayerID, Vector2 i_vStartPos, MapGenerator i_oMap)
    {
        bInited = true;

        oMap = i_oMap;
        vStartPos = i_vStartPos;
        fLandTime = 0.0f;

        iTotalLaps = oMap.iRaceLaps;

        fFuel = MAXFUELINTANK;
        oThruster.enableEmission = false;

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
        }
    }

    bool bFireTriggered = false, bFire = false;
    bool bThrottle, bLeft, bRight, bAdjust;

    Color oShipColorNormal = new Color(138 / 255.0f, 0, 0, 1.0f);
    Color oShipColorBlink = new Color(180 / 255.0f, 0, 0, 0.5f);
    void Update()
    {
        if (!bInited)
            return;

        //update ship color when blinking
        if (bFreeFromBullets && (((int)(fFreeFromBulletsTimer * 1000)) % 300 > 200))
            oShipMaterial.color = oShipColorBlink;
        else oShipMaterial.color = oShipColorNormal;
        oShipBody.GetComponent<MeshRenderer>().material = oShipMaterial;


        if (oMap.iLevelType == (int)LevelType.MAP_RACE)
            status.SetForRace(fShipHealth / FULL_HEALTH, fTotalTime, "[" + iCurLap.ToString() + "/" + iTotalLaps.ToString() + "] " + iCurCP.ToString());
        else
            status.SetForMission(fShipHealth / FULL_HEALTH, iNumLifes, iCargoSpaceUsed / MAXSPACEINHOLDWEIGHT,
                iCargoNumUsed==3 || iCargoSpaceUsed==MAXSPACEINHOLDWEIGHT, fFuel / MAXFUELINTANK, iScore);
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(0, 0, 1000, 1000), "Health " + (fShipHealth * 100).ToString());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        //must exist one impact point

        int iNum = collision.contactCount;
        ContactPoint2D c = collision.GetContact(0);
        float fImpulse = c.normalImpulse * 1.0f;
        float fTangentImpulse = c.tangentImpulse * 1.0f; //sliding impact?
        float fSpeedX = c.relativeVelocity.x;
        float fSpeedY = c.relativeVelocity.y;
        //then it might be a second impact point (that's all we take into concideration
        // any more and the ship is in deep trouble)
        if (iNum > 1)
        {
            c = collision.GetContact(1);
            fImpulse += c.normalImpulse * 1.0f;
            fTangentImpulse += c.tangentImpulse * 1.0f; //sliding impact?
            fSpeedX += c.relativeVelocity.x;
            fSpeedY += c.relativeVelocity.y;
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

            iNoStearing++;
            fNoStearingTimer = 0.0f;
        }

        if (szOtherObject.CompareTo("Map") == 0)
        {
            //minimum impulse to damage (0 - always damage on map)
            fImpulse -= 0.0f;
            if (fImpulse <= 0) fImpulse = 0.0f;

            fShipHealth -= (fImpulse / 80.0f) * 0.5f;

            iNoStearing++;
            fNoStearingTimer = 0.0f;
        }

        //collide with enemy body
        if (szOtherObject.StartsWith("Enemy"))
        {
            if (!bFreeFromBullets) fShipHealth -= 5.0f; //instant kill
        }

        //enemy bullet, take damage
        if (szOtherObject.StartsWith("BulletE"))
        {
            if (!bFreeFromBullets) fShipHealth -= 0.5f;

            //play hit sound
            //...
        }

        //own bullet, do nothing
        if (szOtherObject.StartsWith("BulletP"))
        {
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

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
                fShipHealth -= 0.1f * Time.fixedDeltaTime;
            if (fDiff > 80)
                fShipHealth -= 0.5f * Time.fixedDeltaTime;

            //landing stabel
            if (fDiff < 1)
            {
                fLandTime += Time.fixedDeltaTime;
                if (fLandTime > 0.3f && !bLanded)
                {
                    bLanded = true;
                    iZoneId = int.Parse(szOtherObject.Substring(11));
                }
            }
        }

        if (szOtherObject.CompareTo("Map") == 0)
        {
            fShipHealth -= 0.5f * Time.fixedDeltaTime;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        if (szOtherObject.StartsWith("LandingZone"))
        {
            iNoStearing--;
            fLandTime = 0.0f;
            bLanded = false;
        }

        if (szOtherObject.CompareTo("Map") == 0)
        {
            iNoStearing--;
        }
    }

    Vector2 vForceDir = new Vector2(0, 0);
    int iLastInput = 0; //bool bitfield
    float fReplayMessageTimer = 0;
    void FixedUpdate()
    {
        if (!bInited)
            return;

        //////get input, either from replay or from human player
        if (MapGenerator.bRunReplay)
        {
            ReplayMessage rm;
            while (MapGenerator.theReplay.Get(out rm, 0))
            {
                //we got new input
                if (rm.iType == (byte)MsgType.MOVEMENT)
                {
                    oRb.position = rm.vPos;
                    oRb.velocity = rm.vVel;
                    fDirection = rm.fDirection;
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
                }
                if (rm.iType == (byte)MsgType.PLAYER_KILL)
                {
                    oRb.position = rm.vPos;
                    oRb.velocity = rm.vVel;
                    fDirection = rm.fDirection;
                    Kill(false);
                }
            }
        }
        else
        {
            //get input from joysticks
            float fX = Input.GetAxisRaw("Horizontal");                                          //axis x (x left stick)
            float fY = Input.GetAxisRaw("Vertical");                                            //axis y (y left stick)
            float fX2 = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal"); //axis 4 (x right stick)
            float fY2 = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical");   //axis 5 (y right stick)
            float fTrg1 = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");         //axis 9
            float fTrg2 = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");       //axis 10

            bThrottle = bLeft = bRight = bAdjust = false;

            //keyboard
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) bThrottle = true;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) bAdjust = true;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) bLeft = true;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) bRight = true;

            //joysticks
            if (fX > 0.3f) bRight = true;
            if (fX < -0.3f) bLeft = true;
            if (fX2 > 0.3f) bRight = true;
            if (fX2 < -0.3f) bLeft = true;
            if (fY < -0.5f && bLeft == false && bRight == false) bAdjust = true;
            if (fY2 < -0.5f && bLeft == false && bRight == false) bAdjust = true;
            if (fY2 > 0.5f || fTrg1 > 0.3f || fTrg2 > 0.3f) bThrottle = true;
            if (Input.GetButton("Fire2")) bThrottle = true; //button 1 (B)
            if (Input.GetButton("Jump")) bThrottle = true;  //button 3 (Y)

            //keyboard and joystick for fire (is a trigger once event)
            bool bNewFireState = false;
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space)) bNewFireState = true;
            if (Input.GetButton("Fire1")) bNewFireState = true;     //button 0 (A)
            if (Input.GetButton("Fire3")) bNewFireState = true;     //button 2 (X)
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
                MapGenerator.theReplay.Add(rm);
                iLastInput = iInput;
                fReplayMessageTimer = 0;
            }
        }
        //////end of get input

        //////react to input
        fNoStearingTimer += Time.fixedDeltaTime;

        if (fShipHealth < 0 && !MapGenerator.bRunReplay) Kill(true); //only kill if not in replay
        if (fShipHealth != FULL_HEALTH) bAchieveNoDamage = false;

        if (bFreeFromBullets)
        {
            fFreeFromBulletsTimer += Time.fixedDeltaTime;
            if (fFreeFromBulletsTimer > MapGenerator.BULLETFREETIME)
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
                    GetComponent<PolygonCollider2D>().enabled = true;

                    //if (m_iNoStearing != 0)
                    //    this will fix itself when we move to the start pos below

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
                oExplosion.SetActive(false);
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

                //ParticleSystem.EmissionModule oEM = m_oThruster.emission;
                //using that does not work: (oEM.enabled = false;), but m_oThruster.enableEmission = false; is depricated...
                if (fTemp != 0)
                {
                    oThruster.enableEmission = true;
                    //m_pEngineSoundCh = C_Sound::GetSoundObject()->Play(2, m_fVol, m_fPan, true);
                }
                else
                {
                    oThruster.enableEmission = false;
                    //C_ShipPlayer::s_clFadeOutList.push_back(m_pEngineSoundCh);
                }
            }
            fAcceleration = fTemp;
            vForceDir.x = fCos;
            vForceDir.y = fSin;

            oRb.AddForce(vForceDir * fAcceleration * 3.6f, ForceMode2D.Force);

            /**/
            if (iNoStearing < 0)
                iNoStearing = iNoStearing; //not to happen

            if (true) //was if(iNoStearing == 0 && fNoStearingTimer > 0.2f), but it's probably better letting the ship be able to steer
            { //cannot stear ship if in collision
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
                    if (fFuel < 0) fAchieveFuelBurnt += Time.fixedDeltaTime;
                }
                if (fFuel < 0) fFuel = 0;
            }

            //checkpoints
            if (oMap.iLevelType == (int)LevelType.MAP_RACE) HandleRace();

            //actions on landingzones
            if (bLanded)
            {
                C_LandingZone oZone = oMap.GetLandingZone(iZoneId);

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

                //C_Sound::GetSoundObject()->Play(0, m_fVol, m_fPan); //play bullet sound
            }
        }
        //////end react to input
    }

    void CreateBullet()
    {
        S_BulletInfo stBulletInfo;

        float fSin = Mathf.Sin(fDirection * (Mathf.PI / 180.0f));
        float fCos = Mathf.Cos(fDirection * (Mathf.PI / 180.0f));
        stBulletInfo.vPos = oRb.position + new Vector2(fCos * 0.82f, fSin * 0.82f);
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
        MapGenerator.theReplay.Add(rm);
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
        if (bTimeCounting) fTotalTime += Time.fixedDeltaTime;

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
                        //m_pclSoundFX->Play(6);

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
                    //m_pclSoundFX->Play(5);
                    oMap.aCheckPointList[iCurCP].GetComponent<CheckPoint>().SetBlinkState(true);
                }
            }
        }
    }

    void Stop()
    {
        oCustomGravity.enabled = false;
        oRb.velocity = Vector2.zero;
        oRb.angularVelocity = 0;

        if (fAcceleration != 0.0f)
        {
            oThruster.enableEmission = false;
            //C_ShipPlayer::s_clFadeOutList.push_back(m_pEngineSoundCh);
        }
        fAcceleration = 0.0f;
    }

    void Kill(bool bSetToReplay)
    {
        if (!bAlive) return;
        bAlive = false;

        //take a life if not unlimited
        if (iNumLifes != -1) iNumLifes--;
        iAchieveShipsDestroyed++;

        //inactivate ship
        oShip.SetActive(false);
        GetComponent<PolygonCollider2D>().enabled = false; //make the explosion not moving because of an enemy moving and pushing it
        //start explosion
        oExplosion.SetActive(true);
        fExplosionTimer = 0.0f;

        //C_Sound::GetSoundObject()->Play(1, m_fVol, m_fPan); //play explosion sound

        //to prevent movement during explosion
        Stop();

        //respawn cargo on landingzone taken from
        int i;
        for (i = iCargoNumUsed - 1; i >= 0; i--)
        {
            C_LandingZone oZone = oMap.GetLandingZone(aHoldZoneId[i]);
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
            MapGenerator.theReplay.Add(rm);
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
            oRb.mass += iCargo;
            oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;

            //play load cargo sound
            //if (this == s_pclListner) if (C_Sound::GetSoundObject()) C_Sound::GetSoundObject()->Play(8, m_fVol, m_fPan);
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
            oRb.mass -= aHold[iCargoNumUsed];
            oCustomGravity.force = oMap.vGravity * oRb.mass * fGravityScale;

            //add score for the cargo moved
            iScore += aHold[iCargoNumUsed];

            //play unload cargo sound
            //if (this == s_pclListner) if (C_Sound::GetSoundObject()) C_Sound::GetSoundObject()->Play(9, m_fVol, m_fPan);

            //reset landtime just to make the delay before a level ends the same every time
            //fLandTime = 0;
        }
    }

    public int GetCargoLoaded()
    {
        return iCargoSpaceUsed;
    }
}
