using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position += new Vector3(0, speed, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position += new Vector3(0, -speed, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.position += new Vector3(-speed, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.position += new Vector3(speed, 0, 0);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
        }
    }
}
