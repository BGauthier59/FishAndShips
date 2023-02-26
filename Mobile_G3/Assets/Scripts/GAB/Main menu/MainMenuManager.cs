using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoSingleton<MainMenuManager>
{
    [SerializeField] private CanvasData[] canvasesData;

    [SerializeField] private GameObject connectionDefaultPart;
    [SerializeField] private GameObject connectionClientPart;
    private string ipToConnect;

    [SerializeField] private TMP_Text ipText;
    private string pseudo;

    [SerializeField] private GameObject[] playerIcons;

    [Serializable]
    private struct CanvasData
    {
        public GameObject canvas;
        public CanvasType type;
    }

    private enum CanvasType
    {
        TitleScreen, Settings, Shop, Connection, Selection
    }
    
    #region Canvas Management

    private void SetCurrentCanvas(CanvasType type)
    {
        CanvasData? current = null;
        foreach (var data in canvasesData)
        {
            if (data.type != type) continue;
            current = data;
        }

        if (!current.HasValue)
        {
            Debug.LogError("No valid canvas found.");
            return;
        }

        foreach (var data in canvasesData) data.canvas.SetActive(false);
        current.Value.canvas.SetActive(true);
    }

    #endregion

    private void Start()
    {
        SetCurrentCanvas(CanvasType.TitleScreen);
    }

    #region Main Screen

    public void OnPlayButton()
    {
        SetCurrentCanvas(CanvasType.Connection);
    }

    public void OnSetName(string pseudo)
    {
        this.pseudo = pseudo;
    }

    public void OnSettingsButton()
    {
        SetCurrentCanvas(CanvasType.Settings);
    }

    public void OnShopButton()
    {
        SetCurrentCanvas(CanvasType.Shop);
    }

    #endregion

    #region Settings Screen

    public void OnBackFromSettings()
    {
        SetCurrentCanvas(CanvasType.TitleScreen);
    }

    #endregion

    #region Shop Screen

    public void OnBackFromShop()
    {
        SetCurrentCanvas(CanvasType.TitleScreen);
    }

    #endregion

    #region Connection Screen

    public void OnBackFromConnection()
    {
        connectionDefaultPart.SetActive(true);
        connectionClientPart.SetActive(false);
        SetCurrentCanvas(CanvasType.TitleScreen);
    }

    public void OnHostButton()
    {
        var ip = ConnectionManager.instance.ConnectAsHost();
        
        // Todo - check if connection was successful ?
        
        SetCurrentCanvas(CanvasType.Selection);
        ipText.text = $"IP: {ip}";
    }

    public void OnClientButton()
    {
        connectionDefaultPart.SetActive(false);
        connectionClientPart.SetActive(true);
    }

    public void OnSetIp(string ip)
    {
        ipToConnect = ip;
    }

    public void OnJoinButton()
    {
        ConnectionManager.instance.ConnectAsClient(ipToConnect);
    }

    public void ClientJoinedSuccessfully()
    {
        SetCurrentCanvas(CanvasType.Selection);
    }

    #endregion

    #region Selection Screen

    public void OnStartButton()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("The host only can start the game!");
            return;
        }
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single); 
    }

    public void ClientGetConnected(ulong id)
    {
        playerIcons[(int) id].SetActive(true);
    }

    #endregion
}
