using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Workshop : NetworkBehaviour, IGridEntity
{
    public int positionX, positionY;
    public MiniGame associatedMiniGame;

    public NetworkVariable<bool> isOccupied = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    [SerializeField] private bool isActiveByDefault;
    [SerializeField] [Tooltip("WARNING! Might cause unexpected effects!")] private bool enableMultiUsing;
    
    private void Start()
    {
        isOccupied.OnValueChanged += OnSetOccupied;
        isActive.OnValueChanged += OnSetActivated;
        InitializeWorkshop();
    }

    protected virtual void InitializeWorkshop()
    {
        if(isActiveByDefault) Activate();
    }

    public virtual void OnCollision(IGridEntity entity,int direction)
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

        WorkshopManager.instance.StartWorkshopInteraction(this);
    }

    public void SetPosition(int posX, int posY)
    {
    }

    public virtual void Activate()
    {
        associatedMiniGame.AssociatedWorkshopGetActivated();
        SetActiveServerRpc(true);
    }

    public virtual void Deactivate(bool victory)
    {
        associatedMiniGame.AssociatedWorkshopGetDeactivated();
        SetOccupiedServerRpc(false);
        if (victory) SetActiveServerRpc(false);
    }

    #region Network

    [ServerRpc(RequireOwnership = false)]
    public void SetOccupiedServerRpc(bool occupied)
    {
        Debug.Log("RPC set occupied Sent");
        isOccupied.Value = occupied;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void SetActiveServerRpc(bool active)
    {
        Debug.Log("RPC set active Sent");
        isActive.Value = active;
    }

    private void OnSetOccupied(bool previous, bool current)
    {
        Debug.Log($"{name}'s occupation state has been set to {current} (was {previous})");
    }

    private void OnSetActivated(bool previous, bool current)
    {
        Debug.Log($"{name}'s activation state has been set to {current} (was {previous})");
    }
    
    #endregion

    public bool IsMultiUsingEnabled()
    {
        return enableMultiUsing;
    }
}