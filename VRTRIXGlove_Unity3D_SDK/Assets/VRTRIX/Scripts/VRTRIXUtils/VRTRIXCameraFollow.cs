//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Camera Moving Controlled by Keyboards & Mouse (Used in Non-VR Mode)
//
//=============================================================================
using UnityEngine;
using System.Collections;

public class VRTRIXCameraFollow : MonoBehaviour {

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float moveSpeed = 100f;      // Speed of the camera going back and forth
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;

    public float ZoomAmount = 0; //With Positive and negative values
    public float MaxToClamp = 10f;
    public float ROTSpeed = 10f;
    float rotationX = 0F;
    float rotationY = 0F;
    private bool isRotating;    // Is the camera being rotated?
    Quaternion originalRotation;
    void Start()
    {
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
        }

        // Disable movements on button release
        if (!Input.GetMouseButton(1)) isRotating = false;

        if (isRotating)
        {
            if (axes == RotationAxes.MouseXAndY)
            {
                // Read the mouse input axis
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
                transform.localRotation = originalRotation * yQuaternion;
            }
        }

        // Move the camera linearly along Z axis
        ZoomAmount += Input.GetAxis("Mouse ScrollWheel");
        ZoomAmount = Mathf.Clamp(ZoomAmount, -MaxToClamp, MaxToClamp);
        var translate = Mathf.Min(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")), MaxToClamp - Mathf.Abs(ZoomAmount));
        gameObject.transform.Translate(0, 0, translate * ROTSpeed * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")));


        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= transform.rotation * new Vector3(0, 0, moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.rotation * new Vector3(0, 0, moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position -= transform.rotation * new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += transform.rotation * new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
