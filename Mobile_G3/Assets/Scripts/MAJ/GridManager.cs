using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class GridManager : MonoSingleton<GridManager>
{
    public int xSize, ySize;
    [SerializeField] private float holdOffset;
    [SerializeField] public List<Tile> grid = new (0);
    public GameObject tilePrefab;
    
    public void OnGenerateGrid()
    {
        grid = new List<Tile>(0);
        for (int i = 0; i < 2; i++)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Tile tile = new Tile
                    {
                        name = i == 0 ? $"Deck {x}, {y}" : $"Hold {x}, {y}"
                    };
                    if (x > 0 && x < xSize - 1 && y > 0 && y < ySize - 1)
                    {
                        tile.transform = Instantiate(tilePrefab, new Vector3(x, 0, i == 0 ? y : y + holdOffset), 
                            Quaternion.identity,transform).transform;
                        tile.transform.name = i == 0 ? $"Deck {x}, {y}" : $"Hold {x}, {y}";
                        tile.floor = tile.transform.GetComponent<GridFloorWalkable>();
                        tile.GetFloor().SetPosition(i == 0 ? x : x+xSize, y);
                    }
                    grid.Add(tile);
                }
            }   
        }
    }
    
    public Tile GetTile(int x, int y)
    {
        int index = x * ySize + y;
        return grid[index];
    }
    
    public Tile GetOppositeTile(int x, int y)
    {
        int index = x >= xSize ? (x - xSize) * ySize + y : (x + xSize) * ySize + y;
        return grid[index];
    }
}

[Serializable]
public class Tile
{
    public string name;
    public Transform transform;
    public Component entity;
    public Component floor;

    public void SetTile(IGridEntity newEntity = null, IGridFloor newFloor = null)
    {
        entity = (Component) newEntity;
        if (newFloor != null) floor = (Component) newFloor;
    }

    public void OnInteraction(IGridEntity collidingEntity, int direction)
    {
        if (entity is IGridEntity gridEntity) gridEntity.OnCollision(collidingEntity,direction);
        else if (floor is IGridFloor gridFloor) gridFloor.OnMove(collidingEntity,direction);
    }

    public IGridEntity GetEntity()
    {
        if (entity is not IGridEntity gridEntity)
        {
            Debug.LogWarning("Entity is not IGridEntity");
            return null;
        }
        return gridEntity;
    }
    
    public IGridFloor GetFloor()
    {
        if(floor is not IGridFloor) Debug.LogWarning("Floor is not IGridFloor");
        return floor as IGridFloor;
    }
}
