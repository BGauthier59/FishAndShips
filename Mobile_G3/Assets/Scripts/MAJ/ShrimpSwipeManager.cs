using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShrimpSwipeManager : MiniGameInput<ShrimpSwipeSetupData>
{
    public Vector3 startTouch;
    public bool isDragging;
    

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = Input.mousePosition;
            isDragging = true;
        }
        else if (ctx.canceled)
        {
            isDragging = false;
        }
    }

    public override void Disable()
    {
        base.Disable();
        isDragging = false;
        startTouch = Vector3.zero;
    }
}

[Serializable]
public struct ShrimpSwipeSetupData
{
    
}