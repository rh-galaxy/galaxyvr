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
        if (Input.GetKey(KeyCode.F) || GameManager.bNoVR)
        {
            if (Input.GetKey(KeyCode.G)) fZ += Input.GetAxisRaw("Mouse X") * 3.0f;
            else fY += Input.GetAxisRaw("Mouse X") * 3.0f;
            fX -= Input.GetAxisRaw("Mouse Y") * 3.0f;
            if (Input.GetKey(KeyCode.R)) { fX = 5; fY = 0; fZ = 0; }

            transform.eulerAngles = new Vector3(fX, fY, fZ);
        }
    }
}