using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectionManager : MonoSingleton<ConnectionManager>
{
    public NetworkVariable<Dictionary<int, NetworkObject>> players = new(new Dictionary<int, NetworkObject>(),
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }
    
    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    public void ConnectAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ConnectAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    #region Callbacks

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client with ID {clientId} has been connected!");
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client with ID {clientId} has been disconnected...");
    }

    #endregion
}