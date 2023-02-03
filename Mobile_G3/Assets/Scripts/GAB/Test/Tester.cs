using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {
        
    }

    private void Update()
    {
        var rb = GenericPooling.instance.PoolInstantiate<Rigidbody>(PoolType.ExampleRigidbody, Vector3.up * 10, Quaternion.identity);
    }
}
