using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoSingleton<GridManager>
{
    public int xSize, ySize;
    [SerializeField] public List<Tile> tiles = new List<Tile>(0);
    public GameObject tilePrefab;

    public void OnGenerateGrid()
    {
        tiles = new List<Tile>(0);
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Tile tile = new Tile();
                tile.name = "Tile " + x + "," + y;
                if (x > 0 && x < xSize - 1 && y > 0 && y < ySize - 1)
                {
                    tile.type = TileType.Walkable;
                    tile.obj = Instantiate(tilePrefab, new Vector3(x, 0, y), quaternion.identity,transform);
                }
                else
                {
                    tile.type = TileType.Wall;
                    
                }
                tiles.Add(tile);
                
            }
        }
    }

    public bool CheckForMovement(int posX, int posY,GridEntity entity)
    {
        int tileIndex = (posX * ySize) + posY;
        if (tiles[tileIndex].entity == null && tiles[tileIndex].type == TileType.Walkable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveEntity(int posX, int posY)
    {
        int tileIndex = (posX * ySize) + posY;
        tiles[tileIndex].entity = null;
    }
    
    public void AddEntity(int posX, int posY,GridEntity entity)
    {
        int tileIndex = (posX * ySize) + posY;
        tiles[tileIndex].entity = entity;
    }
}

[Serializable]
public class Tile
{
    public string name;
    public TileType type;
    public GameObject obj;
    public GridEntity entity;
}

public enum TileType
{
    Walkable,
    Wall,
    Table
}
