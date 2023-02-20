using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircularSwipeManager : MonoBehaviour
{
    [SerializeField] private Transform centralPoint;
    [SerializeField] private SpriteRenderer rotatingPointRd;

    [SerializeField] private float2 minMaxMagnitude;


    [SerializeField] private Transform availableZone;
    [SerializeField] private Transform unavailableCenterZone;
    private float2 minMaxSqrMagnitude;

    private Vector2 startTouch, currentTouch;
    private Vector2 startVector, currentVector;
    private bool isDraging;

    private float currentAngle;
    private int circleCount;

    private void OnValidate()
    {
        minMaxSqrMagnitude.x = math.pow(minMaxMagnitude.x, 2);
        minMaxSqrMagnitude.y = math.pow(minMaxMagnitude.y, 2);

        availableZone.localScale = new Vector3(minMaxMagnitude.y * 2, minMaxMagnitude.y * 2, 1);
        unavailableCenterZone.localScale = new Vector3(minMaxMagnitude.x * 2, minMaxMagnitude.x * 2, 1);
    }

    private void Start()
    {
        OnValidate();
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startVector = startTouch - (Vector2) centralPoint.position;

            Debug.Log(startVector.sqrMagnitude);
            if (startVector.sqrMagnitude < minMaxSqrMagnitude.x ||
                startVector.sqrMagnitude > minMaxSqrMagnitude.y) return;

            isDraging = true;
            startVector.Normalize();
            rotatingPointRd.transform.localPosition = startTouch;
            rotatingPointRd.enabled = true;
        }
        else if (ctx.canceled)
        {
            isDraging = false;
            startTouch = currentTouch = startVector = currentVector = Vector2.zero;
            currentAngle = 0;
            rotatingPointRd.enabled = false;
        }
    }

    private void Update()
    {
        Debug.DrawRay(centralPoint.position, startVector, Color.green);
        if (isDraging)
        {
            currentTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentVector = currentTouch - (Vector2) centralPoint.position;

            if (currentVector.sqrMagnitude < minMaxSqrMagnitude.x ||
                currentVector.sqrMagnitude > minMaxSqrMagnitude.y) return;

            currentVector.Normalize();

            var angleGap = Vector2.Angle(startVector, currentVector);

            var last2float3 = new float3(startVector.x, startVector.y, 0);
            var current2float3 = new float3(currentVector.x, currentVector.y, 0);
            var cross = math.cross(last2float3, current2float3);

            if (cross.z > 0) currentAngle += angleGap;
            else currentAngle -= angleGap;
            
            if (currentAngle > 360)
            {
                circleCount++;
                currentAngle -= 360;
            }
            else if (currentAngle < 0 && circleCount != 0)
            {
                circleCount--;
                currentAngle += 360;
            }
            else if (currentAngle < 0 && circleCount == 0)
            {
                currentAngle += 360;
            }

            startVector = currentVector;
            centralPoint.SetEulerAnglesZ(currentAngle);
        }

        Debug.DrawRay(centralPoint.position, currentVector, Color.yellow);
    }
}