using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireManagerV2 : MonoBehaviour
{
    [Header("Manager Spawner")]
    public GameObject firePrefab;
    public int numberOfSpawnPoints;
    public float radius;
    public Vector3 centerPosition;
    public float rectHeight;
    public List<Transform> firePoints = new List<Transform>();
    public Transform layerPoint;
    public float startingAngle;

    [Header("Manager Mini Games")] 
    public List<Transform> pointGate; //0 => can be Filled, Increment Index, 1 => Filled the Left, 2=> Can't be filled
    public GameObject bucket;
    public float speed;
    private Quaternion baseRotationObject = Quaternion.Euler(0f,0f,0f);
    private float actualRotationX;
    public bool isLeft;
    public Vector2 minMaxOrientation = new Vector2(-135f, -45f);

    // The two points that define the rectangular area

    [Header("Gizmos")] 
    public Vector3 offsetGizmos;
    [Range(0, 1)] 
    public float debugRotation = 0.5f;
    private bool enableAreaRectangleGizmos = false;
    

    private float distance;
    private Vector3 _midpoint;
    public int currentIndex = 1;

    public int previousIndex;
    // Set a threshold value to determine if the point has passed the specific position
    public float positionThreshold = 0.1f;

    void Start()
    {
        currentIndex = 1;
        enableAreaRectangleGizmos = true;
        
        // Instantiate the spawn points
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float angle = ((360f / numberOfSpawnPoints) * i +  startingAngle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
        
            Vector3 spawnPosition = new Vector3(x, 0f, z) + layerPoint.transform.position  + centerPosition;
            GameObject spawnPointObject = Instantiate(firePrefab, spawnPosition, Quaternion.identity, transform);

            spawnPointObject.transform.parent = layerPoint.transform;
            firePoints.Add(spawnPointObject.transform);
        }
        bucket.transform.rotation = baseRotationObject;
        
    }
    

    public void Update()
    {
        layerPoint.localRotation  *= Quaternion.Euler(0f,speed * Time.deltaTime,0f);
        DetectRotation(bucket);
        actualRotationX = Mathf.Lerp(minMaxOrientation.x, minMaxOrientation.y, debugRotation);
        bucket.transform.rotation = Quaternion.Euler( actualRotationX, -90f,0f);
        
        IncrementIndex();
        UpdateFillAmount();
        FillGivenIndex();
    }
    
    void IncrementIndex()
    {
        float distanceToSpecificPoint = Vector3.Distance(firePoints[currentIndex].position, pointGate[1].position);
        if (distanceToSpecificPoint <= positionThreshold)
        {
            currentIndex++;
            if (currentIndex >= firePoints.Count)
            {
                currentIndex = 0;
            }
        }
    }
    
    void UpdateFillAmount()
    {
        foreach (var firePoint in firePoints)
        {
            float distanceToFirstGate = Vector3.Distance(firePoint.position, pointGate[0].position);
            if (distanceToFirstGate <= positionThreshold)
            {
                firePoint.GetComponent<FireObject>().canBeFilled = true;
            }

            float distanceToEndGate = Vector3.Distance(firePoint.position, pointGate[2].position);
            if (distanceToEndGate <= positionThreshold)
            {
                firePoint.GetComponent<FireObject>().canBeFilled = false;
            }
        }
    }

    void FillGivenIndex()
    {
        previousIndex = currentIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = firePoints.Count - 1;
        }
        firePoints[currentIndex].GetComponent<FireObject>().isFilled = !isLeft;
        firePoints[previousIndex].GetComponent<FireObject>().isFilled = isLeft;
    }
    
    public void DetectRotation(GameObject obj)
    {
        if (minMaxOrientation.x <  actualRotationX && actualRotationX < -90f) 
        {
            isLeft = false;
        }
        if (minMaxOrientation.y > actualRotationX && actualRotationX > -90f)
        {
            isLeft = true;
        }
    }
    
    void OnDrawGizmos()
    {
        Vector3 objPosition = centerPosition + transform.position;
        if (!enableAreaRectangleGizmos)
        {
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

            Vector3 spawnFirstPoint = new Vector3(x1, 0f, z1) + centerPosition;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnFirstPoint, 0.2f);
        }
        // Draw the circle as a Gizmo
        if (enableAreaRectangleGizmos)
        {
            for (int i = 0; i < numberOfSpawnPoints; i++)
            {
                // Draw the rectangular area between this spawn point and the next one
                Transform nextSpawnPoint = firePoints[(i + 1) % numberOfSpawnPoints];
                DrawRectBetweenSpawnPoints(firePoints[i], nextSpawnPoint);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_midpoint + offsetGizmos + objPosition, 0.1f);
            }
        }
        
        //Point Gate Gizmos
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointGate[0].position, positionThreshold);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pointGate[1].position, positionThreshold);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointGate[2].position, positionThreshold);
    }
    
    void DrawRectBetweenSpawnPoints(Transform spawnPoint1, Transform spawnPoint2)
    {
        // Calculate the midpoint between the two spawn points
        _midpoint = (spawnPoint1.position + spawnPoint2.position) / 2f;
        _midpoint.y *= rectHeight * 0.5f;
    
        // Calculate the direction vector from spawnPoint1 to spawnPoint2
        Vector3 direction = (spawnPoint2.position - spawnPoint1.position).normalized;
    
        // Calculate the up vector for the rectangle
        Vector3 up = Vector3.Cross(direction, Vector3.forward).normalized;
    
        // Calculate the size of the rectangle
        float width = Vector3.Distance(spawnPoint1.position, spawnPoint2.position);
        Vector3 size = new Vector3(width, rectHeight * 0.5f, 0f);
    
        // Calculate the rotation of the rectangle
        Quaternion rotation = Quaternion.LookRotation(direction, up) * Quaternion.Euler(0f,90f,0f);

        //Create a Ref to the matrice
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix *= Matrix4x4.TRS(_midpoint + centerPosition, rotation, Vector3.one);
        
        // Draw the rectangle as a Gizmo
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = oldMatrix;
    }
}
