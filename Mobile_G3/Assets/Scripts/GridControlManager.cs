using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridControlManager : MonoSingleton<GridControlManager>
{
    public bool upKeyPressed,downKeyPressed,leftKeyPressed,rightKeyPressed;
    public RectTransform[] buttonsScheme1;
    public RectTransform[] buttonsScheme2;
    public RectTransform[] buttonsScheme3;
    public GameObject[] schemes;
    private bool isActive;
    public int controlScheme;
    

    public void StartGameLoop()
    {
        isActive = true;
        ActivateScheme();
    }
    
    private void OnKeyPressed(int key)
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
        if (!isActive) return;
        
        if (ctx.started)
        {
            TapOnScreen(Input.mousePosition);
        }
    }

    void TapOnScreen(Vector2 position)
    {
        RectTransform[] currentScheme = controlScheme == 0 ? buttonsScheme1 : controlScheme == 1 ? buttonsScheme2 : buttonsScheme3;
        for (int i = 0; i < 4; i++)
        {
            Rect rectTransformed = new Rect(currentScheme[i].TransformPoint(currentScheme[i].rect.position),
                currentScheme[i].TransformVector(currentScheme[i].rect.size));
            if (rectTransformed.Contains(position))
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

    public void ActivateScheme()
    {
        controlScheme = SaveManager.instance.GetData().controls;
        
        for (int i = 0; i < schemes.Length; i++)
        {
            if(i == controlScheme) schemes[i].SetActive(true);
            else schemes[i].SetActive(false);
        }
    }
}
