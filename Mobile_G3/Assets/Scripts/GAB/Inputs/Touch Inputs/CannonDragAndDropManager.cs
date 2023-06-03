using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonDragAndDropManager : MiniGameInput<CannonDragAndDropData>
{
    [SerializeField] private Camera inputCamera;

    private Vector3 startTouch;
    private Vector3 currentTouch;
    private bool isDraging;

    public Action OnBulletOnTargetPoint;

    public override void Enable(CannonDragAndDropData setupData)
    {
        base.Enable(setupData);
        plane = new Plane(-WorkshopManager.instance.miniGameEnvironmentCamera.forward, data.planeOrigin.position);
        data.draggableItem.position = Vector3.zero;
        data.draggableItem.gameObject.SetActive(false);
        float scale = WorkshopManager.instance.GetCanvasFactor();
        data.endPointRadius *= scale;
        data.startPointRadius *= scale;
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = Input.mousePosition;
            currentTouch = startTouch;

            if (Vector3.Distance(startTouch, data.startPoint.position) <= data.startPointRadius)
            {
                isDraging = true;
                data.draggableItem.position = startTouch;
                data.draggableItem.gameObject.SetActive(true);
                if (data.type == CannonDragAndDropData.MiniGameType.Shoot)
                {
                    OnMatchstickFireStart?.Invoke();
                    data.matchstickOnTable.SetActive(false);
                }
            }
            else Reset();
        }
        else if (ctx.canceled)
        {
            Reset();
        }
    }

    private void Reset()
    {
        startTouch = currentTouch = lastTouch = Vector3.zero;
        isDraging = false;
        timer = 0;
        data.draggableItem.gameObject.SetActive(false);
        data.draggableItem.position = Vector3.zero;

        if (data.type == CannonDragAndDropData.MiniGameType.Shoot)
        {
            OnMatchstickFireStop?.Invoke();
            data.matchstickOnTable.SetActive(true);
        }

        if (data.type == CannonDragAndDropData.MiniGameType.Load &&
            Vector3.Distance(currentBulletPosScreenSpace, data.endPoint.position) < data.endPointRadius)
        {
            OnBulletOnTargetPoint?.Invoke();
        }
    }

    private Vector3 currentBulletPosScreenSpace;
    private Plane plane;

    public Action OnMatchstickFireStop;
    public Action OnMatchstickFireStart;

    public void CalculateBulletPosition()
    {
        if (!isActive)
        {
            Debug.LogError("Should not be calculated.");
            return;
        }
        
        if (!isDraging) return;
        
        lastTouch = currentTouch;
        currentTouch = Input.mousePosition;

        currentBulletPosScreenSpace = Vector3.Lerp(lastTouch, currentTouch, Time.deltaTime * data.lerpSpeed);
        
        data.draggableItem.position = inputCamera.ScreenToWorldPoint(currentBulletPosScreenSpace);
        
        Ray ray = inputCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            data.draggableItem.position = ray.GetPoint(enter);
        }
    }


    private float timer;
    private float speed;
    private Vector3 lastTouch;
    private float currentSpeed;

    public bool CalculateMatchStickPosition()
    {
        if (!isDraging) return false;

        lastTouch = currentTouch;
        currentTouch = Input.mousePosition;

        var currentPos = Vector3.Lerp(lastTouch, currentTouch, Time.deltaTime * data.lerpSpeed);
        
        Ray ray = inputCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            data.draggableItem.position = ray.GetPoint(enter);
            data.vfxTransform.position = data.draggableItem.position;
        }

        currentSpeed = Vector3.Distance(lastTouch, currentTouch) / Time.deltaTime;
        
        timer += Time.deltaTime;

        // Check conditions
        if (timer >= data.matchstickDuration)
        {
            Debug.Log("Too long!");
            OnMatchstickFireStop?.Invoke();
            Reset();
            return false;
        }

        if (currentSpeed > data.maxSpeed)
        {
            Debug.LogWarning("Too fast!");
            OnMatchstickFireStop?.Invoke();
            Reset();
            return false;
        }

        return Vector3.Distance(currentPos, data.endPoint.position) < data.endPointRadius;
    }
}

[Serializable]
public struct CannonDragAndDropData
{
    public Transform startPoint;
    public float startPointRadius;
    public Transform planeOrigin;

    public Transform endPoint;
    public float endPointRadius;
    public Transform draggableItem;
    public float lerpSpeed;
    public MiniGameType type;

    [Header("Shoot")] public float maxSpeed;
    public float matchstickDuration;
    public GameObject matchstickOnTable;
    public Transform vfxTransform;

    public enum MiniGameType
    {
        Shoot,
        Load
    }
}