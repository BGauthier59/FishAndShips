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
    public bool isDraging;
    

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        //if (!isActive) return;

        if (ctx.started)
        {
            startTouch = Input.mousePosition;
            Debug.Log("SwipeStarted at " + startTouch);
            isDraging = true;
        }
        else if (ctx.canceled)
        {
            isDraging = false;
        }
    }

    
}

[Serializable]
public struct ShrimpSwipeSetupData
{
    
}