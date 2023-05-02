using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrimpWorkshop : Workshop, IUpdateWorkshop
{
    [SerializeField] private float baseStationaryDuration, randomStationaryDurationGap;
    private float currentStationaryDuration, stationaryTimer;
    private bool isMoving;

    private void Awake()
    {
        Setup();
    }

    public async void Setup()
    {
        while (WorkshopManager.instance == null)
        {
            Debug.Log("Waiting...");
            await Task.Yield();
        }
        
        WorkshopManager.instance.AddUpdatedWorkshop(this);
    }

    public void StartGameLoopHostOnly()
    {
        isActive.OnValueChanged += OnActive;
    }

    public void UpdateGameLoopHostOnly()
    {
        if (!IsActiveOnGrid()) return;
        CheckStationaryTimer();
    }

    #region Displacement

    private void OnActive(bool previous, bool current)
    {
        if (current) SetNewStationaryDuration();
    }

    private void CheckStationaryTimer()
    {
        if (isMoving) return;

        if (stationaryTimer > currentStationaryDuration)
        {
            TryMove();
        }
        else stationaryTimer += Time.deltaTime;
    }

    private void SetNewStationaryDuration()
    {
        currentStationaryDuration = baseStationaryDuration +
                                    Random.Range(-randomStationaryDurationGap, randomStationaryDurationGap);
        stationaryTimer = 0;
        isMoving = false;
    }

    private void TryMove()
    {
        isMoving = true;

        var neighbours = GridManager.instance.GetNeighboursTiles(currentTile);
        if (neighbours.Length == 0)
        {
            Debug.LogError($"{name} has no neighbour and can't move. This should not happen.");
            return;
        }

        var availableTiles = GridManager.instance.FilterTilesNoEntity(neighbours, TileFilter.Walkable);

        if (availableTiles.Length == 0)
        {
            Debug.LogWarning($"{name} has no available neighbour and can't move.");
            SetNewStationaryDuration();
            return;
        }

        Tile randomTile = availableTiles[Random.Range(0, availableTiles.Length)];
        int2 coord = randomTile.GetTilePos();

        MoveClientRpc(coord.x, coord.y);
        
        // Todo - Add move duration ?
        
        SetNewStationaryDuration();
    }

    [ClientRpc]
    private void MoveClientRpc(int newPosX, int newPosY)
    {
        SetPosition(newPosX, newPosY);
    }

    #endregion

    public override void SetPosition(int posX, int posY)
    {
        if (currentTile == null)
        {
            Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null, currentTile.GetFloor());
        
        base.SetPosition(posX, posY);
    }

    protected override void RemoveWorkshopFromGrid()
    {
        // Called by every client when workshop is over
        base.RemoveWorkshopFromGrid();
        if (NetworkManager.Singleton.IsHost)
        {
            EventsManager.instance.RemoveShrimp();
        }
    }

    private bool IsActiveOnGrid()
    {
        if (!isActive.Value || isOccupied.Value || currentTile == null) return false;
        return true;
    }
}