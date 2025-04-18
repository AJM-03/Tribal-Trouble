using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    private CinemachineFreeLook cam;


    void Start()
    {
        cam = GetComponent<CinemachineFreeLook>();
        transform.parent.GetComponent<Camera>().farClipPlane = 100;
    }


    public void RecenterButtonPressed(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            cam.m_RecenterToTargetHeading.m_enabled = true;
        }

        else if (value.canceled)
            cam.m_RecenterToTargetHeading.m_enabled = false;
    }


    private void OnApplicationFocus(bool focus)  // Lock Mouse Cursor
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
