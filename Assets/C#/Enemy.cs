using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    S_EnemyInfo stInfo;

    MapGenerator oMap;
    public GameObject enemy4;
    public GameObject oExplosion;

    public AudioClip oClipHit;
    public AudioClip oClipExplosion;
    public AudioClip oClipFire;

    Vector3 vPos;
    Vector3 vVel;
    int iCurWP;
    float fWPTime;
    float fFireTime;

    int iNumHits = 1;
    bool bStartExplosion = false;
    float fExplosionTimer = 0.0f;

    int[] SENEMY_HITSTOKILL = { 4, 5, -1, -1, 8, 4, 4, 6 }; //-1=immortal
    int[] SENEMY_SCOREPOINTS = { 10, 20, 0, 0, 20, 15, 25, 25 };
    int[] SENEMY_NUMBULLETS = { 1, 3, 1, 3, 1, 2, 5, 8 };
    int[] SENEMY_BULLETSPEED = { 190, 160, 190, 160, 110, 90, 120, 140 };
    bool[] SENEMY_RANDOMBULLETANGLE = { false, false, false, false, true, false, false, false };
    int[,] SENEMY_BULLETANGLE = {
	    {0,0,0,0,0,0,0,0}, {-50,0,50,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {-50,0,50,0,0,0,0,0},
	    {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {-70,-35,0,35,70,0,0,0}, {-180,-135,-90,-45,0,45,90,135}};


    public void Init(S_EnemyInfo i_stInfo, MapGenerator i_oMap)
    {
        oMap = i_oMap;
        stInfo = i_stInfo;

        //set all to inactive
        gameObject.SetActive(false); //remove later
        enemy4.SetActive(false);

        //enable and init for the current enemy type
        iCurWP = 1;
        fWPTime = 0;
        fFireTime = 0;
        oExplosion.SetActive(false);
        //gameObject.SetActive(true); /*later this is true for all, and then remove below*/
        if (stInfo.iEnemyType == 4)
        {
            gameObject.SetActive(true); //remove later
            enemy4.SetActive(true);
        }
        vPos.x = stInfo.vWayPoints[0].x;
        vPos.y = stInfo.vWayPoints[0].y;
        vPos.z = 1.5f;

        iNumHits = SENEMY_HITSTOKILL[stInfo.iEnemyType];

        name = "Enemy";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        if (szOtherObject.StartsWith("BulletP"))
        {
            //player bullet, one hit
            if (iNumHits > 0)
            {
                iNumHits--;

                if (iNumHits == 0)
                {
                    //kill this enemy
                    bStartExplosion = true;

                    //play explosion sound
                    GetComponent<AudioSource>().PlayOneShot(oClipExplosion);

                    oMap.iAchieveEnemiesKilled++;
                }
                else
                {
                    GetComponent<AudioSource>().PlayOneShot(oClipHit);
                }
            }
        }

        //collision with player, done in player

        //ignore other and own enemy bullets
        // is done in the collision matrix in Edit->Project Settins
    }

    public Player thePlayer; //set in editor
    bool InFireRange()
    {
        Vector2 vPlayerPos = thePlayer.GetPosition();
        Vector2 vDist = vPlayerPos - new Vector2(vPos.x, vPos.y);
        float fDist = Mathf.Sqrt(vDist.x * vDist.x + vDist.y * vDist.y);
        return (fDist<(450/32.0f)); //~14 tiles, like in the old game
    }

    void FixedUpdate()
    {
        //////handle creation of all new enemy bullets, could be done in another place...
        if (MapGenerator.bRunReplay)
        {
            ReplayMessage rm;
            while (MapGenerator.theReplay.Get(out rm, 1))
            {
                if (rm.iType == (byte)MsgType.BULLETE_NEW)
                {
                    //part of CreateBullet()
                    S_BulletInfo stBulletInfo;
                    stBulletInfo.vPos = rm.vPos;
                    stBulletInfo.vVel = rm.vVel;
                    stBulletInfo.fDirection = rm.fDirection;
                    GameObject o = Instantiate(oMap.oBulletObjBase, oMap.transform);
                    o.GetComponent<Bullet>().Init(stBulletInfo, 1/*iOwnerID*/);

                    if(rm.iGeneralByte1==1)
                        GetComponent<AudioSource>().PlayOneShot(oClipFire);
                }
            }
        }
        //////

        //wait for explosion to finish and remove object
        if (iNumHits==0)
        {
            //start explosion
            if (bStartExplosion)
            {
                /**/enemy4.SetActive(false); //set the specific enemytype inactive later
                oExplosion.SetActive(true);
                fExplosionTimer = 0.0f;
                bStartExplosion = false;
            }

            fExplosionTimer += Time.fixedDeltaTime;
            if (fExplosionTimer > 2.0f)
            {
                oExplosion.SetActive(false);
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
                    fWPTime = fDist / (stInfo.iSpeed / 32.0f);

                    vVel = new Vector3(vDist.x / fWPTime, vDist.y / fWPTime, 0);
                    iCurWP = (iCurWP + 1) % stInfo.iNumWayPoints;
                    //send change in movement - m_stMsg1.iMsg |= N_MSG1_MOVEMENT;
                }
                vPos += vVel * Time.fixedDeltaTime;
                //SetCommonInfo(&m_stMsg1);
            }

            //fire bullets
            if (!MapGenerator.bRunReplay)
            {
                fFireTime -= Time.fixedDeltaTime;
                if (fFireTime <= 0)
                { //fire bullet, send to network
                    if (stInfo.iFireInterval == -1) stInfo.iFireInterval = 2000; //previous versions random time becomes 2 sec
                    fFireTime = stInfo.iFireInterval / 1000.0f;

                    if (InFireRange())
                    {
                        GetComponent<AudioSource>().PlayOneShot(oClipFire);

                        float fDirection = 0;
                        if (SENEMY_RANDOMBULLETANGLE[stInfo.iEnemyType])
                        {
                            fDirection = Random.value * 359;
                        }

                        for (int i = 0; i < SENEMY_NUMBULLETS[stInfo.iEnemyType]; i++)
                        {
                            //if (stInfo.iEnemyType == 5) m_pstFirePoint = &SENEMY5_FIREPOINT[2 * (int)(m_dDirection / 90) + i];
                            CreateBullet(fDirection, 0.0f, Bullet.BULLETBASEVEL / 2, i);
                            //^only valid for enemy 4, fix this!
                        }
                    }
                }
            }
        }

        //set transform
        transform.position = vPos;
    }

    private void Update()
    {
        //only enemy 4 rotates
        if (stInfo.iEnemyType == 4)
        {
            enemy4.transform.Rotate(Vector3.forward, 50.0f * Time.deltaTime);
        }
    }

    void CreateBullet(float i_fDirection, float i_fOffset, float i_fSpeed, int iBulletNum)
    {
        S_BulletInfo stBulletInfo;

        float fSin = Mathf.Sin(i_fDirection * (Mathf.PI / 180.0f));
        float fCos = Mathf.Cos(i_fDirection * (Mathf.PI / 180.0f));
        ///**/Debug.DrawLine(new Vector3(pos.x, pos.y, -4.5f), new Vector3(pos.x + fCos * i_fOffset, pos.y + fSin * i_fOffset, -4.5f), Color.black, 2.5f, false);
        stBulletInfo.vPos = new Vector2(vPos.x + fCos * i_fOffset, vPos.y + fSin * i_fOffset);
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
        MapGenerator.theReplay.Add(rm);
    }

}
