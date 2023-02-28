using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Workshop : MonoBehaviour, IGridEntity
{
    public MiniGame associatedMiniGame;
    [SerializeField] private WorkshopType type;
    
    public NetworkVariable<bool> isOccupied = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private PlayerManager2 playingPlayer;

    private void Start()
    {
        InitializeWorkshop();
    }

    public void InitializeWorkshop()
    {
        if(type == WorkshopType.Continuous) Activate();
    }

    public void SetToGrid(int x, int y)
    {
        
    }

    public void OnCollision(IGridEntity other)
    {
        // Todo - start workshop if active
        if (!isActive.Value)
        {
            Debug.LogWarning("This workshop is not active!");
            return;
        }

        if (isOccupied.Value)
        {
            Debug.LogWarning("This workshop is already used by someone!");
            // Ce n'est peut-être pas toujours un problème ?
            return;
        }

        playingPlayer = other as PlayerManager2;
        MiniGameManager.instance.StartWorkshopInteraction(this);
    }

    public void Activate()
    {
        isActive.Value = true;
    }

    public void Deactivate()
    {
        if (type == WorkshopType.Continuous) return;
        isActive.Value = false;
    }
    
    
}
