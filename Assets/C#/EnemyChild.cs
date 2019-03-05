using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChild : MonoBehaviour
{
    void Start()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string szOtherObject = collision.collider.gameObject.name;

        if (szOtherObject.StartsWith("BulletP"))
        {
            transform.parent.GetComponent<Enemy>().HitByBullet();
        }
    }

    void Update()
    {
    }
}
