using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFireMiniGame : MonoBehaviour
{
    public GameObject spawnPointPrefab;
    public int numberOfSpawnPoints = 8;
    public float radius = 1.45f;
    public Vector3 centerPosition;

    public List<GameObject> spawnPointsFire;

    [Header("Gizmos")] public Vector3 offsetGizmos = new Vector3(0f, 0.5f, 0f);
    
    void Start()
    {
        spawnPointsFire.Clear();
        float angleBetweenSpawnPoints = 360f / numberOfSpawnPoints;
    
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float x = radius * Mathf.Cos(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
            float z = radius * Mathf.Sin(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
            
            Vector3 spawnPosition = new Vector3(x, 0f, z) + centerPosition;
            
            GameObject spawnPoint = Instantiate(spawnPointPrefab, spawnPosition, Quaternion.identity);

            // Calculate the angle between the center position and the spawn point
            Vector3 directionToCenter = centerPosition - spawnPosition;
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
            spawnPoint.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angleToCenter));
            
            spawnPointsFire.Add(spawnPoint);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPosition + offsetGizmos, radius);
        float angleBetweenSpawnPoints = 360f / numberOfSpawnPoints;
        for (int i = 0; i < numberOfSpawnPoints; i++)
        {
            float x = radius * Mathf.Cos(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
            float z = radius * Mathf.Sin(angleBetweenSpawnPoints * i * Mathf.Deg2Rad);
    
            Vector3 spawnPosition = new Vector3(x, 0f, z) + centerPosition;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(spawnPosition + offsetGizmos, 0.1f);
        }
    }
}
