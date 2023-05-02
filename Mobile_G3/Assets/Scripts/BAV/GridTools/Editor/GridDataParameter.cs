using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridDataParameter", menuName = "Grid Data/GridDataManager")]
public class GridDataManager : ScriptableObject
{
    public Tile[] tileGridData;
}


[CreateAssetMenu(fileName = "GridDeckDataManager", menuName = "Grid Data/GridDeckDataManager")]
public class GridDeckDataManager : ScriptableObject
{
    public Tile[] tileGridDeckData;
}


[CreateAssetMenu(fileName = "GridContainerDataManager", menuName = "Grid Data/GridContainerDataManager")]
public class GridContainerDataManager : ScriptableObject
{
    public Tile[] tileGridContainerData;
}