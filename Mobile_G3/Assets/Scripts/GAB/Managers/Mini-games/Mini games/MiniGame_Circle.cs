using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGame_Circle : MiniGame
{
    [SerializeField] private CircularSwipeSetupData data;
    [SerializeField] private float duration;
    private float timer;
    [SerializeField] private TMP_Text timerText;

    private void OnValidate()
    {
        data.availableZone.localScale = new Vector3(data.minMaxMagnitude.y * 2, data.minMaxMagnitude.y * 2, 1);
        data.unavailableCenterZone.localScale = new Vector3(data.minMaxMagnitude.x * 2, data.minMaxMagnitude.x * 2, 1);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        MiniGameManager.instance.circularSwipeManager.Enable(data);
        timerText.text = duration.ToString("F0");
    }

    public override void ExecuteMiniGame()
    {
        if (timer >= duration)
        {
            ExitMiniGame(false);
        }
        else
        {
            timer += Time.deltaTime;

            if (MiniGameManager.instance.circularSwipeManager.CalculateCircularSwipe())
            {
                ExitMiniGame(true);
            }
        }

        timerText.text = (duration - timer).ToString("F0");
    }

    public override void ExitMiniGame(bool victory)
    {
        MiniGameManager.instance.circularSwipeManager.Disable();
        timer = 0;
        base.ExitMiniGame(victory);
    }
}