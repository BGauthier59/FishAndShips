using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : MonoBehaviour
{
    private bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    private bool isDraging = false;
    private Vector2 startTouch, swipeDelta;

    private InputMaster inputs;

    private void Start()
    {
        inputs = new InputMaster();
        inputs.Enable();
    }

    void Update()
    {
        Swipe();
    }

    private void Swipe()
    {
        tap = swipeLeft = swipeRight = swipeUp = swipeDown = false;

        // Calculate distance
        swipeDelta = Vector2.zero;
        if (isDraging)
        {
            if (Input.touches.Length != 0)
            {
                swipeDelta = Input.touches[0].position - startTouch;
            }
            else if (Input.GetMouseButton(0))
            {
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
            }
        }

        // Check deadzone
        if (swipeDelta.magnitude > 125)
        {
            var x = swipeDelta.x;
            var y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // Left or Right
                if (x < 0) swipeLeft = true;
                else swipeRight = true;
            }
            else
            {
                // Up or Down
                if (y < 0) swipeDown = true;
                else swipeUp = true;
            }

            Reset();
        }
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            tap = isDraging = true;
            startTouch = Input.mousePosition;
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDraging = false;
    }

    public bool Tap
    {
        get { return tap; }
    }

    public Vector2 SwipeDelta
    {
        get { return swipeDelta; }
    }

    public bool SwipeLeft
    {
        get { return swipeLeft; }
    }

    public bool SwipeRight
    {
        get { return swipeRight; }
    }

    public bool SwipeUp
    {
        get { return swipeUp; }
    }

    public bool SwipeDown
    {
        get { return swipeDown; }
    }
}