using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class GenericPooling<T> : MonoBehaviour where T : Object
{
    [Serializable]
    public struct PoolObject
    {
        public PoolType type;
        public GameObject pooledObject;
        public int count;
    }

    // Transforms pool objects to generic type

    private struct GenericPoolObject<T1> where T1 : Object
    {
        public PoolType type;
        public T1 genericObject;
        public int count;
    }

    [SerializeField] private PoolObject[] poolObjects;
    private GenericPoolObject<T>[] _genericPoolObject;
    private Dictionary<PoolType, Object> _poolDictionary;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _genericPoolObject = new GenericPoolObject<T>[poolObjects.Length];

        for (int i = 0; i < poolObjects.Length; i++)
        {
            var poolObj = poolObjects[i];
            _genericPoolObject[i] = new GenericPoolObject<T>
            {
                type = poolObj.type,
                count = poolObj.count,
            };
            var genericPoolObj = _genericPoolObject[i];
            var obj = GetPoolObjectType(poolObj);
            genericPoolObj.genericObject = obj as T;

            //var inst = Instantiate((obj.GetType())(genericPoolObj.genericObject), )
        }
    }

    private Object GetPoolObjectType(PoolObject poolObject)
    {
        return poolObject.type switch
        {
            PoolType.ExampleTransform => poolObject.pooledObject.GetComponent<Transform>(),
            PoolType.ExampleRigidbody => poolObject.pooledObject.GetComponent<Rigidbody>(),
            PoolType.ExampleMonoBehaviour => poolObject.pooledObject.GetComponent<MonoBehaviour>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    public void PoolInstantiate<T2>(PoolType type) where T2 : Object { }
}