using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private float timerFade, delayFade, timerMove, delayMove;
    private Transform fromPos, toPos, objectToMove;

    public Transform cameraMenu;
    public Camera cam;

    public Transform[] camPos;
    public AnimationCurve camCurve;
    public bool fadeIn, fadeOut;
    public Transform fadeTransition,shopTag,shopPassNames;
    public TMP_Text textInputName, textInputIP, levelName, levelIndex,skinText,impactText,skinLockedText;
    [SerializeField] public LevelIcon[] levelButtons;

    [SerializeField] private GameObject[] playerIcons;
    [SerializeField] private PlayerFigures[] playerFigures;
    [SerializeField] private GameObject[] customFigure;
    [SerializeField] private ParticleSystem[] customImpact;
    [SerializeField] private SpriteRenderer[] starsPass;
    [SerializeField] private SpriteRenderer[] skinIcons;
    [SerializeField] private SpriteRenderer[] impactIcons;

    public string pseudo;
    public int screen;
    public int skinId,skinCustId,impactId,impactCustId;
    public bool connected;
    public int levelId;
    public int starNb;
    public int[] skinRefs;
    
    public Vector3 startTouch;
    public bool isDragging,isOnMap,isOnLevel;
    public float startPosLevels,startPosPass;
    public Transform levelsTransform,startButton,returnButton,treasurePass;
    public GameObject levelScreen;
    public float minX,maxX,forcemultiplier,timeOfTap;
    public LayerMask maskMap,maskCust;
    public Color starLockedColor,starLinkUnlocked;
    public Animation mapTransition,shopTransition,customTransition;
    public Transform[] buttonsCustomScreen;
    public string[] skinNames, impactNames;
    [SerializeField] private PassReward[] rewards;
    public SpriteRenderer skinBox,impactBox;

    [Serializable]
    public struct LevelIcon
    {
        public Transform button;
        public TMP_Text text;
        public Material[] stars;
        public MeshRenderer[] renderers;
    }
    
    [Serializable]
    private struct PassReward
    {
        public SpriteRenderer box,arrow;
        public int starsRequired;
        public bool skin;
        public int nb;
        public float shopPos;
    }

    [Serializable]
    private struct PlayerFigures
    {
        public GameObject[] mapFigures;
    }

    private void Start()
    {
        // Load data
        SaveManager.instance.SetCurrentData();
        SetupStars();
        
        switch (SceneLoaderManager.instance.GetGlobalSceneState())
        {
            case SceneLoaderManager.SceneState.MainMenuFirstTime:
                ConnectionManager.instance.Setup();
                break;
            
            case SceneLoaderManager.SceneState.MainMenuAlreadyConnected:
                // Set correct scene
                Debug.Log("We are already connected! Must set scene in the right way!");
                connected = true;
                ipText.text = $"Code : {ConnectionManager.instance.code}";
                cameraMenu.position = camPos[3].position;
                cameraMenu.rotation = camPos[3].rotation;
                skinId = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].playerDataIndex.Value;
                impactId = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].impactDataIndex.Value;
                for (int i = 0; i < customFigure.Length; i++)
                {
                    if (i == skinId) customFigure[i].SetActive(true);
                    else customFigure[i].SetActive(false);
                }
                int x = 0;
                foreach (var value in ConnectionManager.instance.players)
                {
                    playerIcons[x].SetActive(true);
                    playerIcons[x].transform.GetChild(0).GetComponent<TMP_Text>().text = value.Value.playerName.Value.ToString();
                    playerFigures[x].mapFigures[value.Value.playerDataIndex.Value].SetActive(true);
                    x++;
                }
                skinText.text = skinNames[skinId];
                impactText.text = impactNames[skinId];
                SetCurrentScreen(1);
                slide.SetActive(true);
                break;
        }
    }

    void SetupStars()
    {
        starNb = 0;
        for (int i = 1; i < levelButtons.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                levelButtons[i].stars[j] = new Material(levelButtons[i].renderers[j].material);
                levelButtons[i].renderers[j].material = levelButtons[i].stars[j];
                if (LevelManager.instance.allLevels[i].starCount > j)
                {
                    levelButtons[i].stars[j].SetColor("_Color",Color.white);
                    starNb++;
                }
                else levelButtons[i].stars[j].SetColor("_Color",starLockedColor);
            }
        }
        
        for (int i = 0; i < starNb; i++)
        {
            starsPass[i].color = Color.white;
            starsPass[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = starLinkUnlocked;
        }

        for (int i = 0; i < rewards.Length; i++)
        {
            if (starNb >= rewards[i].starsRequired)
            {
                rewards[i].arrow.color = Color.white;
                rewards[i].box.color = Color.white;
            }
        }
    }

    #region Transition Management

    public async void SetCurrentScreen(int newScreen)
    {
        if (screen == 2 && newScreen != 2)
        {
            shopTransition.Play("ShopCloseTransition");
        }
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
                else
                {
                    for (int i = 0; i < screenObjects.Length; i++)
                    {
                        screenObjects[i].SetActive(false);
                    }
                    
                }

                break;
            case 2:
                ChangeScreenObject(5);
                await Task.Delay(300);
                shopTransition.Play("ShopOpenTransition");
                treasurePass.localPosition = new Vector3(1.09f, treasurePass.localPosition.y, treasurePass.localPosition.z);
                shopPassNames.localPosition = shopPassNames.parent.InverseTransformPoint(shopTag.position);
                break;
            case 3:
                ChangeScreenObject(6);
                break;
        }
    }

    private async void TitleToOtherScreenTransition()
    {
        screenObjects[0].SetActive(false);
        StartMovement(camPos[0], camPos[1], cameraMenu, 0.7f);
        await Task.Delay(400);
        StartFade(true, 0.7f);
        await Task.Delay(700);
        cameraMenu.position = camPos[3].position;
        cameraMenu.rotation = camPos[3].rotation;
        screen = 1;
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
        
        LevelManager.instance.StartLevel(levelId);
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

        if (connected && screen == 1)
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i].button.position.x < 137 || levelButtons[i].button.position.x > 138.9f)
                {
                    levelButtons[i].text.color = Color.Lerp(levelButtons[i].text.color,new Color(0, 0, 0, 0),Time.deltaTime*8);
                }
                else
                {
                    levelButtons[i].text.color = Color.Lerp(levelButtons[i].text.color,Color.white, Time.deltaTime*8);
                }
            }

            if (isDragging)
            {
                float movement = Input.mousePosition.x - startTouch.x;
                levelsTransform.position = new Vector3(Mathf.Clamp(startPosLevels + (movement / ((float) Screen.width / 2))*forcemultiplier,minX,maxX), levelsTransform.position.y, levelsTransform.position.z);
            }
        }

        if (screen == 2 && isDragging)
        {
            float movement = Input.mousePosition.x - startTouch.x;
            treasurePass.localPosition = new Vector3(Mathf.Clamp(startPosPass + (movement / ((float) Screen.height / 2))*0.5f,-1.7f,1.09f), treasurePass.localPosition.y, treasurePass.localPosition.z);
            shopPassNames.localPosition = shopPassNames.parent.InverseTransformPoint(shopTag.position);
            shopPassNames.localPosition = new Vector3(Mathf.Clamp(shopPassNames.localPosition.x, -1.186f, 100),shopPassNames.localPosition.y, shopPassNames.localPosition.z);
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

    public async void OnSkinModification()
    {
        int refs = skinCustId == 3 ? 0 : skinCustId == 4 ? 4 : -1;
        customTransition.Play("ChangeSkin");
        await Task.Delay(160);
        if (refs != -1 && starNb >= rewards[refs].starsRequired)
        {
            skinId = skinCustId;
            if (connected)
                ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].playerDataIndex.Value =
                    skinId;
            skinLockedText.gameObject.SetActive(false);
            skinBox.color = Color.white;
            skinIcons[skinCustId].color = Color.white;
        }
        else if(refs != -1)
        {
            skinLockedText.gameObject.SetActive(true);
            skinLockedText.text = "You need "+(rewards[refs].starsRequired - starNb)+" more stars to unlock this Skin";
            skinBox.color = starLockedColor;
            skinIcons[skinCustId].color = Color.gray;
        }
        else
        {
            skinId = skinCustId;
            if (connected)
                ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].playerDataIndex.Value =
                    skinId;
            skinLockedText.gameObject.SetActive(false);
            skinBox.color = Color.white;
            skinIcons[skinCustId].color = Color.white;
        }
        for (int i = 0; i < customFigure.Length; i++)
        {
            if (i == skinCustId)
            {
                customFigure[i].SetActive(true);
                skinIcons[i].gameObject.SetActive(true);
            }
            else
            {
                customFigure[i].SetActive(false);
                skinIcons[i].gameObject.SetActive(false);
            }
        }
        skinText.text = skinNames[skinCustId];
    }
    
    public async void OnImpactModification()
    {
        int refs = impactCustId == 1 ? 1 : impactCustId == 2 ? 3 : impactCustId == 3 ? 2 : impactCustId == 4 ? 5 : -1;
        customTransition.Play("ChangeImpact");
        await Task.Delay(160);
        if (refs != -1 && starNb >= rewards[refs].starsRequired)
        {
            impactId = impactCustId;
            if (connected)
                ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].impactDataIndex.Value =
                    impactId;
            skinLockedText.gameObject.SetActive(false);
            impactBox.color = Color.white;
            impactIcons[impactCustId].color = Color.white;
        }
        else if(refs != -1)
        {
            skinLockedText.gameObject.SetActive(true);
            skinLockedText.text = "You need "+(rewards[refs].starsRequired - starNb)+" more stars to unlock this Impact";
            impactBox.color = starLockedColor;
            impactIcons[impactCustId].color = Color.gray;
        }
        else
        {
            impactId = impactCustId;
            if (connected)
                ConnectionManager.instance.players[NetworkManager.Singleton.LocalClient.ClientId].impactDataIndex.Value =
                    impactId;
            skinLockedText.gameObject.SetActive(false);
            impactBox.color = Color.white;
            impactIcons[impactCustId].color = Color.white;
        }
        for (int i = 0; i < impactIcons.Length; i++)
        {
            if (i == impactCustId)
            {
                impactIcons[i].gameObject.SetActive(true);
            }
            else
            {
                impactIcons[i].gameObject.SetActive(false);
            }
        }
        customImpact[impactCustId].Play();
        impactText.text = impactNames[impactCustId];
    }
    
    
    public void OnChangeSkinForward()
    {
        skinCustId = (skinCustId + 1) % 5;
        OnSkinModification();

    }
    
    public void OnChangeSkinBackward()
    {
        skinCustId = skinCustId - 1;
        if (skinCustId < 0) skinCustId = 4;
        OnSkinModification();
    }
    
    public void OnChangeImpactForward()
    {
        impactCustId = (impactCustId + 1) % 5;
        OnImpactModification();
    }
    
    public void OnChangeImpactBackward()
    {
        impactCustId = impactCustId - 1;
        if (impactCustId < 0) impactCustId = 4;
        OnImpactModification();
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

        for (int i = 0; i < screenObjects.Length; i++)
        {
            screenObjects[i].SetActive(false);
        }
        
        connected = true;
        
        ipText.text = $"Code : {ip}";
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
        for (int i = 0; i < screenObjects.Length; i++)
        {
            screenObjects[i].SetActive(false);
        }
        ipText.text = $"Code: {ipToConnect}";
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
    
    public async void OnReturnButton()
    {
        isOnLevel = false;
        mapTransition.Play("LevelToMapTransition");
        await Task.Delay(500);
        isOnMap = true;
    }

    public async void OnLevelSelected(int level)
    {
        isOnMap = false;
        mapTransition.Play("MapToLevelTransition");
        levelId = level;
        levelIndex.text = LevelManager.instance.allLevels[levelId].so.levelName;
        levelName.text = LevelManager.instance.allLevels[levelId].so.levelDescription;
        if(NetworkManager.Singleton.IsHost) startButton.gameObject.SetActive(true);
        else startButton.gameObject.SetActive(false);
        await Task.Delay(500);
        isOnLevel = true;
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

    #region Controls

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            startTouch = Input.mousePosition;
            isDragging = true;
            startPosLevels = levelsTransform.transform.position.x;
            startPosPass = treasurePass.transform.localPosition.x;
            timeOfTap = Time.time;
        }
        else if (ctx.canceled)
        {
            isDragging = false;
            if (Time.time - timeOfTap < 0.2f)
            {
                OnClick();
            }
        }
    }

    void OnClick()
    {
        if (connected && screen == 1)
        {
            Ray ray = cam.ScreenPointToRay(startTouch);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, maskMap))
            {
                if (isOnMap)
                {
                    for (int i = 0; i < levelButtons.Length; i++)
                    {
                        if (levelButtons[i].button == hit.transform)
                        {
                            OnLevelSelected(i);
                        }
                    }   
                }
                else if (isOnLevel)
                {
                    if(startButton == hit.transform) OnStartButton();
                    else if(returnButton == hit.transform) OnReturnButton();
                }
            }
        }

        if (screen == 0)
        {
            Ray ray = cam.ScreenPointToRay(startTouch);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, maskCust))
            {
                for (int i = 0; i < buttonsCustomScreen.Length; i++)
                {
                    if (buttonsCustomScreen[i] == hit.transform)
                    {
                        switch (i)
                        {
                            case 0:
                                OnChangeSkinForward();
                                break;
                            case 1:
                                OnChangeSkinBackward();
                                break;
                            case 2:
                                OnChangeImpactForward();
                                break;
                            case 3:
                                OnChangeImpactBackward();
                                break;
                        }
                    }
                }
            }
        }
    }
    

    #endregion
}