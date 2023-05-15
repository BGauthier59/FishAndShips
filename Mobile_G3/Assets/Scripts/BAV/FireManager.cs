using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    [Header("Manager Spawner")]
    public GameObject spawnPointPrefab;
    public int numberOfSpawnPoints;
    public float radius;
    public Vector3 centerPosition;
    public float rectHeight;
    public List<Transform> spawnPoints = new List<Transform>();
    public Transform layerPoint;

    [Header("Manager Mini Games")] 
    public GameObject bucket;
    public float speed;
    private Quaternion baseRotationObject = Quaternion.Euler(0,0,-90f);
    private Quaternion actualRotation;
    public bool isLeft;
    public Vector2 minMaxOrientation = new Vector2(-135f, -45f);
    public GameObject leftObject;
    public GameObject rightObject;
    
    // The two points that define the rectangular area

    [Header("Gizmos")] 
    public Vector3 offsetGizmos;
    [Range(0, 1)] 
    public float debugRotation = 0.5f;
    private bool enableAreaRectangleGizmos = false;
    

    private float distance;
    private Transform pointToTchek;
    private Vector3 _midpoint;

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
        }
        // Draw the circle as a Gizmo
        if (enableAreaRectangleGizmos)
        {
            for (int i = 0; i < numberOfSpawnPoints; i++)
            {
                // Draw the rectangular area between this spawn point and the next one
                Transform nextSpawnPoint = spawnPoints[(i + 1) % numberOfSpawnPoints];
                DrawRectBetweenSpawnPoints(spawnPoints[i], nextSpawnPoint);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_midpoint + offsetGizmos + objPosition, 0.1f);
            }
        }
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
    
    void Start()
    {
        enableAreaRectangleGizmos = true;
        // Instantiate the spawn points
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float angle = i * (360f / numberOfSpawnPoints) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
        
            Vector3 spawnPosition = new Vector3(x, 0f, z) + centerPosition;
            GameObject spawnPointObject = Instantiate(spawnPointPrefab, spawnPosition, Quaternion.identity, transform);
            
            spawnPointObject.transform.parent = layerPoint.transform;
            spawnPoints.Add(spawnPointObject.transform);
        }
        bucket.transform.rotation = baseRotationObject;
    }

    public void Update()
    {
        layerPoint.localRotation  *= Quaternion.Euler(0f,speed * Time.deltaTime,0f);
        DetectRotation(bucket);
        bucket.transform.localRotation = Quaternion.Euler( Mathf.Lerp(minMaxOrientation.x, minMaxOrientation.y, debugRotation), -90f,0f);
        Debug.Log(bucket.transform.localRotation);
    }
    
    // Check if a point is within a rectangular area
    bool IsWithinRect(Vector3 point, Vector3 midpoint, float distance, float height)
    {
        // Calculate the width of the rectangular area
        float width = Mathf.Sqrt(Mathf.Pow(distance, 2f) - Mathf.Pow(height, 2f));
    
        // Calculate the bounds of the rectangular area
        float minX = midpoint.x - width / 2f;
        float maxX = midpoint.x + width / 2f;
        float minY = midpoint.y - height / 2f;
        float maxY = midpoint.y + height /2f;
       // Check if the point is within the bounds
        return (point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY);
    }

    public void DetectRotation(GameObject obj)
    {
        actualRotation = bucket.transform.localRotation;
        float actualRotationX = actualRotation.x * Mathf.Deg2Rad;
        Debug.Log(actualRotationX);
        if (actualRotationX < -90f && minMaxOrientation.y <actualRotationX)
        {
            isLeft = false;
        }
        else if (actualRotationX > -90f && minMaxOrientation.x < actualRotationX)
        {
            isLeft = true;
        }
    }
    
    //void Start()
    //{
    //    float angleBetweenSpawnPoints = 360f / numberOfSpawnPoints;
    //
    //    for (int i = 0; i < numberOfSpawnPoints; i++)
    //    {
    //        float x = radius * Mathf.Cos(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
    //        float z = radius * Mathf.Sin(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
    //
    //        Vector3 spawnPosition = new Vector3(x, 0f, z) + centerPosition;
    //        GameObject spawnPoint = Instantiate(spawnPointPrefab, spawnPosition, Quaternion.identity);
    //        // Calculate the midpoint between the two points
    //        Vector3 midpoint = (rectPoint1 + rectPoint2) / 2f;
    //
    //        // Calculate the distance between the two points
    //        float distance = Vector3.Distance(rectPoint1, rectPoint2);
    //
    //        // Check if the spawned object is within the rectangular area
    //        if (IsWithinRect(spawnPosition, midpoint, distance, rectHeight))
    //        {
    //            // Calculate the angle between the center position and the spawn point
    //            Vector3 directionToCenter = centerPosition - spawnPosition;
    //            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
    //
    //            // Set the rotation of the spawned object to face towards the center of the circle
    //            spawnPoint.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angleToCenter));
    //        }
    //        else
    //        {
    //            // If the spawned object is not within the rectangular area, destroy it
    //            Destroy(spawnPoint);
    //        }
    //    }
    //}
}
