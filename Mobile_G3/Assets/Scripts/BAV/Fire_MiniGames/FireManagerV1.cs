using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireManagerV1 : MonoBehaviour
{
    /*[Header("Manager Spawner")]
    public GameObject firePrefab;
    public int numberOfSpawnPoints;
    public float radius;
    public Vector3 centerPosition;
    public List<Transform> firePoints = new List<Transform>();
    public Transform layerPoint;
    public float startingAngle;

    [Header("Manager Mini Games")] 
    public List<Transform> pointGate; //0 => can be Filled, Increment Index, 1 => Filled the Left, 2=> Can't be filled
    public GameObject woodSupport;
    private float actualRotationY;
    public int totalFire;

    // The two points that define the rectangular area

    [Header("Debug")]
    [Range(0, 1)] 
    public bool useRandom;
    public float debugRotation = 0.5f;
    public Vector3 offsetGizmos;
    
    private float distance;
    private Vector3 _midpoint;
    // Set a threshold value to determine if the point has passed the specific position
    public float positionThreshold = 0.1f;
    public float gyroSpeed = 5f;
    
    private Gyroscope gyroscope;
    private Quaternion correctionQuaternion;
    private int totalPoints;

    private void Awake()
    {
        foreach (var firePoint in firePoints)
        {
            firePoint.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        ListUtils.RandomizeList(firePoints);
        totalPoints = (int)(useRandom ? Random.Range(1f, numberOfSpawnPoints) : numberOfSpawnPoints);
        // Instantiate the spawn points
        for (int i = 0; i < totalPoints; i++)
        {
            float angle = ((360f / totalPoints) * i +  startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
        
            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position  + centerPosition;
            firePoints[i].transform.position = spawnPosition;
            firePoints[i].gameObject.SetActive(true);
            firePoints[i].transform.parent = layerPoint.transform;
        }

        EnableGyro();
    }
    
    public float increasedRotationSpeed = 10f;
    public float minMaxGyro = 180;

    private Vector3 currentGyro;
    private float currentGyroZ;
    private float gyroAngle;
    private float moveGyro;

    public void Update()
    {
        currentGyroZ = currentGyro.z;
        if (currentGyro.z > 180f)
        {
            currentGyroZ = currentGyroZ - 360f;
        }
        currentGyroZ /= 180f;
        moveGyro = currentGyroZ * gyroSpeed;
        layerPoint.transform.Rotate(Vector3.up, moveGyro);
        FillGivenIndex();
    }
    
    void FillGivenIndex()
    {
        foreach (var firePoint in firePoints)
        {
            bool isBetweenPoints = IsObjectBetweenPoints(firePoint);
            if(isBetweenPoints)
            {
                firePoint.GetComponent<FireObject>().isFilled = true;
            }
            else
            {
                firePoint.GetComponent<FireObject>().isFilled = false;
            }
        }
    }

    void LaunchExitMinigame()
    {
        foreach (Transform obj in firePoints)
        {
            if (obj.GetComponent<FireObject>().currentValue != 0f)
            {
                return; // If any object has a non-zero value, exit the method
            }
        }
        
        Debug.Log("Game Over!");
    }

    bool IsObjectBetweenPoints(Transform objectToCheck)
    {
        if (objectToCheck != null)
        {
            Vector3 objectPosition = objectToCheck.transform.position;
            Vector3 point1Position = pointGate[0].position;
            Vector3 point2Position = pointGate[1].position;

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
        Gizmos.DrawWireSphere(pointGate[0].position, positionThreshold);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointGate[1].position, positionThreshold);
    }
    public void EnableGyro()
    {
        gyroscope = Input.gyro;
        gyroscope.enabled = true;
        correctionQuaternion = Quaternion.Euler(90f, 0, 0);
    }

    public Quaternion GetGyroRotation()
    {
        var gyroQuaternion = GyroToUnity(gyroscope.attitude);
        var calculatedQuaternion = correctionQuaternion * gyroQuaternion;
    
        return calculatedQuaternion;
    }

    private Quaternion GyroToUnity(Quaternion q)
    {
        var rotation = new Quaternion(q.x, q.y, -q.z, -q.w);
        return rotation;
    }*/
}