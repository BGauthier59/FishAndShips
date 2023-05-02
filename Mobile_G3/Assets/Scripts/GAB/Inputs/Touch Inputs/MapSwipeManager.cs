using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapSwipeManager : MiniGameInput<MapSwipeData>
{
    private bool isDraging = false;
    private Vector3 currentTouch, lastTouch;
    
    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        
        if (ctx.started)
        {
            currentTouch = Input.mousePosition;
            isDraging = true;
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    public override void Enable(MapSwipeData setupData)
    {
        base.Enable(setupData);
    }

    private void Reset()
    {
        currentTouch = lastTouch = Vector3.zero;
        isDraging = false;
    }

    public Vector3 CalculateMoveDelta()
    {
        if(!isDraging) return Vector3.zero;

        lastTouch = currentTouch;
        currentTouch = Input.mousePosition;
        
        var dir = (currentTouch - lastTouch) * (Time.deltaTime * data.sensitivity);
        dir.z = dir.y;
        dir.y = 0;
        
        return dir;
    }
}
[Serializable]
public struct MapSwipeData
{
    public float sensitivity;
}