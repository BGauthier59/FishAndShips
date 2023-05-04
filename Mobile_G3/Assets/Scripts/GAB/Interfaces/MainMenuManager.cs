using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
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
    public string pseudo;
    public string scene;
    public float timerFade, delayFade,timerMove,delayMove;
    public Transform fromPos, toPos,objectToMove;

    public Transform cameraMenu;

    public Transform[] camPos;
    public AnimationCurve camCurve;
    public bool fadeIn, fadeOut;
    public Transform fadeTransition;
    public TMP_Text textInputName,textInputIP;

    [SerializeField] private GameObject[] playerIcons;

    [Serializable]
    private struct CanvasData
    {
        public GameObject canvas;
        public MenuScreen type;
    }

    public enum MenuScreen
    {
        TitleScreen, Settings, Shop, Connection, Selection
    }
    
    #region Canvas Management

    private void SetCurrentCanvas(MenuScreen type)
    {
        if (type == MenuScreen.Connection)
        {
            TitleToOtherScreenTransition();
        }
    }
    
    
    public async void TitleToOtherScreenTransition()
    {
        StartMovement(camPos[0], camPos[1],cameraMenu, 0.7f);
        await Task.Delay(400);
        StartFade(true, 0.7f);
        await Task.Delay(700);
        cameraMenu.position = camPos[3].position;
        cameraMenu.rotation = camPos[3].rotation;
        canvasesData[0].canvas.SetActive(false);
        canvasesData[3].canvas.SetActive(true);
        StartFade(false, 0.7f);
        await Task.Delay(50);
        StartMovement(camPos[3], camPos[2],cameraMenu, 0.8f);
    }
    
    public async void ConnectionToClientTransition()
    {
        connectionDefaultPart.SetActive(false);
        StartMovement(camPos[6], camPos[7],camPos[4], 0.5f);
        await Task.Delay(550);
        camPos[4].position = camPos[7].position;
        camPos[5].position = camPos[7].position;
        camPos[4].gameObject.SetActive(false);
        camPos[5].gameObject.SetActive(true);
        StartMovement(camPos[7], camPos[6],camPos[5], 0.5f);
        await Task.Delay(500);
        connectionClientPart.SetActive(true);
        
    }
    
    public async void MapTransition()
    {
        canvasesData[3].canvas.SetActive(false);
        StartMovement(camPos[2], camPos[8],cameraMenu, 0.8f);
        await Task.Delay(800);
        canvasesData[4].canvas.SetActive(true);
        
        
    }
    
    public async void PlayTransition()
    {
        canvasesData[4].canvas.SetActive(false);
        StartFade(true, 0.7f);
        await Task.Delay(800);
        ConnectionManager.instance.gameState = GameState.Game;
        NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single); 
    }

    #endregion

    private void Update()
    {
        if (timerMove > 0)
        {
            timerMove -= Time.deltaTime;
            objectToMove.position = Vector3.Lerp(fromPos.position,toPos.position,camCurve.Evaluate(1-timerMove/delayMove));
            objectToMove.rotation = Quaternion.Lerp(fromPos.rotation,toPos.rotation,camCurve.Evaluate(1-timerMove/delayMove));
        }

        if (timerFade > 0)
        {
            timerFade -= Time.deltaTime;
            if(fadeIn) fadeTransition.localPosition = new Vector3(Mathf.Lerp(-2200, 0, 1 - timerFade / delayFade), 0, 0);
            if(fadeOut) fadeTransition.localPosition = new Vector3(Mathf.Lerp(0, 2200, 1 - timerFade / delayFade), 0, 0);
        }
    }
    
    
    void StartMovement(Transform from,Transform to,Transform obj,float time)
    {
        objectToMove = obj;
        fromPos = from;
        toPos = to;
        timerMove = time;
        delayMove = time;
    }
    
    void StartFade(bool isFadeIn,float time)
    {
        fadeIn = isFadeIn;
        fadeOut = !isFadeIn;
        timerFade = time;
        delayFade = time;
    }

    private void Start()
    {
        SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    #region Main Screen

    public void OnPlayButton()
    {
        SetCurrentCanvas(MenuScreen.Connection);
    }

    public void OnSetName(string pseudo)
    {
        this.pseudo = pseudo;
    }

    public void OnSettingsButton()
    {
        SetCurrentCanvas(MenuScreen.Settings);
    }
    
    public void OnQualitySettings(int setting)
    {
        switch (setting)
        {
            case 0:
                QualitySettings.resolutionScalingFixedDPIFactor = 0.8f;
                break;
            case 1:
                QualitySettings.resolutionScalingFixedDPIFactor = 0.9f;
                break;
            case 2:
                QualitySettings.resolutionScalingFixedDPIFactor = 1f;
                break;
        }
    }

    public void OnShopButton()
    {
        SetCurrentCanvas(MenuScreen.Shop);
    }

    #endregion

    #region Settings Screen

    public void OnBackFromSettings()
    {
        SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    #endregion

    #region Shop Screen

    public void OnBackFromShop()
    {
        SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    #endregion

    #region Connection Screen

    public void OnBackFromConnection()
    {
        
        SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    public void OnHostButton()
    {
        var ip = ConnectionManager.instance.ConnectAsHost();
        
        // Todo - check if connection was successful ?
        
        
        MapTransition();
        ipText.text = $"IP: {ip}";
    }

    public void OnClientButton()
    {
        ConnectionToClientTransition();
    }

    public void OnSetIp(string ip)
    {
        ipToConnect = ip;
    }

    public void OnModifyIp(string ip)
    {
        textInputIP.text = ip;
    }
    public void OnSetPseudo(string pseudotext)
    {
        pseudo = pseudotext;
    }

    public void OnJoinButton()
    {
        ConnectionManager.instance.ConnectAsClient(ipToConnect);
    }

    public void ClientJoinedSuccessfully()
    {
        MapTransition();
        ipText.text = $"IP: {ipToConnect}";
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

        PlayTransition();
        
    }

    public void ClientGetConnected(ulong id, string pseudo)
    {
        playerIcons[(int) id].SetActive(true);
        playerIcons[(int) id].transform.GetChild(0).GetComponent<TMP_Text>().text = pseudo;
    }
    

    #endregion
}
