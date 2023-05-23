using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniGame_Fire : MiniGame
{
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private FireGyroscopeSetupData fireData;

    public Transform basementWood, layerPoint, waterGate1, waterGate2;

    [Header("Setup Start Fire Ring")] public int numberOfSpawnPoints;
    public float radius;
    public float startingAngle;
    public float positionThreshold;

    [Header("Gyro Value")] public float gyroStart, gyroSpeed, gyroValue;
    [Header("Gizmos")] public bool enableGizmos;
    public Vector3 offsetGizmos;
    private Vector3 fireInitPos;

    public override void OnNetworkSpawn()
    {
        fireInitPos = fireData.fireSizeType[0].position;
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        SetupFirePlacement();
        gyroValue = 0;

        // Enables Gyroscope
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());

        WorkshopManager.instance.gyroscopeManager.Enable(data);
        gyroStart = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.z;
        StartExecutingMiniGame();
    }

    [ContextMenu("Test setup")]
    private void SetupFirePlacement()
    {
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float angle = ((360f / numberOfSpawnPoints) * i + startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position;
            fireData.fireSizeType[i].position = spawnPosition;
            fireData.fireSizeType[i].parent = layerPoint.transform;
        }
    }

    private float currentGyro;
    private float moveGyro;
    private float eulerY;
    private float speedIncrease = 5f;

    public override void ExecuteMiniGame()
    {
        currentGyro = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.z;
        if (currentGyro > 180f)
        {
            currentGyro = currentGyro - 360f;
        }
        currentGyro /= 90f;
        moveGyro = currentGyro * speedIncrease;
        layerPoint.transform.Rotate(Vector3.up, moveGyro);
        CheckObjectIsInsideGate();
        LaunchExitMiniGame();
    }

    public override void Reset()
    {
        foreach (Transform fire in fireData.fireSizeType)
        {
            fire.position = fireInitPos;
            fire.GetComponent<FireObject>().ResetFire();
            //fire.gameObject.SetActive(false);
        }

        gyroStart = 0;
        gyroValue = 0;
        eulerY = 0;
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
        foreach (Transform fire in fireData.fireSizeType)
        {
            if (fire.GetComponent<FireObject>().currentValue != 0f)
            {
                return; // If any object has a non-zero value, exit the method
            }
        }

        StopExecutingMiniGame();
        WorkshopManager.instance.gyroscopeManager.Disable();
        HonorificManager.instance.AddHonorific(Honorifics.Firefighter);
        WorkshopManager.instance.SetVictoryIndicator();
        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        ExitMiniGame(true);
    }

    
    
    private void CheckObjectIsInsideGate()
    {
        foreach (var fire in fireData.fireSizeType)
        {
            bool isBetweenPoints = IsObjectBetweenPoints(fire);
            if (isBetweenPoints)
            {
                fire.GetComponent<FireObject>().isFilled = true;
            }
            else
            {
                fire.GetComponent<FireObject>().isFilled = false;
            }
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