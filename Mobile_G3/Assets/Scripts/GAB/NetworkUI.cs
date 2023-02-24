using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;


public class NetworkUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_Text ipText;
    private string ipToConnect;
    
    #region Buttons

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Already client!");
            return;
        }

        var ip = GetIPv4AddressMobile();
        ipText.text = $"IP: {ip}";
        transport.SetConnectionData(ip, transport.ConnectionData.Port);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("Already client!");
            return;
        }

        // TODO - Check if room full
        
        transport.SetConnectionData(ipToConnect, transport.ConnectionData.Port);
        NetworkManager.Singleton.StartClient();
    }

    public void OnSetIP(string ip)
    {
        ipToConnect = ip;
    }
    
    #endregion
    
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
}