using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridControlManager : MonoSingleton<GridControlManager>
{
    public bool upKeyPressed,downKeyPressed,leftKeyPressed,rightKeyPressed;

    public void OnKeyPressed(int key)
    {
        Debug.Log("Oui");
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

    public void Reset()
    {
        upKeyPressed = downKeyPressed = leftKeyPressed = rightKeyPressed = false;
    }
}
