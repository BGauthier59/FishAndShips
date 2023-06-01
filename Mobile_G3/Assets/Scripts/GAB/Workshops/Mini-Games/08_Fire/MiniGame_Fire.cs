using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Fire : MiniGame
{
    [SerializeField] private GyroscopeSetupData data;
    public List<FireObject> fireSizeType;

    public Transform basementWood, layerPoint, waterGate1;

    [Header("Setup Start Fire Ring")] 
    public bool useRandom;
    public int numberOfSpawnPoints = 8;
    public int numberOfFireToComplete = 3;
    public float radius;
    public float startingAngle;
    public float positionThreshold;
    

    [Header("Gyro Value")] 
    public float gyroStart;
    public float gyroSpeed;
    public float currentGyroValue;
    
    [Header("Gizmos")] 
    public bool enableGizmos;
    public Vector3 offsetGizmos;
    private Vector3 fireInitPos;
    private int totalPoints;
    public List<Transform> firePlacement;
    public FireObject currentFireActive;
    private int currentFireComplete;
    private int randomIndex;
    private int currentFlameIndex;

    public override void OnNetworkSpawn()
    {
        fireInitPos = fireSizeType[0].transform.position;
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        SetupFirePlacement();
        currentGyroValue = 0;
        currentFireComplete = 0;

        // Enables Gyroscope
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        WorkshopManager.instance.StartMiniGameGyroscopeFireTutorial();
        WorkshopManager.instance.gyroscopeManager.Enable(data);
        StartExecutingMiniGame();
    }
    
    private void SetupFirePlacement()
    {

        Debug.Log("C'EST SET");
        //Set a Fire to a Random Point
        randomIndex = Random.Range(0, firePlacement.Count-1);
        currentFireActive = fireSizeType[0];
        currentFireActive.transform.position = firePlacement[randomIndex].position;
        currentFireActive.gameObject.SetActive(true);
        currentFireActive.firePart[0].Play();
    }
    
    private float moveGyro;
    private float actualGyro;

    public override void ExecuteMiniGame()
    {
        currentGyroValue = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.z;
        actualGyro = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.z;
        if (currentGyroValue > 180f)
        {
            currentGyroValue -= 360f;
        }
        currentGyroValue /= 180f;
        moveGyro = currentGyroValue * -gyroSpeed;
        layerPoint.transform.Rotate(Vector3.up, moveGyro);
        CheckObjectIsInsideGate();
    }

    public override void Reset()
    {
        foreach (FireObject fire in fireSizeType)
        {
            fire.transform.position = fireInitPos;
            fire.ResetFire();
            fire.gameObject.SetActive(false);
        }

        currentGyroValue = 0;
        currentFireComplete = 0;
        moveGyro = 0;
        currentFlameIndex = 0;
        randomIndex = 0;
        layerPoint.transform.eulerAngles = Vector3.zero;
    }
    
    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        WorkshopManager.instance.gyroscopeManager.Disable();
        StopExecutingMiniGame();
        ExitMiniGame(false);
    }

    private async void LaunchExitMiniGame()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.gyroscopeManager.Disable();
        HonorificManager.instance.AddHonorific(Honorifics.Firefighter);
        WorkshopManager.instance.SetVictoryIndicator();
        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        ExitMiniGame(true);
    }

    private void ChangeIndexForTheFlame()
    {
        Debug.Log("CHANGE FLAME");
        int rng = randomIndex;
        while (rng == randomIndex)
        {
            rng = Random.Range(0, firePlacement.Count);
        }
        Debug.Log(randomIndex + " P / N " +rng);
        randomIndex = rng;
        currentFireActive = fireSizeType[currentFireComplete];
        Debug.Log(currentFireComplete);
        currentFireActive.transform.position = firePlacement[randomIndex].position;
        currentFireActive.gameObject.SetActive(true);
        currentFireActive.firePart[0].Play();
    }
    
    private void CheckObjectIsInsideGate()
    {
        bool isBetweenPoints = IsObjectBetweenPoints(currentFireActive.transform);
        if (isBetweenPoints)
        {
            currentFireActive.ExtinguishFlame();
            CompleteFire();
        }
    }
    
    private void  CompleteFire()
    {
        currentFireComplete += 1;
        if (currentFireComplete < 3) ChangeIndexForTheFlame();
        else LaunchExitMiniGame();
    }

    bool IsObjectBetweenPoints(Transform objectToCheck)
    {
        if (objectToCheck != null)
        {
            // Calculate the distance between the object and the two points
            float distanceToObject = Vector3.SqrMagnitude(objectToCheck.transform.position - waterGate1.position);

            // Check if the object is between the two points
            if (distanceToObject < positionThreshold * positionThreshold)
            {
                return true;
            }
        }

        return false;
    }

    public void OnDrawGizmos()
    {
        if (enableGizmos)
        {
            Vector3 objPosition = layerPoint.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(objPosition + offsetGizmos, radius);
            float angleBetweenSpawnPoints = 360f / numberOfSpawnPoints;
            for (int i = 0; i < numberOfSpawnPoints; i++)
            {
                float x = radius * Mathf.Cos(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
                float z = radius * Mathf.Sin(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);

                Vector3 spawnPosition = new Vector3(x, 0f, z) + objPosition;
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(spawnPosition + offsetGizmos, 0.1f);
            }

            // First Point
            float angle = startingAngle * Mathf.Deg2Rad;
            float x1 = radius * Mathf.Cos(angle);
            float z1 = radius * Mathf.Sin(angle);

            Vector3 spawnFirstPoint = new Vector3(x1, 0f, z1) + layerPoint.position;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnFirstPoint, 0.2f);


            //Point Gate Gizmos
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(waterGate1.position, positionThreshold);
        }
    }
}