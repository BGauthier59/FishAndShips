using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionManager : NetworkMonoSingleton<ConnectionManager>
{
    private string hostIp;
    public string code;
    public Dictionary<ulong, (PlayerManager player, int id)> players = new ();
    private UnityTransport transport;
    private string ipToConnect;
    public GameState gameState = GameState.Menu;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void Setup()
    {
        players.Clear();
        transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        SetConnectionCallback();
    }

    public string ConnectAsHost()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Already client!");
            return "0";
        }

        hostIp = GetIPv4AddressMobile();
        transport.SetConnectionData(hostIp, transport.ConnectionData.Port);
        bool done = NetworkManager.Singleton.StartHost();
        if (!done)
        {
            Debug.Log("Connection failed.");
        }
        code = StringUtils.NumberToLetterIP(hostIp);
        return code;
    }

    public async void ConnectAsClient(string ip)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Ask for disconnection!");

            NetworkManager.Singleton.Shutdown();
            await UniTask.Delay(1000); // Pas propre ?
        }

        transport.SetConnectionData(StringUtils.LetterToNumberIP(ip), transport.ConnectionData.Port);
        code = ip;
        if(!transport.ConnectionData.ServerEndPoint.IsValid)
        {
            Debug.Log("It seems the IP is not correct. We don't connect");
            return;
        }

        bool done = NetworkManager.Singleton.StartClient();
        if (!done)
        {
            Debug.Log("Connection failed.");
        }
    }

    private void SetConnectionCallback()
    {
        Debug.Log("Set connection callback");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
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
        //Debug.LogError("Client disconnected!");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogError("This is me!");


            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    #endregion

    public void AddPlayerToDictionary(ulong playerId, PlayerManager manager)
    {
        if (players.ContainsKey(playerId))
        {
            Debug.LogError("Player was in dictionary!");
            return;
        }

        Debug.Log($"Adding {manager}, count is {players.Count}");
        int count = players.Count;
        players.Add(playerId, (manager, count));
        Debug.Log($"Player with ID {playerId} has been added to dictionary!");
        MainMenuManager.instance.ClientGetConnected(playerId, manager.playerName.Value.Value,
            manager.playerDataIndex.Value);
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

    public PlayerManager GetClientPlayer()
    {
        foreach (var kvp in players)
        {
            if (kvp.Value.player.IsOwner) return kvp.Value.player;
        }

        Debug.LogWarning("Didn't find your player. This should not happen");
        return null;
    }
}