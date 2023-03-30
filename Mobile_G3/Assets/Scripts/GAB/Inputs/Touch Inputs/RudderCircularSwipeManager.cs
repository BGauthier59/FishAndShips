using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RudderCircularSwipeManager : MiniGameInput<RudderCircularSwipeSetupData>
{
    [SerializeField] private Camera inputCamera;

    private float2 minMaxSqrMagnitude;
    private Vector3 startTouch, currentTouch;
    private Vector3 startVector, currentVector;
    private bool isDraging;

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
            startTouch.y = data.centralPoint.position.y;
            Debug.Log(startTouch.ToString());
            startVector = startTouch - data.centralPoint.position;

            if (startVector.sqrMagnitude < minMaxSqrMagnitude.x ||
                startVector.sqrMagnitude > minMaxSqrMagnitude.y) return;

            isDraging = true;
            startVector.Normalize();
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        isDraging = false;
        startTouch = currentTouch = startVector = currentVector = Vector3.zero;
        calculatedAngle = 0;
    }

    public override void Enable(RudderCircularSwipeSetupData data)
    {
        base.Enable(data);

        minMaxSqrMagnitude.x = math.pow(data.minMaxMagnitude.x, 2);
        minMaxSqrMagnitude.y = math.pow(data.minMaxMagnitude.y, 2);

        data.availableZone.localScale = new Vector3(data.minMaxMagnitude.y * 2, data.minMaxMagnitude.y * 2, 1);
        data.unavailableCenterZone.localScale = new Vector3(data.minMaxMagnitude.x * 2, data.minMaxMagnitude.x * 2, 1);
        Reset();
    }

    public override void Disable()
    {
        base.Disable();
        Reset();
    }

    private float calculatedAngle;

    public float? CalculateCircularSwipe()
    {
        if (!isDraging) return null;

        currentTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        currentTouch.y = data.centralPoint.position.y;
        currentVector = currentTouch - data.centralPoint.position;

        if (currentVector.sqrMagnitude < minMaxSqrMagnitude.x ||
            currentVector.sqrMagnitude > minMaxSqrMagnitude.y) return null;

        currentVector.Normalize();

        var angleGap = Vector3.Angle(startVector, currentVector);
        //var zeroAngle = Vector3.Angle(Vector3.forward, currentVector);
        Debug.DrawRay(data.centralPoint.position, Vector3.forward, Color.red);
        Debug.DrawRay(data.centralPoint.position, currentVector, Color.blue);

        var crossGap = Vector3.Cross(startVector, currentVector);
        
        startVector = currentVector;
        
        var leftSideVector = data.leftSide.position - data.rudder.position;
        var leftSideCross = Vector3.Cross(Vector3.up, leftSideVector);
        var degrees = Vector3.Angle(Vector3.up, leftSideVector);
        
        // Todo - clamp à 0
        
        /*
        if (zeroAngle < data.clampToZeroAngleGap)
        {
            Debug.LogWarning("Clamped to zero!");
            data.rudder.rotation = Quaternion.identity;
            return null;
        }
        */
        
        if (leftSideCross.z > 0) degrees = -degrees;
        
        if (crossGap.y > 0) angleGap = -angleGap;
        calculatedAngle += angleGap;
        
        // Todo - rendre le clamp propre
        
        if (crossGap.y < 0 && angleGap > 0 && leftSideCross.z < 0 && degrees < 90 + data.maxRotationDegree)
        {
            data.rudder.rotation = Quaternion.Euler(Vector3.forward * (data.maxRotationDegree));
            Reset();
            return null;
        }

        if (crossGap.y > 0 && angleGap < 0 && leftSideCross.z < 0 && degrees > 90 - data.maxRotationDegree)
        {
            data.rudder.rotation = Quaternion.Euler(Vector3.forward * (360 - data.maxRotationDegree));
            Reset();
            return null;
        }
        
        if (math.abs(calculatedAngle) > data.minimumAngleToRotate) return angleGap;
        return null;
    }
}

[Serializable]
public struct RudderCircularSwipeSetupData
{
    public Transform centralPoint;
    public float2 minMaxMagnitude;
    public Transform availableZone;
    public Transform unavailableCenterZone;

    public Transform rudder;
    public Transform leftSide;

    public float maxRotationDegree;

    public float minimumAngleToRotate;

    public float clampToZeroAngleGap;
    //public int countToWin;
}