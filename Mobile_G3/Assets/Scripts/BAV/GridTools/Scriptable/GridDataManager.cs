using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridDataParameter", menuName = "Grid Data/GridDataManager")]
public class GridDataManager : ScriptableObject
{
    public Tile[] tileGridData;
}