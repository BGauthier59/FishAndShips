using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolManager : MonoSingleton<PoolManager>
{
    [Serializable]
    private struct GenericPoolItem
    {
        public PoolType type;
        public Component item;
        public uint count;
    }

    [SerializeField] private GenericPoolItem[] genericPoolItems;
    private Dictionary<PoolType, Queue<Component>> _poolDict;
    [SerializeField] private Transform poolDefaultParent;

    public override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        _poolDict = new Dictionary<PoolType, Queue<Component>>();

        foreach (var item in genericPoolItems)
        {
            var newQueue = new Queue<Component>();

            for (uint i = 0; i < item.count; i++)
            {
                var obj = Instantiate(item.item, transform.position, Quaternion.identity, poolDefaultParent);
                obj.gameObject.SetActive(false);
                newQueue.Enqueue(obj);
            }

            _poolDict.Add(item.type, newQueue);
        }
    }

    public T PoolInstantiate<T>(PoolType type, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        if (!(_poolDict[type].Peek() as T))
        {
            Debug.LogWarning($"Invalid generic type cast! Can't instantiate the object with type {type}");
            return null;
        }

        var obj = _poolDict[type].Dequeue();

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        _poolDict[type].Enqueue(obj);

        return obj as T;
    }
}