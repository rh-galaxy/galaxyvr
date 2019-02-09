using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject oPlayer;
    public GameLevel oMap;

    private Vector3 camOffset;
    private Vector3 mapSize;

    // Start is called before the first frame update
    void Start()
    {
        camOffset = new Vector3(0,3,-10);
        mapSize = oMap.GetMapSize();
    }

    // Update is called once per frame
    float fX = 0, fY = 0, fZ = 0;
    void LateUpdate()
    {
        //emulate headset movement
        if (Input.GetKey(KeyCode.F))
        {
            if (Input.GetKey(KeyCode.G)) fZ = Input.GetAxisRaw("Mouse X");
            else fY = Input.GetAxisRaw("Mouse X");
            fX = -Input.GetAxisRaw("Mouse Y");
            transform.Rotate(new Vector3(fX * 3.0f, fY * 3.0f, fZ * 3.0f));
        }

        Vector3 v = oPlayer.transform.position + camOffset;
        float fLeftLimit = -(mapSize.x / 2.0f) + 5;
        float fRightLimit = (mapSize.x / 2.0f) - 5;
        if (v.x < fLeftLimit) v.x = fLeftLimit;
        if (v.x > fRightLimit) v.x = fRightLimit;
        float fTopLimit = (mapSize.y / 2.0f) - 3;
        float fBottomLimit = -(mapSize.y / 2.0f) + 10;
        if (v.y < fBottomLimit) v.y = fBottomLimit;
        if (v.y > fTopLimit) v.y = fTopLimit;
        transform.position = v;
    }
}
