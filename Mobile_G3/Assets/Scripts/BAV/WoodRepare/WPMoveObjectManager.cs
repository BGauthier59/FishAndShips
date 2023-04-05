using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WPMoveObjectManager : MonoBehaviour
{
    private Vector2 firstInputPos, secondInputPos;
    private Vector3 lastDragPos;
    public bool isMovingAnObject;
    public Camera cam;
    [SerializeField] private LayerMask woodLayer;
    public float distanceRaycast = 10f;
    public TextMeshPro debugPos1;

    [UsedImplicitly]
    public void FirstInputToTap(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isMovingAnObject = true;
        }
        
        if(ctx.canceled)
        {
            isMovingAnObject = false;
        }
    }
    
    [UsedImplicitly]
    public void UpdateInputPosition(InputAction.CallbackContext ctx)
    {
        firstInputPos = ctx.ReadValue<Vector2>();
    }
    
    [UsedImplicitly]
    public void UpdateInputSecondPosition(InputAction.CallbackContext ctx)
    {
        secondInputPos = ctx.ReadValue<Vector2>();
    }
    
    public void Awake()
    {
        AndroidUtils.CreateCurrentActivity();
    }

    public void Update()
    {
        debugPos1.text = "Pos" + firstInputPos;
        if (isMovingAnObject)
        {
            Ray ray = cam.ScreenPointToRay(firstInputPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distanceRaycast,woodLayer))
            {
                hit.transform.gameObject.transform.localPosition = new Vector3(hit.point.x, hit.point.y,0);
            }
        }
    }
}
