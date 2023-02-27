using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager2 : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName = new("None", NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Start()
    {
        playerName.OnValueChanged += OnNameChanged;
    }

    #region Network

    public override void OnNetworkSpawn()
    {
        // TODO - Check if can join the room

        if (IsOwner)
        {
            Debug.LogWarning("You've been connected!");
            playerName.Value = MainMenuManager.instance.pseudo;
        }

        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);
        
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) Debug.LogWarning("You've been disconnected");
        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    public void OnNameChanged(FixedString32Bytes previousName, FixedString32Bytes newName)
    {
        Debug.Log("Name value has changed");
        MainMenuManager.instance.ClientGetConnected(OwnerClientId, newName.Value);
    }

    #endregion
}