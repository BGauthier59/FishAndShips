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
    public bool canBeFilled;
    public bool isFilled;

    public float currentValue;
    public float countdownTimer;

    public MeshRenderer meshRend;

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
    }

    void Update()
    {
        if (isFilled)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                currentValue = 0;
                countdownTimer = 0;
                meshRend.material.color = Color.black;
                Debug.Log("Finish");
            }
            else
            {
                currentValue = Mathf.Lerp(0f, firePowerCount, countdownTimer / firePowerCount);
                meshRend.material.color = Color.blue;
            }
        }
        else
        {
            if (currentValue <= 0)
            {
                meshRend.material.color = Color.black;
            }
            else
            {
                meshRend.material.color = Color.red;
            }
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