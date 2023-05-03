using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasManager : MonoSingleton<MainCanvasManager>
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Slider boatLifeSlider;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite planckIcon, bulletIcon;
    [SerializeField] private TMP_Text debugItemText;
    private (string minutes, string seconds) currentTimer;
    
    public void SetTimerOnDisplay(float remainingTime)
    {
        currentTimer = ConvertFloatInMinutesAndSeconds(remainingTime);
        timerText.text = $"{currentTimer.minutes}:{currentTimer.seconds}";
    }

    private static (string, string) ConvertFloatInMinutesAndSeconds(float seconds)
    {
        int minutes = 0;
        string secondString = "";
        while (seconds >= 60)
        {
            seconds -= 60;
            minutes++;
        }

        if (seconds < 10) secondString += "0";
        secondString += ((int) seconds).ToString();
        return (minutes.ToString(), secondString);
    }

    public void SetLifeOnDisplay(int life, int maxLife)
    {
        var ratio = life / maxLife;
        boatLifeSlider.value = ratio;
    }

    public void SetStarOnDisplay(int count)
    {
        starText.text = $"{count}/3";
    }

    public void SetItemOnDisplay(InventoryObject item)
    {
        itemImage.sprite = item switch
        {
            InventoryObject.None => null,
            InventoryObject.Plank => planckIcon,
            InventoryObject.CannonBall => bulletIcon,
            _ => null
        };
        
        debugItemText.text = item switch
        {
            InventoryObject.None => "None",
            InventoryObject.Plank => "Planck",
            InventoryObject.CannonBall => "Bullet",
            _ => "Error"
        };
    }
}