using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInput : MonoBehaviour
{
    public float mouseSensitivity;
    public float panSpeed;
    float yaw;
    float pitch;
    Vector2 pitchLimit = new Vector2(-45, 45);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, pitchLimit.x, pitchLimit.y); ;

        Vector3 targetRotation = new Vector3(pitch, yaw);
        transform.eulerAngles = targetRotation;

        if (Input.GetKey("w"))
        {
            transform.Translate(transform.forward * panSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey("s"))
        {
            transform.Translate(-transform.forward * panSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey("d"))
        {
            transform.Translate(transform.right * panSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey("a"))
        {
            transform.Translate(-transform.right * panSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey("q"))
        {
            transform.Translate(transform.up * panSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey("e"))
        {
            transform.Translate(-transform.up * panSpeed * Time.deltaTime, Space.World);
        }
    }
}
