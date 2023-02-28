using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

public class CleanSwipeManager : MonoBehaviour
{
    private Vector2 startTouch, currentTouch;
    private Vector2 swipeVector;
    private bool isDraging;
    private float score;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Transform movingPoint;
    
    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDraging = true;
        }
        else if (ctx.canceled)
        {
            isDraging = false;
            startTouch = currentTouch = swipeVector = Vector2.zero;
            movingPoint.position = currentTouch;
            score = 0;
            scoreText.text = score.ToString("F2");
        }
    }

    private void Update()
    {
        if (isDraging)
        {
            currentTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            movingPoint.position = currentTouch;

            swipeVector = currentTouch - startTouch;

            score += swipeVector.magnitude;

            scoreText.text = score.ToString("F2");
            startTouch = currentTouch;
        }
    }
}