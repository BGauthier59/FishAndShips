using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Fire : MiniGame
{
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private FireGyroscopeSetupData fireData;

    public Transform basementWood, layerPoint, waterGate1, waterGate2;

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
    private List<Transform> firePointsWorkshop = new List<Transform>();
    public List<Vector3> firePlacement = new List<Vector3>();
    private Transform currentFireActive;
    private int currentFireComplete;
    private int randomIndex;
    private int currentFlameIndex;

    public override void OnNetworkSpawn()
    {
        fireInitPos = fireData.fireSizeType[0].position;
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

    [ContextMenu("Test setup")]
    private void SetupFirePlacement()
    {
        RandomizeList(fireData.fireSizeType);
        foreach (Transform fire in fireData.fireSizeType)
        {
            fire.gameObject.SetActive(false);
        }
        
        totalPoints = useRandom ? Random.Range(1, numberOfSpawnPoints) : numberOfSpawnPoints;
        for (int i = 0; i < totalPoints; i++)
        {
            firePointsWorkshop.Add(fireData.fireSizeType[i]);
            float angle = ((360f / totalPoints) * i + startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position;
            firePlacement.Add(spawnPosition);
        }
        
        //Set a Fire to a Random Point
        randomIndex = Random.Range(0, firePlacement.Count);
        currentFlameIndex = randomIndex;
        currentFireActive = firePointsWorkshop[0];
        currentFireActive.position = firePlacement[currentFlameIndex];
        currentFireActive.gameObject.SetActive(true);
        currentFireActive.gameObject.GetComponent<FireObject>().firePart[0].Play();
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
        Debug.Log(moveGyro);
        CheckObjectIsInsideGate();
        CheckIfFireIsComplete();
        LaunchExitMiniGame();
    }

    public override void Reset()
    {
        foreach (Transform fire in firePointsWorkshop)
        {
            fire.position = fireInitPos;
            fire.GetComponent<FireObject>().ResetFire();
        }
        
        currentGyroValue = 0;
        currentFireComplete = 0;
        moveGyro = 0;
        currentFlameIndex = 0;
        randomIndex = 0;
        layerPoint.transform.eulerAngles = Vector3.zero;
        firePointsWorkshop.Clear();
        firePlacement.Clear();
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
        if (currentFireComplete != numberOfFireToComplete) return;
        StopExecutingMiniGame();
        WorkshopManager.instance.gyroscopeManager.Disable();
        HonorificManager.instance.AddHonorific(Honorifics.Firefighter);
        WorkshopManager.instance.SetVictoryIndicator();
        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        ExitMiniGame(true);
    }

    [ContextMenu("Randomize Fire Size List")]
    private void RandomizeListDebug()
    {
        RandomizeList(fireData.fireSizeType);
    }

    void RandomizeList(List<Transform> list)
    {
        int count = list.Count;
        for (int i = 0; i < count - 1; i++)
        {
            int randomIndex = Random.Range(i, count);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }

    private void CheckIfFireIsComplete()
    {
        if (currentFireActive.GetComponent<FireObject>().currentValue <= 0)
        {
            currentFireComplete += 1;
            ChangeIndexForTheFlame();
        }
    }

    private void ChangeIndexForTheFlame()
    {
        while (currentFlameIndex == randomIndex)
        {
            randomIndex = Random.Range(0, firePointsWorkshop.Count);
        }
        currentFlameIndex = randomIndex;
        Debug.Log("New Current Index" + currentFlameIndex);
        currentFireActive = firePointsWorkshop[currentFlameIndex];
        currentFireActive.position = firePlacement[randomIndex];
        currentFireActive.gameObject.SetActive(true);
        currentFireActive.gameObject.GetComponent<FireObject>().firePart[0].Play();
    }
    
    private void CheckObjectIsInsideGate()
    {
        bool isBetweenPoints = IsObjectBetweenPoints(currentFireActive);
        if (isBetweenPoints)
        {
            currentFireActive.GetComponent<FireObject>().isFilled = true;
        }
        else
        {
            currentFireActive.GetComponent<FireObject>().isFilled = false;
        }
    }

    bool IsObjectBetweenPoints(Transform objectToCheck)
    {
        if (objectToCheck != null)
        {
            Vector3 objectPosition = objectToCheck.transform.position;
            Vector3 point1Position = waterGate1.position;
            Vector3 point2Position = waterGate2.position;

            // Calculate the distance between the object and the two points
            float distanceToObject1 = Vector3.Distance(objectPosition, point1Position);
            float distanceToObject2 = Vector3.Distance(objectPosition, point2Position);

            // Check if the object is between the two points
            if (Math.Abs(distanceToObject1 + distanceToObject2 - Vector3.Distance(point1Position, point2Position)) <
                positionThreshold)
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
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(waterGate2.position, positionThreshold);
        }
    }
}