using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    private static InputMaster _inputMaster;
    
    public static void CreateInputMaster()
    {
        if (_inputMaster != null)
        {
            Debug.LogError("Input Master has already been created");
            return;
        }
        _inputMaster = new InputMaster();
    }

    public static void EnableInputMaster()
    {
        _inputMaster.Enable();
    }

    public static void DisableInputMaster()
    {
        _inputMaster.Disable();
    }

    public static void EnableInputAction(InputAction action)
    {
        action.Enable();
    }

    public static void DisableInputAction(InputAction action)
    {
        action.Disable();
    }
}
