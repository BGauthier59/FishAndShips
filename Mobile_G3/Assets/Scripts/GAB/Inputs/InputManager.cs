using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    public InputMaster inputMaster;

    public override void Awake()
    {
        base.Awake();
        CreateInputMaster();
    }

    private void CreateInputMaster()
    {
        if (inputMaster != null)
        {
            Debug.LogError("Input Master has already been created");
            return;
        }
        inputMaster = new InputMaster();
    }

    public void EnableInputMaster()
    {
        inputMaster.Enable();
    }

    public void DisableInputMaster()
    {
        inputMaster.Disable();
    }

    public void EnableInputAction(InputActionMap map)
    {
        map.Enable();
    }

    public void DisableInputAction(InputActionMap map)
    {
        map.Disable();
    }
    
    public void EnableInputAction(InputAction action)
    {
        action.Enable();
    }

    public void DisableInputAction(InputAction action)
    {
        action.Disable();
    }
}
