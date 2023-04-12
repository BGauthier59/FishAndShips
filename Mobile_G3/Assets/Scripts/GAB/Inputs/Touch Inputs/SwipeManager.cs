using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : MiniGameInput<SwipeData>
{
    [SerializeField] private Camera inputCamera;

    private bool isDraging = false;
    private Vector3 startTouch, swipeDelta;

    public bool CalculateSwipe()
    {
        // Calculate distance
        swipeDelta = Vector3.zero;
        if (isDraging)
        {
            swipeDelta = inputCamera.ScreenToWorldPoint(Input.mousePosition) - startTouch;
            swipeDelta.y = data.centralPoint.position.y;

            Debug.DrawRay(startTouch, swipeDelta, Color.green);
        }

        // Check deadzone
        if (swipeDelta.magnitude < 2) return false;

        // Check direction
        var dot = Vector3.Dot(swipeDelta.normalized, data.rightDirection);
        Reset();
        return dot >= data.minimumCollinearFactor;
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        
        if (ctx.started)
        {
            startTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
            startTouch.y = data.centralPoint.position.y;

            Debug.Log(startTouch);

            if (Vector3.Distance(startTouch, data.centralPoint.position) <= data.centralPointRadius)
            {
                isDraging = true;
            }
            else Reset();
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector3.zero;
        isDraging = false;
    }
}

[Serializable]
public struct SwipeData
{
    public Transform centralPoint;
    public float centralPointRadius;
    public Vector3 rightDirection;
    [Range(0, 1)] public float minimumCollinearFactor;
}