using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniGame_Fire : MiniGame
{
    /* 23/04
     * On ne produit pas ce mini-jeu pour l'instant !
     *
     * Priorité sur les choses pas encore produites
     * GameLoop, Events, etc
     */
    
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private FireGyroscopeSetupData fireData;
    [SerializeField] private float damagePerSecond;

    public Transform basementWood, layerPoint, waterGate1, waterGate2;
    
    [Header("Setup Start Fire Ring")]
    public int numberOfSpawnPoints;
    public float radius;
    public float startingAngle;
    public float positionThreshold;
    
    [Header("Gyro Value")]
    public float gyroStart,gyroSpeed,gyroValue;
    [Header("Gizmos")]
    public bool enableGizmos;    
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
        
        //Workshop.instance.FireManager.OnCheckFire =  CheckObjectIsInsideGate();
        
        gyroValue = 0;
        // Enables Gyroscope
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        
        WorkshopManager.instance.gyroscopeManager.Enable(data);
        gyroStart = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.y;
        StartExecutingMiniGame();
    }

    private void SetupFirePlacement()
    {
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float angle = ((360f / numberOfSpawnPoints) * i +  startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
        
            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position;
            fireData.fireSizeType[i].position = spawnPosition;
            fireData.fireSizeType[i].parent = layerPoint.transform;
        }
    }
    
    public override void AssociatedWorkshopGetActivatedHostSide()
    {
        base.AssociatedWorkshopGetActivatedHostSide();
        // Todo - Boat takes damages
    }
    

    public override void ExecuteMiniGame()
    {
        if (isRunning)
        {
            float currentGyro = WorkshopManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles.y;
            float move = currentGyro - gyroStart;
            gyroValue += move * gyroSpeed;
            gyroStart = currentGyro;
            gyroValue = Mathf.Clamp(gyroValue, -180, 180);

            float eulerY = layerPoint.eulerAngles.y < 180 ? layerPoint.eulerAngles.y : layerPoint.eulerAngles.y - 360;
            layerPoint.rotation = Quaternion.Euler(0,Mathf.Lerp(eulerY,gyroValue,Time.deltaTime*5),0);
        }
    }

    public override void Reset()
    {
        
    }

    protected override void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void OnLeaveMiniGame()
    {
        WorkshopManager.instance.shrimpSwipeManager.Disable();
        WorkshopManager.instance.gyroscopeManager.Disable();
        StopExecutingMiniGame();
        ExitMiniGame(false);
    }

    private void CheckObjectIsInsideGate()
    {
        foreach (var fire in fireData.fireSizeType)
        {
            bool isBetweenPoints = IsObjectBetweenPoints(fire);
            if(isBetweenPoints)
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
            if (Math.Abs(distanceToObject1 + distanceToObject2 - Vector3.Distance(point1Position, point2Position)) < positionThreshold)
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
