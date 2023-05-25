using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GridManager : MonoSingleton<GridManager>
{
    public int xSize, ySize;
    [SerializeField] private float holdOffset;
    [SerializeField] public List<Tile> grid = new(0);
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
                            Quaternion.identity, transform).transform;
                        tile.transform.name = i == 0 ? $"Deck {x}, {y}" : $"Hold {x}, {y}";
                        tile.floor = tile.transform.GetComponent<GridFloorWalkable>();
                        tile.GetFloor().SetPosition(i == 0 ? x : x + xSize, y);

                        tile.SetCoordinates(x, y);
                    }

                    grid.Add(tile);
                }
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        int index = x * ySize + y;
        if (index > grid.Count - 1)
        {
            Debug.LogError("Tile is not in list. Should not happen.");
            return null;
        }

        return grid[index];
    }

    public Tile GetOppositeTile(int x, int y)
    {
        int index = x >= xSize ? (x - xSize) * ySize + y : (x + xSize) * ySize + y;
        return grid[index];
    }
    
    public Tile GetRandomTile(int2[] tiles)
    {
        Tile targetedTile;
        int2 randomCoord;
        int securityCount = 0;

        if (tiles.Length == 0)
        {
            Debug.LogWarning("Array is empty. Please don't forget to fill the array.");
            return null;
        }
        do
        {
            randomCoord = tiles[Random.Range(0, tiles.Length)];
            targetedTile = GetTile(randomCoord.x, randomCoord.y);
            
            securityCount++;
            if (securityCount == 100)
            {
                Debug.LogWarning("Didn't find any tile after trying 100 times. Returns null.");
                return null;
            }
            
        } while (targetedTile == null || 
                 targetedTile.GetEntity() != null);

        return targetedTile;
        
        /*
        int randomX, randomY;

        do
        {
            randomX = Random.Range(0, xSize);
            randomY = Random.Range(0, ySize);
            targetedTile = GetTile(randomX, randomY);

            securityCount++;
            if (securityCount == 100)
            {
                Debug.LogWarning("Didn't find any tile after trying 100 times. Returns null.");
                return null;
            }
        } while (targetedTile == null || 
                 targetedTile.GetEntity() != null ||
                 targetedTile.GetFloor() is not GridFloorWalkable ||
                 !((GridFloorWalkable) targetedTile.GetFloor()).CanBeTargeted());

        return targetedTile;
        */
    }

    public Tile[] GetNeighboursTiles(Tile current)
    {
        List<Tile> tiles = new List<Tile>();

        Tile rightTile = GetNeighbourTile(1, 0);
        Tile leftTile = GetNeighbourTile(-1, 0);
        Tile upTile = GetNeighbourTile(0, 1);
        Tile downTile = GetNeighbourTile(0, -1);

        if (rightTile != null) tiles.Add(rightTile);
        if (leftTile != null) tiles.Add(leftTile);
        if (upTile != null) tiles.Add(upTile);
        if (downTile != null) tiles.Add(downTile);

        Tile GetNeighbourTile(int xOffset, int yOffset)
        {
            int2 currentPos = current.GetTilePos();
            return GetTile(currentPos.x + xOffset, currentPos.y + yOffset);
        }

        return tiles.ToArray();
    }

    public Tile[] FilterTilesNoEntity(Tile[] tileToFilter, params TileFilter[] filters)
    {
        List<Tile> filteredTiles = new List<Tile>();

        foreach (var tile in tileToFilter)
        {
            if (tile.GetEntity() != null) continue;
            foreach (var filter in filters)
            {
                if (filter == TileFilter.Walkable)
                {
                    if (tile.GetFloor() is GridFloorWalkable) filteredTiles.Add(tile);
                }
            }
        }

        return filteredTiles.ToArray();
    }
}

[Serializable]
public class Tile
{
    public string name;
    public Transform transform;
    public Component entity;
    public Component floor;
    public int2 coordinates;

    public void SetCoordinates(int x, int y)
    {
        coordinates = new int2(x, y);
    }

    public int2 GetTilePos()
    {
        return coordinates;
    }

    public void SetTile(IGridEntity newEntity = null, IGridFloor newFloor = null)
    {
        entity = (Component) newEntity;
        if (newFloor != null) floor = (Component) newFloor;
    }

    public void OnInteraction(IGridEntity collidingEntity, int direction)
    {
        if (entity is IGridEntity gridEntity) gridEntity.OnCollision(collidingEntity, direction);
        else if (floor is IGridFloor gridFloor) gridFloor.OnMove(collidingEntity, direction);
    }

    public IGridEntity GetEntity()
    {
        if (entity is not IGridEntity gridEntity) return null;
        return gridEntity;
    }

    public IGridFloor GetFloor()
    {
        return floor as IGridFloor;
    }
}