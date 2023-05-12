using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : MiniGameInput<SwipeData>
{
    [SerializeField] private Camera inputCamera;

    private bool isDraging = false;
    private Vector3 startTouch, swipeDelta;
    
    private float swipeMagnitude = 125;
    private float realSwipeMagnitude;

    public bool CalculateSwipe()
    {
        if (!isActive) return false;
        
        // Calculate distance
        swipeDelta = Vector3.zero;
        if (isDraging)
        {
            swipeDelta = Input.mousePosition - startTouch;
        }

        // Check deadzone
        if (swipeDelta.magnitude < realSwipeMagnitude) return false;

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
            startTouch = Input.mousePosition;
            
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

    public override void Enable(SwipeData setupData)
    {
        base.Enable(setupData);
        float scale = WorkshopManager.instance.GetCanvasFactor();
        setupData.centralPointRadius *= scale;
        realSwipeMagnitude = swipeMagnitude * scale;
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