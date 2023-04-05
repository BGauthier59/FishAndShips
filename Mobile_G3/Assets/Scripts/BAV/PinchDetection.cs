using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchDetection : MonoBehaviour
{
    [SerializeField] private float cameraSpeed = 4f;
    private TouchControls _controls;
    private Coroutine zoomCoroutine;
    public Transform cameraTransform;
    // Start is called before the first frame update
    void Awake()
    {
        _controls = new TouchControls();
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Start()
    {
        _controls.PlayerTouchScreen.SecondaryTouchContact.started += _ => ZoomStart();
        _controls.PlayerTouchScreen.SecondaryTouchContact.canceled += _ => ZoomEnd();
    }
    
    private void ZoomStart()
    {
        zoomCoroutine = StartCoroutine(ZoomDetection());
    }
    
    private void ZoomEnd()
    {
        StopCoroutine(zoomCoroutine);
    }


    IEnumerator ZoomDetection()
    {
        float previousDistance = 0f, distance = 0f;
        while (true)
        {
            distance = Vector2.Distance(_controls.PlayerTouchScreen.PrimaryFingerPosition.ReadValue<Vector2>(),
                _controls.PlayerTouchScreen.SecondaryFingerPosition.ReadValue<Vector2>());

            if (distance > previousDistance)
            {
                Vector3 targetPosition = cameraTransform.position;
                targetPosition.z -= 1;
                cameraTransform.position = Vector3.Slerp(
                    cameraTransform.position, 
                    targetPosition, 
                    Time.deltaTime * cameraSpeed);
            }
            else if (distance < previousDistance)
            {
                Vector3 targetPosition = cameraTransform.position;
                targetPosition.z += 1;
                cameraTransform.position = Vector3.Slerp(
                    cameraTransform.position, 
                    targetPosition, 
                    Time.deltaTime * cameraSpeed);
            }

            previousDistance = distance;
            yield return null;
        }
    }
}
