using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvasManager : MonoSingleton<MainCanvasManager>
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image boatLifeSlider;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite plankIcon, bulletIcon, emptyIcon;
    [SerializeField] private Animation itemAnim;
    [SerializeField] private Animation timerAnim;
    private (string minutes, string seconds) currentTimer;
    [SerializeField] private Animation[] starsAnims;
    private bool endgame;
    

    public void SetTimerOnDisplay(float remainingTime)
    {
        currentTimer = ConvertFloatInMinutesAndSeconds(remainingTime);
        timerText.text = $"{currentTimer.minutes}:{currentTimer.seconds}";
        if (!endgame && remainingTime < 15) SetTimerEndGame();
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

    public void SetLifeOnDisplay(float life, float maxLife)
    {
        var ratio = life / maxLife;
        boatLifeSlider.fillAmount = ratio;
    }

    public void GainStar(int count)
    {
        starsAnims[count].Play("StarUnlockedInGame");
    }
    
    public void LooseStar(int count)
    {
        starsAnims[count].Play("StarLostInGame");
    }
    
    public void SetTimerEndGame()
    {
        if(endgame) return;
        endgame = true;
        timerAnim.Play("TimerEndGame");
    }

    public void ResetCanvasIfNeeded()
    {
        foreach(Animation anim in starsAnims)
        {
            if (anim.isPlaying & anim.clip.name == "StarUnlockedInGame")
            {
                anim.Stop();
                Color c = anim.gameObject.GetComponent<Image>().color;
                c = Color.white;
                anim.gameObject.GetComponent<Image>().color = c;
                anim.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void SetItemOnDisplay(InventoryObject item)
    {
        if (!itemAnim.isPlaying && item != InventoryObject.None) itemAnim.Play(itemAnim.clip.name);
        itemImage.sprite = item switch
        {
            InventoryObject.None => emptyIcon,
            InventoryObject.Plank => plankIcon,
            InventoryObject.CannonBall => bulletIcon,
            _ => null
        };
    }
}