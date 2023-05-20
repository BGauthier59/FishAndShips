using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireObject : MonoBehaviour
{
    public int firePowerCount = 200;
    public FireSize fireSize;
    private float decreaseSpeed;
    public bool canBeFilled;
    public bool isFilled;

    public float currentValue;
    public float countdownTimer;

    public void Start()
    {
        SetFireStart(); 
        currentValue = firePowerCount;
        countdownTimer = firePowerCount;
    }

    void FireStat()
    {
        switch (fireSize)
        {
            case FireSize.Small:
                firePowerCount = 3;
                break;
            case FireSize.Medium:
                firePowerCount = 5;
                break;
            case FireSize.Large:
                firePowerCount = 10;
                break;
        }
    }

    void SetFireStart()
    {
        int randomIndex = Random.Range(0, Enum.GetValues(typeof(FireSize)).Length);
        fireSize = (FireSize)randomIndex;
        FireStat();
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
                GetComponent<MeshRenderer>().material.color = Color.black;
                Debug.Log("Finish");
            }
            else
            {
                currentValue = Mathf.Lerp(0f, firePowerCount, countdownTimer / firePowerCount);
                GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
        else
        {
            if (currentValue <= 0)
            {
                GetComponent<MeshRenderer>().material.color = Color.black;
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
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
