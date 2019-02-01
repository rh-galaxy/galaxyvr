using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    void Start()
    {

    }

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
    }
}