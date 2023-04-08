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
        data.draggableItem.gameObject.SetActive(false);
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
            startTouch.y = data.startPoint.position.y;
            currentTouch = startTouch;

            if (Vector3.Distance(startTouch, data.startPoint.position) <= data.startPointRadius)
            {
                isDraging = true;
                data.draggableItem.position = startTouch;
                data.draggableItem.gameObject.SetActive(true);
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

        if (data.type == CannonDragAndDropData.MiniGameType.Load &&
            Vector3.Distance(data.draggableItem.position, data.endPoint.position) < data.endPointRadius)
        {
            OnBulletOnTargetPoint?.Invoke();
        }
    }

    public void CalculateBulletPosition()
    {
        if (!isDraging) return;

        currentTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        currentTouch.y = data.startPoint.position.y;

        data.draggableItem.position =
            Vector3.Lerp(data.draggableItem.position, currentTouch, Time.deltaTime * data.lerpSpeed);
    }


    private float timer;
    private float speed;
    private Vector3 lastTouch;
    private float currentSpeed;
    public bool CalculateMatchStickPosition()
    {
        // Calculer la vitesse ici

        if (!isDraging) return false;

        lastTouch = currentTouch;
        currentTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        currentTouch.y = data.startPoint.position.y;

        data.draggableItem.position =
            Vector3.Lerp(data.draggableItem.position, currentTouch, Time.deltaTime * data.lerpSpeed);

        currentSpeed = Vector3.Distance(lastTouch, currentTouch) / Time.deltaTime;
        Debug.Log(currentSpeed);

        timer += Time.deltaTime;
        
        // Check conditions
        if (timer >= data.matchstickDuration)
        {
            Debug.Log("Too long!");
            Reset();
            return false;
        }

        if (currentSpeed > data.maxSpeed)
        {
            Debug.LogWarning("Too fast!");
            Reset();
            return false;
        }

        return Vector3.Distance(data.draggableItem.position, data.endPoint.position) < data.endPointRadius;
    }
}

[Serializable]
public struct CannonDragAndDropData
{
    public Transform startPoint;
    public float startPointRadius;

    public Transform endPoint;
    public float endPointRadius;
    public Transform draggableItem;
    public float lerpSpeed;
    public MiniGameType type;

    [Header("Shoot")] public float maxSpeed;
    public float matchstickDuration;

    public enum MiniGameType
    {
        Shoot,
        Load
    }
}