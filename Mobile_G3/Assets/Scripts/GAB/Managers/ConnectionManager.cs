using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionManager : MonoSingleton<ConnectionManager>
{
    private string ip;
    public Dictionary<ulong, PlayerManager> players = new();
    private UnityTransport transport;
    private string ipToConnect;
    public GameState gameState = GameState.Menu;

    private void Start()
    {
        transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    public string ConnectAsHost()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Already client!");
            return "0";
        }

        ip = GetIPv4AddressMobile();
        transport.SetConnectionData(ip, transport.ConnectionData.Port);
        NetworkManager.Singleton.StartHost();
        return ip;
    }

    public void ConnectAsClient(string ip)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Already client!");
            return;
        }

        transport.SetConnectionData(ip, transport.ConnectionData.Port);
        NetworkManager.Singleton.StartClient();
    }

    string GetIPv4AddressMobile()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        socket.Connect("8.8.8.8", 65530);

        IPEndPoint endpoint = socket.LocalEndPoint as IPEndPoint;
        IPAddress ipAddress = endpoint.Address;

        var ip = ipAddress.ToString();
        socket.Close();

        return ip;
    }

    #region Callbacks

    private void OnClientConnectedCallback(ulong clientId)
    {
        var infoText = $"Client with ID {clientId} has been connected!";
        if (NetworkManager.Singleton.IsHost)
            infoText += $", number of client(s): {NetworkManager.Singleton.ConnectedClients.Count} ";
        else
        {
            MainMenuManager.instance.ClientJoinedSuccessfully();
        }

        Debug.Log(infoText);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        var infoText = $"Client with ID {clientId} has been disconnected...";
        if (NetworkManager.Singleton.IsHost)
            infoText += $", number of client(s): {NetworkManager.Singleton.ConnectedClients.Count} ";

        Debug.Log(infoText);
    }

    #endregion

    public void AddPlayerToDictionary(ulong playerId, PlayerManager manager)
    {
        if (players.ContainsKey(playerId))
        {
            Debug.LogError("Player was in dictionary!");
            return;
        }

        players.Add(playerId, manager);
        Debug.Log($"Player with ID {playerId} has been added to dictionary!");
        MainMenuManager.instance.ClientGetConnected(playerId, manager.playerName.Value.Value);
    }

    public void RemovePlayerFromDictionary(ulong playerId)
    {
        if (!players.ContainsKey(playerId))
        {
            Debug.LogError("Player was not in dictionary!");
            return;
        }

        players.Remove(playerId);
        Debug.Log($"Player with ID {playerId} has been removed from dictionary!");
    }
}

public enum GameState
{
    Menu,
    Game
}