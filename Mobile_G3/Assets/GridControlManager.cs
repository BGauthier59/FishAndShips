using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridControlManager : MonoSingleton<GridControlManager>
{
    public bool upKeyPressed,downKeyPressed,leftKeyPressed,rightKeyPressed;
    public RectTransform[] buttons;

    public void OnKeyPressed(int key)
    {
        switch (key)
        {
            case 0:
                upKeyPressed = true;
                break;
            case 1:
                downKeyPressed = true;
                break;
            case 2:
                leftKeyPressed = true;
                break;
            case 3:
                rightKeyPressed = true;
                break;
        }
    }
    
    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            TapOnScreen(Input.mousePosition);
        }
    }

    void TapOnScreen(Vector2 position)
    {
        for (int i = 0; i < 4; i++)
        {
            if (buttons[i].rect.Contains(position-(Vector2)buttons[i].position))
            {
                OnKeyPressed(i);
                break;
            }
        }
    }

    public void Reset()
    {
        upKeyPressed = downKeyPressed = leftKeyPressed = rightKeyPressed = false;
    }
}
