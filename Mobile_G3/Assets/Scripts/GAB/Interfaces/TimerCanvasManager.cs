using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerCanvasManager : MonoSingleton<TimerCanvasManager>
{
    [SerializeField] private TMP_Text timerText;
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
}