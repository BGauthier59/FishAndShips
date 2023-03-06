using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Workshop : MonoBehaviour, IGridEntity
{
    public int positionX, positionY;
    public MiniGame associatedMiniGame;

    public NetworkVariable<bool> isOccupied = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    [SerializeField] protected NetworkVariable<bool> isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private PlayerManager2 playingPlayer;

    private void Start()
    {
        isOccupied.OnValueChanged += OnSetOccupied;
        isActive.OnValueChanged += OnSetActivated;
        InitializeWorkshop();
    }

    protected virtual void InitializeWorkshop()
    {
    }

    public virtual void OnCollision(IGridEntity entity)
    {
        if (!isActive.Value)
        {
            Debug.LogWarning("This workshop is not active!");
            return;
        }

        if (isOccupied.Value)
        {
            Debug.LogWarning("This workshop is already used by someone!");
            return;
        }

        playingPlayer = entity as PlayerManager2;
        MiniGameManager.instance.StartWorkshopInteraction(this);
    }

    public void SetPosition(int posX, int posY)
    {
    }

    public virtual void Activate()
    {
        isActive.Value = true;
    }

    public virtual void Deactivate(bool victory)
    {
        isOccupied.Value = false;
        if (victory) isActive.Value = false;
    }

    #region Network

    private void OnSetOccupied(bool previous, bool current)
    {
        Debug.Log($"{name}'s occupation state has been set to {current} (was {previous})");
    }

    private void OnSetActivated(bool previous, bool current)
    {
        Debug.Log($"{name}'s activation state has been set to {current} (was {previous})");
    }

    #endregion
}