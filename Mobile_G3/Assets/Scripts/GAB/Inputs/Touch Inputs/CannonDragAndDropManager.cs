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
        data.bullet.gameObject.SetActive(false);
    }

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;

        if (ctx.started)
        {
            startTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
            startTouch.y = data.startPoint.position.y;

            if (Vector3.Distance(startTouch, data.startPoint.position) <= data.startPointRadius)
            {
                isDraging = true;
                data.bullet.position = startTouch;
                data.bullet.gameObject.SetActive(true);
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
        startTouch = Vector3.zero;
        isDraging = false;
        data.bullet.gameObject.SetActive(false);

        if (Vector3.Distance(data.bullet.position, data.endPoint.position) < data.endPointRadius)
        {
            OnBulletOnTargetPoint?.Invoke();
        }
    }

    public void CalculateBulletPosition()
    {
        if (!isDraging) return;

        currentTouch = inputCamera.ScreenToWorldPoint(Input.mousePosition);
        currentTouch.y = data.startPoint.position.y;

        data.bullet.position = Vector3.Lerp(data.bullet.position, currentTouch, Time.deltaTime * data.lerpSpeed);
    }

    public void CalculateMatchStickPosition()
    {
        // Calculer la vitesse ici
    }
}

[Serializable]
public struct CannonDragAndDropData
{
    public Transform startPoint;
    public float startPointRadius;

    public Transform endPoint;
    public float endPointRadius;
    
    [Header("Load")]
    public Transform bullet;
    public float lerpSpeed;

    [Header("Shoot")] public float maxSpeed;
}