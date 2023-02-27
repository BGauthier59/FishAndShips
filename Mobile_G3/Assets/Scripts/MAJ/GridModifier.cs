using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridModifier : MonoBehaviour
{
    public GridManager gridManager;
    public bool generate;
    
    private void OnDrawGizmos()
    {
        if (generate)
        {
            generate = false;
            gridManager.OnGenerateGrid();
        }
    }
}
