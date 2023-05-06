using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReparationDragAndDropManager : MiniGameInput<ReparationDragAndDropManagerData>
{
    [SerializeField] private Camera inputCamera;

    private Vector3 startTouch;
    private Vector3 currentTouch;
    private Vector3 initPlankPos;
    private bool isDraging;
    private int currentPlankIndex;

    private Transform currentPlank;

    public Action OnSetPlank;

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = currentTouch = Input.mousePosition;

            if (Vector3.Distance(startTouch, data.plankStartPoint.position) > data.startRadius) return;

            isDraging = true;
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        if (isDraging)
        {
            if (Vector3.Distance(currentTouch, data.plankAvailableEndPoint.position) > data.endRadius)
            {
                currentPlank.position = initPlankPos;
            }
            else
            {
                OnSetPlank?.Invoke();
                currentPlankIndex++;
                if (currentPlankIndex == data.draggablePlanks.Length)
                {
                    currentPlankIndex = 0;
                }

                currentPlank = data.draggablePlanks[currentPlankIndex];
                RefreshPlanks();
            }
        }

        startTouch = currentTouch = Vector3.zero;
        isDraging = false;
    }

    private Ray ray;
    private float enter;
    public void CalculateCurrentPlankPosition()
    {
        if (!isDraging) return;

        currentTouch = Input.mousePosition;

        ray = inputCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out enter))
        {
            data.draggablePlanks[currentPlankIndex].position = ray.GetPoint(enter);
        }
    }

    private Plane plane;

    public override void Enable(ReparationDragAndDropManagerData setupData)
    {
        base.Enable(setupData);
        plane = new Plane(-WorkshopManager.instance.miniGameEnvironmentCamera.forward, data.planeOrigin.position);
        currentPlankIndex = UnityEngine.Random.Range(0, data.draggablePlanks.Length);
        currentPlank = data.draggablePlanks[currentPlankIndex];
        initPlankPos = currentPlank.position;
        RefreshPlanks();
    }

    private void RefreshPlanks()
    {
        foreach (var plank in data.draggablePlanks)
        {
            if (plank == currentPlank)
            {
                plank.gameObject.SetActive(true);
                return;
            }
        }
    }
}

[Serializable]
public struct ReparationDragAndDropManagerData
{
    public Transform planeOrigin;

    public Transform plankStartPoint;
    public float startRadius;

    public Transform plankAvailableEndPoint;
    public float endRadius;

    public float plankDelay;
    public Transform[] draggablePlanks;
}