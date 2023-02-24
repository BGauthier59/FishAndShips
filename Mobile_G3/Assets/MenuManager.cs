using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public MenuScreen menuScreen;
    public GameObject[] canvases;
    public NetworkUI networkUI;
    [SerializeField] public PlayerIcon[] icons;
    public static MenuManager instance;
    public TMP_InputField nameInput;


    public void Awake()
    {
        instance = this;
    }

    public void OnPlayButtonClicked()
    {
        if(menuScreen == MenuScreen.Title)
        {
            menuScreen = MenuScreen.Connection;
            canvases[0].SetActive(false);
            canvases[1].SetActive(true);
        }
    }
    
    public void OnBackButtonClicked()
    {
        switch (menuScreen)
        {
            case MenuScreen.Connection:
                menuScreen = MenuScreen.Title;
                canvases[1].SetActive(false);
                canvases[0].SetActive(true);
                break;
            
            case MenuScreen.Client:
                menuScreen = MenuScreen.Connection;
                canvases[3].SetActive(false);
                canvases[2].SetActive(true);
                break;
        }
    }
    
    public void OnHostButtonClicked()
    {
        if(menuScreen == MenuScreen.Connection)
        {
            menuScreen = MenuScreen.Selection;
            networkUI.StartHost();
            canvases[3].SetActive(false);
            canvases[2].SetActive(true);
            canvases[1].SetActive(false);
            canvases[4].SetActive(true);
        }
    }
    
    public void OnClientButtonClicked()
    {
        if(menuScreen == MenuScreen.Connection)
        {
            menuScreen = MenuScreen.Client;
            canvases[2].SetActive(false);
            canvases[3].SetActive(true);
        }
    }
    
    public void OnJoinButtonClicked()
    {
        if(menuScreen == MenuScreen.Client)
        {
            menuScreen = MenuScreen.Selection;
            networkUI.StartClient();
            canvases[3].SetActive(false);
            canvases[2].SetActive(true);
            canvases[1].SetActive(false);
            canvases[4].SetActive(true);
        }
    }
}

public enum MenuScreen
{
    Title,
    Connection,
    Client,
    Selection
}

[Serializable]
public class PlayerIcon
{
    public GameObject obj;
    public TMP_Text nameText;
}