using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireManagerV1 : MonoBehaviour
{
    [Header("Manager Spawner")]
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
    public Vector2 minMaxOrientation = new Vector2(-135f, -45f);

    // The two points that define the rectangular area

    [Header("Debug")]
    [Range(0, 1)] 
    public float debugRotation = 0.5f;
    public Vector3 offsetGizmos;
    
    private float distance;
    private Vector3 _midpoint;
    // Set a threshold value to determine if the point has passed the specific position
    public float positionThreshold = 0.1f;
    
    private Gyroscope gyroscope;
    private Quaternion correctionQuaternion;
    public float gyroStart,gyroSpeed,gyroValue;

    void Start()
    {
        // Instantiate the spawn points
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float angle = ((360f / numberOfSpawnPoints) * i +  startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
        
            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position  + centerPosition;
            firePoints[i].transform.position = spawnPosition;
            firePoints[i].transform.parent = layerPoint.transform;
        }

        EnableGyro();
    }
    

    public void Update()
    {
        float currentGyro = GetGyroRotation().eulerAngles.y;
        float move = currentGyro - gyroStart;
        gyroValue += move * gyroSpeed;
        gyroStart = currentGyro;
        gyroValue = Mathf.Clamp(gyroValue, -180, 180);
        Debug.Log(gyroValue);

        float eulerY = layerPoint.eulerAngles.y < 180 ? layerPoint.eulerAngles.y : layerPoint.eulerAngles.y - 360;
        //layerPoint.rotation = Quaternion.Euler(0,Mathf.Lerp(eulerY,gyroValue,Time.deltaTime*5),0);
        //actualRotationY = Mathf.Lerp(minMaxOrientation.x, minMaxOrientation.y, debugRotation);
        woodSupport.transform.rotation = Quaternion.Euler( -90f,Mathf.Lerp(eulerY,gyroValue,Time.deltaTime*5) ,0f);
        layerPoint.transform.rotation = Quaternion.Euler(0f, Mathf.Lerp(eulerY,gyroValue,Time.deltaTime*5), 0f);
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
    }
}