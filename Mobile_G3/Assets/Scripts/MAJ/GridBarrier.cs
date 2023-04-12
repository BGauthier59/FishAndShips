using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBarrier : MonoBehaviour
{
    public bool isClosed;

    public Transform barrier;
    public Vector3 closedPos,openPos;
    

    private void Update()
    {
        if (isClosed)
        {
            barrier.position = Vector3.Lerp(barrier.position,closedPos,5*Time.deltaTime);
        }
        else
        {
            barrier.position = Vector3.Lerp(barrier.position,openPos,5*Time.deltaTime);
        }
    }
}
