using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodMGManager : MonoBehaviour
{
    public Transform folderWood;
    public GameObject[] woodPlates;
    public bool oneSpawnPoint;
    public List<GameObject> woodSpawnPoints;

    [Header("Control Pools")]
    public float poolSize = 10;
    private List<GameObject>[] poolWoodObject;
    private int[] poolIndexList;
    private int poolIndex = 0;

    private float[] randomValueList;

    // Start is called before the first frame update
    void Awake()
    {
        poolWoodObject = new List<GameObject>[woodPlates.Length];
        poolIndexList = new int[woodPlates.Length];
        for (int i = 0; i < woodPlates.Length; i++)
        {
            poolWoodObject[i] = new List<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = Instantiate(woodPlates[i], folderWood.transform, true);
                obj.SetActive(false);
                poolWoodObject[i].Add(obj);
            }
        }
        GenerateRandomRotationValue(10, 0,360);
    }

    public void Start()
    {
        StartSpawnWood();
    }

    void StartSpawnWood(bool multiple = false, bool randomRotation = false)
    {
        
        if (!multiple)
        {
            for (int i = 0; i < woodSpawnPoints.Count; i++)
            {
                int randomPoolIndex = UnityEngine.Random.Range(0, poolWoodObject.Length);
                GameObject newWoodPlat = GetObjectFromPool(randomPoolIndex);
                newWoodPlat.transform.position = woodSpawnPoints[i].transform.position;
                newWoodPlat.transform.Rotate(new Vector3(0,0,randomValueList[UnityEngine.Random.Range(0,10)]));
                newWoodPlat.SetActive(true);
            }
        }
        else
        {
            int randomPoolIndex = UnityEngine.Random.Range(0, poolWoodObject.Length);
            GameObject newWoodPlat = GetObjectFromPool(randomPoolIndex);
            newWoodPlat.transform.position = woodSpawnPoints[0].transform.position;
            newWoodPlat.transform.rotation = woodSpawnPoints[0].transform.rotation * UnityEngine.Random.rotation;
            newWoodPlat.transform.Rotate(new Vector3(0,0,randomValueList[UnityEngine.Random.Range(0,10)]));
            newWoodPlat.SetActive(true);
        }
    }

    GameObject GetObjectFromPool(int index)
    {
        List<GameObject> objectPool = poolWoodObject[index];
        poolIndex = poolIndexList[index];
        GameObject obj = objectPool[poolIndex];
        poolIndex++;
        if (poolIndex >= objectPool.Count)
        {
            poolIndex = 0;
        }
        poolIndexList[index] = poolIndex;
        return obj;
    }

    void GenerateRandomRotationValue(int number, float minValue, float maxValue)
    {
        randomValueList = new float[number];
        for (int i = 0; i < number; i++)
        {
            float randomValue = UnityEngine.Random.Range(minValue, maxValue);
            randomValueList[i] = randomValue;
        }
    }
    
    //TODO : Faire un système qui créé un tableau de 100 value et on va piocher dedans et ça évite d'utiliser le random.
}
