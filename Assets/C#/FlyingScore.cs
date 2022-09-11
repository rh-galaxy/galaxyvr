using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FlyingScore

//class to handle the flying text when the player gets score.

public struct S_FlyingScoreInfo
{
    public string szScore;
    public Vector3 vPos;
    public Vector3 vVel;
}

////////////////////////////////////////////////////////////////////////////////

public class FlyingScore : MonoBehaviour
{
    float fTotAlive;
    S_FlyingScoreInfo stFlyingScore;

    Rigidbody rb;

    public void Init(S_FlyingScoreInfo i_stFlyingScoreInfo)
    {
        stFlyingScore = i_stFlyingScoreInfo;
        fTotAlive = 0.0f;

        gameObject.SetActive(true);
        TextMesh t = gameObject.GetComponent<TextMesh>();
        t.text = stFlyingScore.szScore;

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
