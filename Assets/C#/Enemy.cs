using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    internal static int iOwnerIdBase = 10; //must be set to 10 also before load of every level
    int iOwnerId;
    internal S_EnemyInfo stInfo;

    GameLevel oMap;
    public GameObject enemy0;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;
    public GameObject enemy4_part;
    public GameObject enemy5;
    public GameObject enemy6;
    public ParticleSystem oExplosionParticle;

    public AudioClip oClipHit;
    public AudioClip oClipExplosion;
    public AudioClip oClipFire;

    AudioSource oAudioSource;

    internal Vector3 vPos;
    Vector3 vVel;
    int iCurWP;
    float fWPTime;
    float fFireTime;

    bool bInited = false;
    int iNumHits = 1;
    bool bStartExplosion = false;
    bool bExplosionStarted = false;
    float fExplosionTimer = 0.0f;

    int[] SENEMY_HITSTOKILL = { 4, 5, -1, -1, 8, 4, 4, 6 }; //-1=immortal
    int[] SENEMY_SCOREPOINTS = { 10, 20, 0, 0, 20, 15, 25, 25 };
    int[] SENEMY_NUMBULLETS = { 1, 3, 1, 3, 1, 2, 5, 8 };
    int[] SENEMY_BULLETSPEED = { 190, 160, 190, 160, 110, 90, 120, 140 };
    bool[] SENEMY_RANDOMBULLETANGLE = { false, false, false, false, true, false, false, false };
    int[,] SENEMY_BULLETANGLE = {
        {0,0,0,0,0,0,0,0}, {-45,0,45,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {-45,0,45,0,0,0,0,0},
        {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {-70,-35,0,35,70,0,0,0}, {-180,-135,-90,-45,0,45,90,135} };
    Vector2[,] SENEMY_FIREPOINT = {
        {new Vector2(10.0f/32.0f, 0), new Vector2(0, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 0), new Vector2(0, -10.0f/32.0f) },
        {new Vector2(10.0f/32.0f, 0), new Vector2(0, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 0), new Vector2(0, -10.0f/32.0f) },
        {new Vector2(10.0f/32.0f, 0), new Vector2(0, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 0), new Vector2(0, -10.0f/32.0f) },
        {new Vector2(10.0f/32.0f, 0), new Vector2(0, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 0), new Vector2(0, -10.0f/32.0f) },
        {new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) },
        {new Vector2(10.0f/32.0f, 13.0f/32.0f), new Vector2(-13.0f/32.0f, 10.0f/32.0f), new Vector2(-10.0f/32.0f, -13.0f/32.0f), new Vector2(13.0f/32.0f, -10.0f/32.0f) },
        {new Vector2(10.0f/32.0f, 0), new Vector2(0, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 0), new Vector2(0, -10.0f/32.0f) } };
    Vector2[] SENEMY5_FIREPOINT2 = {
        new Vector2(10.0f/32.0f, -13.0f/32.0f), new Vector2(13.0f/32.0f, 10.0f/32.0f), new Vector2(-10.0f/32.0f, 13.0f/32.0f), new Vector2(-13.0f/32.0f, -10.0f/32.0f) };


    public void Init(S_EnemyInfo i_stInfo, GameLevel i_oMap)
    {
        bInited = true;
        oMap = i_oMap;
        stInfo = i_stInfo;

        //set owner id to a uniqe value
        iOwnerId = iOwnerIdBase++;

        //set the correct one active
        gameObject.SetActive(true);
        enemy0.SetActive(stInfo.iEnemyType == 0);
        enemy1.SetActive(stInfo.iEnemyType == 1);
        enemy2.SetActive(stInfo.iEnemyType == 2);
        enemy3.SetActive(stInfo.iEnemyType == 3);
        enemy4.SetActive(stInfo.iEnemyType == 4);
        enemy5.SetActive(stInfo.iEnemyType == 5);
        enemy6.SetActive(stInfo.iEnemyType == 6);
        oExplosionParticle.Stop();

        oAudioSource = GetComponent<AudioSource>();

        //init for the current enemy type
        iCurWP = 1;
        fWPTime = 0;
        fFireTime = 0;
        if (stInfo.iEnemyType != 4) oExplosionParticle.transform.localScale = new Vector3(1.60f, 1.60f, 1.60f); //smaller explosion
        else oExplosionParticle.transform.localScale = new Vector3(2.00f, 2.00f, 2.00f); //normal explosion
        if (stInfo.iEnemyType == 0) enemy0.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 1) enemy1.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 2) enemy2.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 3) enemy3.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 4) enemy4.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 5) enemy5.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        if (stInfo.iEnemyType == 6) enemy6.transform.Rotate(new Vector3(0, 0, stInfo.iAngle));
        vPos.x = stInfo.vWayPoints[0].x;
        vPos.y = stInfo.vWayPoints[0].y;
        vPos.z = 0.0f;

        iNumHits = SENEMY_HITSTOKILL[stInfo.iEnemyType];

        name = "Enemy" + stInfo.iEnemyType.ToString();
    }

    public void HitByBullet()
    {
        //player bullet, one hit
        if (iNumHits > 0)
        {
            iNumHits--;

            if (iNumHits == 0)
            {
                //kill this enemy
                bStartExplosion = true;
            }
            else
            {
                oAudioSource.PlayOneShot(oClipHit);
            }
        }
        else
        {
            oAudioSource.PlayOneShot(oClipHit);
        }

        //collision with player, done in player

        //ignore other and own enemy bullets
        // is done in the collision matrix in Edit->Project Settings
    }

    public Player thePlayer; //set in editor
    bool InFireRange()
    {
        Vector2 vPlayerPos = thePlayer.GetPosition();
        Vector2 vDist = vPlayerPos - new Vector2(vPos.x, vPos.y);
        return (vDist.magnitude < (450/320.0f)); //~14 tiles, like in the old game
    }

    void FixedUpdate()
    {
        if (!bInited) return;

        //////handle creation of all new enemy bullets, could be done in another place...
        if (GameLevel.bRunReplay)
        {
            ReplayMessage rm;
            while (GameLevel.theReplay.Get(out rm, 1))
            {
                if (rm.iType == (byte)MsgType.BULLETE_NEW)
                {
                    //part of CreateBullet()
                    S_BulletInfo stBulletInfo;
                    stBulletInfo.vPos = rm.vPos;
                    stBulletInfo.vVel = rm.vVel;
                    stBulletInfo.fDirection = rm.fDirection;
                    GameObject o = Instantiate(oMap.oBulletObjBase, oMap.transform);
                    o.GetComponent<Bullet>().Init(stBulletInfo, 1/*iOwnerId*/);

                    if(rm.iGeneralByte1==0)
                        GetComponent<AudioSource>().PlayOneShot(oClipFire);
                }
            }

            if (GameLevel.theReplay.iVersion >= 1)
            {
                if(iNumHits!=0) iNumHits = 3; //fake always 3 hits left in replay, until kill below

                while (GameLevel.theReplay.Get(out rm, iOwnerId))
                {
                    if (rm.iType == (byte)MsgType.ENEMY_KILL)
                    {
                        bStartExplosion = true;
                        iNumHits = 0; //kill below
                    }
                }
            }
        }
        //////

        //wait for explosion to finish and remove object
        if (iNumHits==0 || bExplosionStarted)
        {
            //start explosion
            //note: some bug above can make this run more than once if hit by rapid fire.
            if (bStartExplosion && !bExplosionStarted)
            {
                bStartExplosion = false;
                bExplosionStarted = true; //hack: fixes bug

                //create kill message
                if (!GameLevel.bRunReplay && GameLevel.theReplay.iVersion >= 1)
                {
                    ReplayMessage rm = new ReplayMessage();
                    rm.iID = iOwnerId;
                    rm.iType = (byte)MsgType.ENEMY_KILL;
                    GameLevel.theReplay.Add(rm);
                }

                //play explosion sound
                oAudioSource.PlayOneShot(oClipExplosion);

                //add score
                oMap.iAchieveEnemiesKilled++;
                oMap.player.iScore += SENEMY_SCOREPOINTS[stInfo.iEnemyType];

                //add flying score text
                if (oMap.iLevelType == (int)LevelType.MAP_MISSION)
                {
                    S_FlyingScoreInfo stFlyingScoreInfo;
                    stFlyingScoreInfo.szScore = SENEMY_SCOREPOINTS[stInfo.iEnemyType].ToString();
                    stFlyingScoreInfo.vPos = new Vector3(transform.position.x, transform.position.y, -0.2f);
                    stFlyingScoreInfo.vVel = new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), UnityEngine.Random.Range(-0.15f, 0.15f), -0.35f);
                    FlyingScore o = Instantiate(oMap.oFlyingScoreObjBase, oMap.transform);
                    o.Init(stFlyingScoreInfo);
                }

                //set the specific enemytype inactive
                enemy0.SetActive(false);
                enemy1.SetActive(false);
                enemy2.SetActive(false);
                enemy3.SetActive(false);
                enemy4.SetActive(false);
                enemy5.SetActive(false);
                enemy6.SetActive(false);

                oExplosionParticle.Play();
                fExplosionTimer = 0.0f;
            }

            fExplosionTimer += Time.fixedDeltaTime;
            if (fExplosionTimer > 2.0f)
            {
                oExplosionParticle.Stop();
                Destroy(gameObject);
            }
        }
        else
        {
            //move along waypoints
            if (stInfo.iNumWayPoints > 1)
            {
                fWPTime -= Time.fixedDeltaTime;
                if (fWPTime <= 0)
                {
                    Vector2 vDist;
                    float fDist;

                    vDist = new Vector3(stInfo.vWayPoints[iCurWP].x - vPos.x, stInfo.vWayPoints[iCurWP].y - vPos.y, 0);

                    fDist = vDist.magnitude;
                    fWPTime = fDist / (stInfo.iSpeed/10.0f / 32.0f);

                    vVel = new Vector3(vDist.x / fWPTime, vDist.y / fWPTime, 0);
                    iCurWP = (iCurWP + 1) % stInfo.iNumWayPoints;
                    //send change in movement - m_stMsg1.iMsg |= N_MSG1_MOVEMENT;
                }
                vPos += vVel * Time.fixedDeltaTime;
            }

            //fire bullets
            if (!GameLevel.bRunReplay)
            {
                fFireTime -= Time.fixedDeltaTime;
                if (fFireTime <= 0)
                { //fire bullet, set in replay
                    if (stInfo.iFireInterval == -1) stInfo.iFireInterval = 2000; //previous versions random time becomes 2 sec
                    fFireTime = stInfo.iFireInterval / 1000.0f;

                    if (InFireRange())
                    {
                        oAudioSource.PlayOneShot(oClipFire);

                        float fDirection = stInfo.iAngle;
                        if (SENEMY_RANDOMBULLETANGLE[stInfo.iEnemyType])
                        {
                            fDirection = Random.value * 359;
                        }

                        for (int i = 0; i < SENEMY_NUMBULLETS[stInfo.iEnemyType]; i++)
                        {
                            Vector2 vFirePoint = vPos;
                            if (stInfo.iEnemyType == 5 && i == 1) vFirePoint += SENEMY5_FIREPOINT2[stInfo.iAngle / 90] / 10.0f;
                            else vFirePoint += SENEMY_FIREPOINT[stInfo.iEnemyType, stInfo.iAngle / 90] / 10.0f;
                            CreateBullet(vFirePoint, fDirection + SENEMY_BULLETANGLE[stInfo.iEnemyType, i], Bullet.BULLETBASEVEL / 2, i);
                        }
                    }
                }
            }
        }

        //set transform
        transform.position = vPos;
    }

    void Update()
    {
        if (!bInited) return;

        //only enemy 4 rotates
        if (stInfo.iEnemyType == 4)
        {
            enemy4_part.transform.Rotate(Vector3.forward, 50.0f * Time.deltaTime);
        }
    }

    void CreateBullet(Vector2 i_vFirePoint, float i_fDirection, float i_fSpeed, int iBulletNum)
    {
        S_BulletInfo stBulletInfo;

        float fSin = Mathf.Sin(i_fDirection * (Mathf.PI / 180.0f));
        float fCos = Mathf.Cos(i_fDirection * (Mathf.PI / 180.0f));
        stBulletInfo.vPos = i_vFirePoint; // new Vector2(vPos.x + fCos * i_fOffset, vPos.y + fSin * i_fOffset);
        stBulletInfo.vVel = new Vector2(vVel.x + fCos * i_fSpeed, vVel.y + fSin * i_fSpeed);
        stBulletInfo.fDirection = i_fDirection;

        GameObject o = Instantiate(oMap.oBulletObjBase, oMap.transform);
        o.GetComponent<Bullet>().Init(stBulletInfo, 1);

        ReplayMessage rm = new ReplayMessage();
        rm.vPos = stBulletInfo.vPos;
        rm.vVel = stBulletInfo.vVel;
        rm.fDirection = stBulletInfo.fDirection;
        rm.iID = 1;
        rm.iType = (byte)MsgType.BULLETE_NEW;
        rm.iGeneralByte1 = (byte)iBulletNum; //used for playing sound only for first bullet
        GameLevel.theReplay.Add(rm);
    }

}
