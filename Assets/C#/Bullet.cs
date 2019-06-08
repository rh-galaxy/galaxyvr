using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bullet

//class to handle bullets. (all existing bullets in a game are hold in GameLevel)
// - collision detections against the map and objects always destroys the bullet.
// - or it is removed after a timeout

public struct S_BulletInfo
{
    public Vector2 vPos;
    public Vector2 vVel;
    public float fDirection;
}

////////////////////////////////////////////////////////////////////////////////

public class Bullet : MonoBehaviour
{
    public const float BULLETBASEVEL = 9.0f;
    const float BULLETLIFETIME = 8.0f;

    static int iUniqeValBase = 0;

    int iBulletUniqeVal;
    int iOwnerID;
    float fTotAlive;
    S_BulletInfo stBullet;

    Rigidbody2D rb;

    public ParticleSystem oBulletHit;
    public GameObject oCube;

    public void Init(S_BulletInfo i_stBulletInfo, int i_iOwnerID)
    {
        iBulletUniqeVal = iUniqeValBase++;
        iOwnerID = i_iOwnerID;
        stBullet = i_stBulletInfo;

        fTotAlive = 0.0f;

        gameObject.SetActive(true);

        rb = GetComponent<Rigidbody2D>();
        rb.transform.position = stBullet.vPos;
        rb.velocity = stBullet.vVel;

        if (iOwnerID == 0)
        {
            name = "BulletP" + iBulletUniqeVal.ToString(); //player bullet
            gameObject.layer = 11; //set it to BulletP layer to be able to ignore colliding with other player bullets
        }
        else
        {
            name = "BulletE" + iBulletUniqeVal.ToString(); //enemy bullet
            gameObject.layer = 10; //set it to BulletE layer to be able to ignore colliding with enemies
        }
    }

    public int GetUId()
    {
        return iBulletUniqeVal;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //collision.collider.name

        //if hit player and not coming from the player
        //hit player, handled in Player OnCollisionEnter2D

        //if hit enemy and is coming from the player
        //hit enemy, handled in Enemy OnCollisionEnter2D

        //if hit map or other bullet
        //nothing

        //if hit door button and is coming from the player
        if (collision.collider.name.StartsWith("Knapp"))
        {
            if(name.StartsWith("BulletP"))
            {
                char[] szName = collision.collider.name.ToCharArray();
                string szId = new string(szName, 5, szName.Length - 5);
                int iDoorId = int.Parse(szId);

                GameLevel.theMap.DoorToggleOpenClose(iDoorId);
            }
        }

        //always remove the bullet
        //this removes the object from its parent and from existance
        //Destroy(gameObject);

        //new, play particle effect, then destroy object
        rb.simulated = false;
        oCube.SetActive(false);
        oBulletHit.Play();
        Destroy(gameObject, 0.7f);
        if(fTotAlive+0.75 > BULLETLIFETIME) Destroy(gameObject); //act on special case where lifetime will not last 0.75 S, delete now
    }

    void FixedUpdate()
    {
        fTotAlive += Time.fixedDeltaTime;
        if (fTotAlive > BULLETLIFETIME)
        {
            //this removes the object from its parent and from existance
            Destroy(gameObject);
        }
    }
}
