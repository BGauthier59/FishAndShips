using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : NetworkMonoSingleton<GameManager>
{
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

    [SerializeField] private int mainMenuIndex;

    [Header("Level Parameters")] [SerializeField]
    private TutorialSO[] tutorials;

    [SerializeField] private int2[] initPlayerPositions = new int2[4];

    [Header("Instances")] private EventsManager eventsManager;
    private TutorialEventManager tutorialEventManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    private CanvasManager canvasManager;
    private TimerManager timerManager;
    private CameraManager cameraManager;
    private BarrierManager barrierManager;
    [SerializeField] private bool isTutorialLevel;

    public List<PlayerManager> players = new List<PlayerManager>();
    private int hostReadyClientCount;

    private bool needToClick;
    private int hostReadyForTutorialClientCount;

    private NetworkVariable<bool> isGameCancelledBecauseOfDisconnection = new NetworkVariable<bool>();

    [SerializeField] private UnityEvent startCameraEvent;
    [SerializeField] private UnityEvent startTutorialEvent;
    [SerializeField] private UnityEvent startGameLoopEvent;

    #region Start Game Loop

    private void Start()
    {
        UpdateReadyStateServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyStateServerRpc(ulong id)
    {
        if (ConnectionManager.instance.players[id].player == null)
        {
            Debug.LogError("This client does not exist.");
            return;
        }

        hostReadyClientCount++;
        if (hostReadyClientCount != ConnectionManager.instance.players.Count) return;
        InitializeGameLoopClientRpc();
    }

    [ClientRpc]
    private void InitializeGameLoopClientRpc()
    {
        StartGameLoop();
    }

    private async void StartGameLoop()
    {
        LinkInstance();

        if (barrierManager) barrierManager.StartGameLoop();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].StartGameLoop(initPlayerPositions[i]);
        }

        cameraManager.StartGameLoop();
        startCameraEvent?.Invoke();
        await CinematicCanvasManager.instance.IntroductionCinematic();

        if (NetworkManager.Singleton.IsHost)
        {
            if (tutorials.Length != 0)
            {
                startTutorialEvent?.Invoke();
                StartTutorialHostSide(0);
            }
            else
            {
                await cameraManager.PlayCameraAnimation();
                isRunning.Value = true;
            }
        }
        else
        {
            if (tutorials.Length == 0)
            {
                await cameraManager.PlayCameraAnimation();
            }
        }

        while (!isRunning.Value) await UniTask.Yield();
        TutorialManager.instance.DisableWaitArea();

        await UniTask.Yield();

        Debug.Log("Start game loop!");
        startGameLoopEvent?.Invoke();
        HonorificManager.instance.StartGameLoop();
        GridControlManager.instance.StartGameLoop();
        shipManager.StartGameLoop();
        workshopManager.StartGameLoop();
        if (isTutorialLevel) tutorialEventManager.StartGameLoop();
        else
        {
            timerManager.StartGameLoop();
            eventsManager.StartGameLoop();
        }
    }

    private void LinkInstance()
    {
        foreach (var kvp in ConnectionManager.instance.players)
        {
            players.Add(kvp.Value.player);
        }

        if (isTutorialLevel)
        {
            tutorialEventManager = TutorialEventManager.instance;
            tutorialEventManager.playerNb = players.Count;
        }
        else
        {
            timerManager = TimerManager.instance;
            eventsManager = EventsManager.instance;
        }

        workshopManager = WorkshopManager.instance;
        shipManager = ShipManager.instance;
        canvasManager = CanvasManager.instance;
        cameraManager = CameraManager.instance;
        barrierManager = BarrierManager.instance;
    }

    #endregion

    #region Update Game Loop

    public void Update()
    {
        if (!isRunning.Value) return;
        UpdateGameLoop();
    }

    private void UpdateGameLoop()
    {
        shipManager.UpdateGameLoop();
        workshopManager.UpdateGameLoop();
        if (barrierManager) barrierManager.UpdateGameLoop();

        if (isTutorialLevel) tutorialEventManager.UpdateGameLoop();
        else eventsManager.UpdateGameLoop();

        foreach (var player in players)
        {
            player.UpdateGameLoop();
        }

        if (!isTutorialLevel) timerManager.UpdateGameLoop();
    }

    #endregion

    #region End Game Loop

    public void GameEnds(bool victory)
    {
        // Host-side!
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No client should call GameEnds()");
            return;
        }

        if (!isRunning.Value)
        {
            Debug.LogWarning("Should not end while running. Maybe two connected workshops were lost.");
            return;
        }

        isRunning.Value = false;

        var starCount = shipManager.EvaluateStarScore();

        HonorificManager.instance.InitiateHonorificsResumeClientRpc(victory, starCount);
    }

    [ClientRpc]
    public void GameEndsClientRpc(bool victory, int starCount, long[] honorifics)
    {
        GameEndsFeedback(victory, starCount, honorifics);
    }

    private async void GameEndsFeedback(bool victory, int starCount, long[] honorifics)
    {
        await CinematicCanvasManager.instance.EndCinematic();

        canvasManager.DisplayCanvas(CanvasType.EndGame);
        EndOfGameCanvasManager.instance.SetupCanvas(NetworkManager.Singleton.IsHost, victory, starCount, honorifics);
    }

    #endregion

    #region Disconnection Management

    public void PlayersGetDisconnected()
    {
        if (isGameCancelledBecauseOfDisconnection.Value)
        {
            Debug.Log("Already cancelled");
            return;
        }

        isGameCancelledBecauseOfDisconnection.Value = true;
        isRunning.Value = false;
        NetworkManager.Singleton.Shutdown();
    }

    public bool IsGameCancelledBecauseOfDisconnection()
    {
        return isGameCancelledBecauseOfDisconnection.Value;
    }

    #endregion

    #region Tutorial Management

    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!needToClick) return;
        if (ctx.started)
        {
            if (currentTutorialIndex == tutorialMaxIndex) EndTutorial();
            else needTutorialRefresh = false;
        }
    }

    public void StartTutorialHostSide(int index)
    {
        hostReadyForTutorialClientCount = 0;
        isRunning.Value = false;
        StartTutorialClientRpc(index);
    }

    [ClientRpc]
    private void StartTutorialClientRpc(int index)
    {
        DisplayTutorial(index);
    }

    private int currentTutorialIndex, tutorialMaxIndex;
    private bool needTutorialRefresh;

    private async void DisplayTutorial(int index)
    {
        TutorialSO currentTutorial = tutorials[index];

        tutorialMaxIndex = tutorials[index].tutorials.Length;

        for (int i = 0; i < tutorialMaxIndex; i++)
        {
            currentTutorialIndex = i;
            await TutorialManager.instance.DisplayTutorial(currentTutorial, currentTutorialIndex);
            needToClick = true;
            needTutorialRefresh = true;
            while (needTutorialRefresh) await UniTask.Yield();
        }

        //await TutorialManager.instance.DisplayTutorial(currentTutorial, currentTutorialIndex);
        currentTutorialIndex = tutorialMaxIndex;
    }

    private async void EndTutorial()
    {
        needToClick = false;

        await TutorialManager.instance.DisableTutorial();
        TutorialManager.instance.DisplayWaitArea();

        // Send to server that we're done
        UpdateTutorialReadyStateServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTutorialReadyStateServerRpc(ulong id)
    {
        if (ConnectionManager.instance.players[id].player == null)
        {
            Debug.LogError("This client does not exist.");
            return;
        }
        
        hostReadyForTutorialClientCount++;
        if (hostReadyForTutorialClientCount != ConnectionManager.instance.players.Count) return;
        CameraAnimClientRpc();
    }

    private async void FinishTutorial()
    {
        await UniTask.Delay(500);
        await cameraManager.PlayCameraAnimation();
        if (NetworkManager.Singleton.IsHost) isRunning.Value = true;
    }

    [ClientRpc]
    private void CameraAnimClientRpc()
    {
        FinishTutorial();
    }

    public bool IsGameRunning()
    {
        return isRunning.Value || !isGameCancelledBecauseOfDisconnection.Value;
    }

    #endregion
}