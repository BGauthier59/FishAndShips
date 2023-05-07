using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class RudderCircularSwipeManager : MiniGameInput<RudderCircularSwipeSetupData>
{
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
            startTouch = Input.mousePosition;
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
        canBeClamped = true;
        startTouch = currentTouch = startVector = currentVector = Vector3.zero;
        calculatedAngle = calculatedAngleAfterClamping = 0;
    }

    public override void Enable(RudderCircularSwipeSetupData setupData)
    {
        base.Enable(setupData);

        float scale = WorkshopManager.instance.GetCanvasFactor();

        minMaxSqrMagnitude.x = math.pow(setupData.minMaxMagnitude.x * scale, 2);
        minMaxSqrMagnitude.y = math.pow(setupData.minMaxMagnitude.y * scale, 2);
        Reset();
    }

    public override void Disable()
    {
        base.Disable();
        Reset();
    }

    private float calculatedAngle;
    private float calculatedAngleAfterClamping;
    private bool clampedToZero;
    private bool canBeClamped;

    public (float eulerAngles, float degrees)? CalculateCircularSwipe()
    {
        if (!isDraging) return null;

        currentTouch = Input.mousePosition;
        currentVector = currentTouch - data.centralPoint.position;

        if (currentVector.sqrMagnitude < minMaxSqrMagnitude.x ||
            currentVector.sqrMagnitude > minMaxSqrMagnitude.y) return null;

        currentVector.Normalize();

        var angleGap = Vector3.Angle(startVector, currentVector);

        var crossGap = Vector3.Cross(startVector, currentVector);
        startVector = currentVector;

        var leftSideVector = data.leftSide.position - data.rudder.position;
        var dot = Vector3.Dot(Vector3.up, leftSideVector);
        var degreesWithLeftSide = Vector3.Angle(Vector3.left, leftSideVector);
        
        if (dot < 0) degreesWithLeftSide = -degreesWithLeftSide;
        
        if (crossGap.z < 0) angleGap = -angleGap;
        calculatedAngle += angleGap;

        if (data.rudder.eulerAngles.z > data.maxRotationDegree &&
            data.rudder.eulerAngles.z < 360 - data.maxRotationDegree)
        {
            if (data.rudder.eulerAngles.z > 180)
            {
                data.rudder.eulerAngles = Vector3.forward * (360 - data.maxRotationDegree);
                return null;
            }

            data.rudder.eulerAngles = Vector3.forward * data.maxRotationDegree;
            return null;
        }

        if (math.abs(calculatedAngle) > data.minimumAngleToRotate) return (angleGap, degreesWithLeftSide);
        return null;
    }
}

[Serializable]
public struct RudderCircularSwipeSetupData
{
    public Transform centralPoint;
    public float2 minMaxMagnitude;

    public Transform rudder;
    public Transform leftSide;

    public float maxRotationDegree;

    public float minimumAngleToRotate;
}