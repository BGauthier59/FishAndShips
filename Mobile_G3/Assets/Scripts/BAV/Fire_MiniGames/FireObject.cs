using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireObject : MonoBehaviour
{
    public int firePowerCount = 3;
    public FireSize fireSize;
    private float decreaseSpeed;
    public bool isFilled;
    public float currentValue;
    public float countdownTimer;
    public List<ParticleSystem> firePart;

    public void Start()
    {
        FireStat();
        currentValue = firePowerCount;
        countdownTimer = firePowerCount;
    }

    void FireStat()
    {
        switch (fireSize)
        {
            case FireSize.Small:
                firePowerCount = 1;
                break;
            case FireSize.Medium:
                firePowerCount = 2;
                break;
            case FireSize.Large:
                firePowerCount = 3;
                break;
        }
    }

    public void ResetFire()
    {
        isFilled = false;
        countdownTimer = firePowerCount;
        currentValue = firePowerCount;
        firePart[0].gameObject.SetActive(false);
        firePart[1].gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentValue <= 0) return;
        if (isFilled)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                currentValue = 0;
                countdownTimer = 0;
                firePart[0].Stop();
                firePart[0].gameObject.SetActive(false);
                firePart[1].gameObject.SetActive(true);
                firePart[1].Play();
            }
            else
            {
                currentValue = Mathf.Lerp(0f, firePowerCount, countdownTimer / firePowerCount);
                //Modify Value
                var emissionModule = firePart[0].emission;
                emissionModule.rateOverTimeMultiplier = Mathf.Lerp(40f, 20f, countdownTimer / firePowerCount);
            }
        }
        else
        {
            firePart[0].gameObject.SetActive(true);
            firePart[0].Play();
        }
    }
}

[Serializable]
public enum FireSize
{
    Small,
    Medium,
    Large
}