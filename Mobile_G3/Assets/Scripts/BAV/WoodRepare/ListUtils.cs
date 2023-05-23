using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtils 
{
    public static void RandomizeList<T>(List<T> list)
    {
        int count = list.Count;
        for (int i = 0; i < count - 1; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, count);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }
}
