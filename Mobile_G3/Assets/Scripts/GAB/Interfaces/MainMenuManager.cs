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
using UnityEngine.UI;

public class MainMenuManager : MonoSingleton<MainMenuManager>
{
    [SerializeField] private GameObject[] screenObjects;
    [SerializeField] private GameObject slide;
    [SerializeField] private Image[] slidesButtons;
    [SerializeField] private GameObject connectionDefaultPart;
    [SerializeField] private GameObject connectionClientPart;
    private string ipToConnect;

    [SerializeField] private TMP_Text ipText;
    public string pseudo;
    private float timerFade, delayFade, timerMove, delayMove;
    private Transform fromPos, toPos, objectToMove;

    public Transform cameraMenu;

    public Transform[] camPos;
    public AnimationCurve camCurve;
    public bool fadeIn, fadeOut;
    public Transform fadeTransition;
    public TMP_Text textInputName, textInputIP, levelName, levelIndex;

    [SerializeField] private GameObject[] playerIcons;
    [SerializeField] private PlayerFigures[] playerFigures;
    [SerializeField] private GameObject[] customFigure;

    public int screen;
    public int skinId;
    public bool connected;

    [Serializable]
    private struct CanvasData
    {
        public GameObject canvas;
        public MenuScreen type;
    }

    [Serializable]
    private struct PlayerFigures
    {
        public GameObject[] mapFigures;
    }

    public enum MenuScreen
    {
        TitleScreen,
        Settings,
        Shop,
        Play,
        Customizing
    }

    private void Start()
    {
        // Load data
        SaveManager.instance.SetCurrentData();

        switch (SceneLoaderManager.instance.GetGlobalSceneState())
        {
            case SceneLoaderManager.SceneState.MainMenuFirstTime:
                ConnectionManager.instance.Setup();
                break;
            
            case SceneLoaderManager.SceneState.MainMenuAlreadyConnected:
                // Set correct scene
                Debug.Log("We are alreayd connected! Must set scene in the right way!");
                connected = true;
                SetCurrentScreen(1);
                ConnectionToClientTransition();
                slide.SetActive(true);

                break;
        }
    }

    #region Transition Management

    public void SetCurrentScreen(int newScreen)
    {
        screen = newScreen;
        switch (screen)
        {
            case 0:
                ChangeScreenObject(4);
                break;
            case 1:
                if (!connected)
                {
                    if (connectionClientPart.activeSelf) ClientToConnectionTransition();
                    ChangeScreenObject(1);
                }
                else ChangeScreenObject(3);

                break;
            case 2:
                ChangeScreenObject(5);
                break;
            case 3:
                ChangeScreenObject(6);
                break;
        }
    }

    private async void TitleToOtherScreenTransition()
    {
        StartMovement(camPos[0], camPos[1], cameraMenu, 0.7f);
        await Task.Delay(400);
        StartFade(true, 0.7f);
        await Task.Delay(700);
        cameraMenu.position = camPos[3].position;
        cameraMenu.rotation = camPos[3].rotation;
        screen = 1;
        screenObjects[0].SetActive(false);
        screenObjects[1].SetActive(true);
        slide.SetActive(true);
        StartFade(false, 0.7f);
        await Task.Delay(50);
        StartMovement(camPos[3], camPos[2], cameraMenu, 0.8f);
    }

    private void ChangeScreenObject(int screenObj)
    {
        for (int i = 0; i < screenObjects.Length; i++)
        {
            screenObjects[i].SetActive(false);
        }

        screenObjects[screenObj].SetActive(true);
    }

    private async void PlayTransition()
    {
        for (int i = 0; i < screenObjects.Length; i++)
        {
            screenObjects[i].SetActive(false);
        }

        StartFade(true, 0.7f);
        await Task.Delay(800);
        ConnectionManager.instance.gameState = GameState.Game;

        // todo - set current level
        LevelManager.instance.StartLevel(0);
    }

    private async void ConnectionToClientTransition()
    {
        connectionDefaultPart.SetActive(false);
        StartMovement(camPos[6], camPos[7], camPos[4], 0.5f);
        await Task.Delay(550);
        camPos[4].position = camPos[7].position;
        camPos[5].position = camPos[7].position;
        camPos[4].gameObject.SetActive(false);
        camPos[5].gameObject.SetActive(true);
        StartMovement(camPos[7], camPos[6], camPos[5], 0.5f);
        await Task.Delay(500);
        connectionClientPart.SetActive(true);
    }

    private async void ClientToConnectionTransition()
    {
        connectionClientPart.SetActive(false);
        StartMovement(camPos[6], camPos[7], camPos[5], 0.5f);
        await Task.Delay(550);
        camPos[5].position = camPos[7].position;
        camPos[4].position = camPos[7].position;
        camPos[5].gameObject.SetActive(false);
        camPos[4].gameObject.SetActive(true);
        StartMovement(camPos[7], camPos[6], camPos[4], 0.5f);
        await Task.Delay(500);
        connectionDefaultPart.SetActive(true);
    }

    #endregion

    private void Update()
    {
        if (timerMove > 0)
        {
            timerMove -= Time.deltaTime;
            objectToMove.position = Vector3.Lerp(fromPos.position, toPos.position,
                camCurve.Evaluate(1 - timerMove / delayMove));
            objectToMove.rotation = Quaternion.Lerp(fromPos.rotation, toPos.rotation,
                camCurve.Evaluate(1 - timerMove / delayMove));
        }

        if (timerFade > 0)
        {
            timerFade -= Time.deltaTime;
            if (fadeIn)
                fadeTransition.localPosition = new Vector3(Mathf.Lerp(-2200, 0, 1 - timerFade / delayFade), 0, 0);
            if (fadeOut)
                fadeTransition.localPosition = new Vector3(Mathf.Lerp(0, 2200, 1 - timerFade / delayFade), 0, 0);
        }

        for (int i = 0; i < 4; i++)
        {
            if (i == screen)
            {
                slidesButtons[i].transform.localPosition = new Vector3(
                    slidesButtons[i].transform.localPosition.x,
                    Mathf.Lerp(slidesButtons[i].transform.localPosition.y, 20, Time.deltaTime * 5),
                    slidesButtons[i].transform.localPosition.z);
                slidesButtons[i].color = Color.Lerp(slidesButtons[i].color, Color.white, Time.deltaTime * 5);
            }
            else
            {
                slidesButtons[i].transform.localPosition = new Vector3(
                    slidesButtons[i].transform.localPosition.x,
                    Mathf.Lerp(slidesButtons[i].transform.localPosition.y, 0, Time.deltaTime * 5),
                    slidesButtons[i].transform.localPosition.z);
                slidesButtons[i].color = Color.Lerp(slidesButtons[i].color, Color.grey, Time.deltaTime * 5);
            }
        }

        switch (screen)
        {
            case 0:
                cameraMenu.position = Vector3.Lerp(cameraMenu.position, camPos[10].position, Time.deltaTime * 5);
                cameraMenu.rotation = Quaternion.Lerp(cameraMenu.rotation, camPos[10].rotation, Time.deltaTime * 5);
                break;
            case 1:
                if (!connected)
                {
                    cameraMenu.position = Vector3.Lerp(cameraMenu.position, camPos[2].position, Time.deltaTime * 5);
                    cameraMenu.rotation = Quaternion.Lerp(cameraMenu.rotation, camPos[2].rotation, Time.deltaTime * 5);
                }
                else
                {
                    cameraMenu.position = Vector3.Lerp(cameraMenu.position, camPos[8].position, Time.deltaTime * 5);
                    cameraMenu.rotation = Quaternion.Lerp(cameraMenu.rotation, camPos[8].rotation, Time.deltaTime * 5);
                }

                break;
            case 2:
                cameraMenu.position = Vector3.Lerp(cameraMenu.position, camPos[9].position, Time.deltaTime * 5);
                cameraMenu.rotation = Quaternion.Lerp(cameraMenu.rotation, camPos[9].rotation, Time.deltaTime * 5);
                break;
            case 3:
                cameraMenu.position = Vector3.Lerp(cameraMenu.position, camPos[11].position, Time.deltaTime * 5);
                cameraMenu.rotation = Quaternion.Lerp(cameraMenu.rotation, camPos[11].rotation, Time.deltaTime * 5);
                break;
        }
    }

    void StartMovement(Transform from, Transform to, Transform obj, float time)
    {
        objectToMove = obj;
        fromPos = from;
        toPos = to;
        timerMove = time;
        delayMove = time;
    }

    void StartFade(bool isFadeIn, float time)
    {
        fadeIn = isFadeIn;
        fadeOut = !isFadeIn;
        timerFade = time;
        delayFade = time;
    }

    #region Main Screen

    public void OnPlayButton()
    {
        TitleToOtherScreenTransition();
    }

    public void OnSetName(string pseudo)
    {
        this.pseudo = pseudo;
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

    #endregion

    #region Settings Screen

    public void OnBackFromSettings()
    {
        //SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    #endregion

    #region Customisation Screen

    public void OnChangeSkinForward()
    {
        skinId = (skinId + 1) % 3;
        Debug.Log("SKIN N" + skinId);
        if (connected)
            ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].playerDataIndex.Value =
                skinId;
        for (int i = 0; i < customFigure.Length; i++)
        {
            if (i == skinId) customFigure[i].SetActive(true);
            else customFigure[i].SetActive(false);
        }
    }

    #endregion

    #region Shop Screen

    public void OnBackFromShop()
    {
        //SetCurrentCanvas(MenuScreen.TitleScreen);
    }

    #endregion

    #region Connection Screen

    public void OnHostButton()
    {
        var ip = ConnectionManager.instance.ConnectAsHost();

        // Todo - check if connection was successful ?

        connected = true;
        ChangeScreenObject(3);
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
        connected = true;
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

    public void ClientGetConnected(ulong id, string pseudo, int skin)
    {
        playerIcons[(int) id].SetActive(true);
        playerIcons[(int) id].transform.GetChild(0).GetComponent<TMP_Text>().text = pseudo;
        playerFigures[(int) id].mapFigures[skin].SetActive(true);
    }

    public void ClientSkinChanged(ulong id, int skin)
    {
        int idInt = (int) id;
        Debug.Log("Skin Changed for " + idInt + " at Skin " + skin);
        for (int i = 0; i < playerFigures[idInt].mapFigures.Length; i++)
        {
            if (i == skin) playerFigures[idInt].mapFigures[i].SetActive(true);
            else playerFigures[idInt].mapFigures[i].SetActive(false);
        }
    }

    #endregion
}