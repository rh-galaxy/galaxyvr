using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bullet

//class to handle bullets. (all existing bullets in a game are hold in GameLevel)
// - collision detections against the map and objects always destroys the bullet.
// - or it is removed after a timeout

public struct S_FlyingScoreInfo
{
    public int iScore;
    public Vector3 vPos;
    public Vector3 vVel;
}

////////////////////////////////////////////////////////////////////////////////

public class FlyingScore : MonoBehaviour
{
    float fTotAlive;
    S_FlyingScoreInfo stFlyingScore;

    Rigidbody rb;

    //public GameObject oTextScore;

    public void Init(S_FlyingScoreInfo i_stFlyingScoreInfo)
    {
        stFlyingScore = i_stFlyingScoreInfo;
        fTotAlive = 0.0f;

        gameObject.SetActive(true);
        TextMesh t = gameObject.GetComponent<TextMesh>();
        t.text = stFlyingScore.iScore.ToString();

        rb = gameObject.GetComponent<Rigidbody>();
        rb.transform.position = stFlyingScore.vPos;
        rb.velocity = stFlyingScore.vVel;
    }

    void FixedUpdate()
    {
        fTotAlive += Time.fixedDeltaTime;
        if (fTotAlive > 2.0f)
        {
            //this removes the object from its parent and from existance
            Destroy(gameObject);
        }
    }
}
