using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircularSwipeManager : MonoBehaviour
{
    private bool isActive;

    private Transform centralPoint;
    private SpriteRenderer rotatingPointRd;
    private Transform availableZone;
    private Transform unavailableCenterZone;
    private TMP_Text circleCountText;
    private TMP_Text timerText;

    private float2 minMaxSqrMagnitude;
    private Vector2 startTouch, currentTouch;
    private Vector2 startVector, currentVector;
    private bool isDraging;
    private float currentAngle;
    private int circleCount;

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startVector = startTouch - (Vector2)centralPoint.position;

            if (startVector.sqrMagnitude < minMaxSqrMagnitude.x ||
                startVector.sqrMagnitude > minMaxSqrMagnitude.y) return;

            isDraging = true;
            startVector.Normalize();
            rotatingPointRd.transform.localPosition = startTouch;
            rotatingPointRd.enabled = true;
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        isDraging = false;
        startTouch = currentTouch = startVector = currentVector = Vector2.zero;
        currentAngle = 0;
        rotatingPointRd.enabled = false;
    }

    public void Enable(CircularSwipeSetupData data)
    {
        centralPoint = data.centralPoint;
        rotatingPointRd = data.rotatingPointRd;
        availableZone = data.availableZone;
        unavailableCenterZone = data.unavailableCenterZone;
        circleCountText = data.circleCountText;

        minMaxSqrMagnitude.x = math.pow(data.minMaxMagnitude.x, 2);
        minMaxSqrMagnitude.y = math.pow(data.minMaxMagnitude.y, 2);

        availableZone.localScale = new Vector3(data.minMaxMagnitude.y * 2, data.minMaxMagnitude.y * 2, 1);
        unavailableCenterZone.localScale = new Vector3(data.minMaxMagnitude.x * 2, data.minMaxMagnitude.x * 2, 1);

        isActive = true;

        circleCount = 0;
        Reset();
    }
    
    public void Disable()
    {
        isActive = false;
        Reset();
    }

    public bool CalculateCircularSwipe(int countToWin)
    {
        if (!isDraging) return false;

        currentTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentVector = currentTouch - (Vector2)centralPoint.position;

        if (currentVector.sqrMagnitude < minMaxSqrMagnitude.x ||
            currentVector.sqrMagnitude > minMaxSqrMagnitude.y) return false;

        currentVector.Normalize();

        var angleGap = Vector2.Angle(startVector, currentVector);

        var last2float3 = new float3(startVector.x, startVector.y, 0);
        var current2float3 = new float3(currentVector.x, currentVector.y, 0);
        var cross = math.cross(last2float3, current2float3);

        if (cross.z > 0) currentAngle += angleGap;
        else currentAngle -= angleGap;

        if (currentAngle > 360)
        {
            circleCount--;
            currentAngle -= 360;
        }
        else if (currentAngle < 0)
        {
            circleCount++;
            currentAngle += 360;
        }

        circleCountText.text = circleCount.ToString();

        startVector = currentVector;
        centralPoint.SetEulerAnglesZ(currentAngle);

        return circleCount >= countToWin;
    }
}

[Serializable]
public struct CircularSwipeSetupData
{
    public Transform centralPoint;
    public float2 minMaxMagnitude;
    public SpriteRenderer rotatingPointRd;
    public Transform availableZone;
    public Transform unavailableCenterZone;
    public TMP_Text circleCountText;
}