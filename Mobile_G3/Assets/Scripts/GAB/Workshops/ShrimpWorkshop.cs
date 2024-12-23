using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ShrimpWorkshop : Workshop, IUpdateWorkshop
{
    [SerializeField] private float baseStationaryDuration, randomStationaryDurationGap;
    private float currentStationaryDuration, stationaryTimer;
    private bool isMoving;

    [SerializeField] private float moveDuration;
    private float moveTimer;
    [SerializeField] private AnimationCurve moveAmplitudeY;

    [SerializeField] private Animation animation;
    [SerializeField] private AnimationClip idle1, idle2, idle3, flip, jump;
    [SerializeField] private UnityEvent moveEvent;

    private void Awake()
    {
        Setup();
    }

    public async void Setup()
    {
        while (WorkshopManager.instance == null)
        {
            await UniTask.Yield();
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

        Tile[] neighbours = GridManager.instance.GetNeighboursTiles(currentTile);
        if (neighbours.Length == 0)
        {
            Debug.LogError($"{name} has no neighbour and can't move. This should not happen.");
            return;
        }

        Tile[] availableTiles = GridManager.instance.FilterTilesNoEntity(neighbours, TileFilter.Walkable);
        if (availableTiles.Length == 0)
        {
            SetNewStationaryDuration();
            return;
        }

        Tile randomTile = availableTiles[Random.Range(0, availableTiles.Length)];
        int2 coord = randomTile.GetTilePos();

        MoveClientRpc(coord.x, coord.y);
        
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
            //Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null);

        base.SetPosition(posX, posY);
    }

    protected override async void MoveToNewTile(Vector3 newPosition)
    {
        currentTile.SetTile(this);

        Vector3 oldPosition = workshopObject.position;
        Quaternion oldRotation = workshopObject.rotation;
        Quaternion nextRotation = SetRotation(newPosition - oldPosition);
        float ratio = 0;
        moveTimer = 0;

        animation.Play(jump.name);
        moveEvent?.Invoke();
        
        while (moveTimer < moveDuration)
        {
            workshopObject.position =
                Vector3.Lerp(oldPosition, newPosition, ratio) +
                                      (Vector3.up * moveAmplitudeY.Evaluate(ratio));
            workshopObject.rotation = Quaternion.Lerp(oldRotation, nextRotation, ratio);
            await UniTask.Yield();
            moveTimer += Time.deltaTime;
            ratio = moveTimer / moveDuration;
        }

        workshopObject.position = feedbackTransform.position = newPosition;
        
        var random = Random.Range(0, 3);
        if (random == 1) animation.Play(idle1.name);
        else if (random == 2) animation.Play(idle2.name);
        else animation.Play(idle3.name);
    }

    private Quaternion SetRotation(Vector3 direction)
    {
        if (direction.x > 0) return Quaternion.Euler(Vector3.up * 270);
        if (direction.x < 0) return Quaternion.Euler(Vector3.up * 90);
        if (direction.z > 0) return Quaternion.Euler(Vector3.up * 180);
        if (direction.z < 0) return Quaternion.Euler(Vector3.zero);

        return Quaternion.identity;
    }
}