using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkMonoSingleton<GameManager>
{
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

    [SerializeField] private int mainMenuIndex;

    [Header("Level Parameters")] [SerializeField]
    private TutorialSO[] tutorials;

    [SerializeField] private int2[] initPlayerPositions = new int2[4];

    [Header("Instances")] private EventsManager eventsManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    private CanvasManager canvasManager;
    private TimerManager timerManager;
    private CameraManager cameraManager;

    private List<PlayerManager> players = new List<PlayerManager>();
    private int hostReadyClientCount;

    private bool needToClick;
    private int hostReadyForTutorialClientCount;

    private NetworkVariable<bool> isGameCancelledBecauseOfDisconnection = new NetworkVariable<bool>();

    #region Start Game Loop

    private void Start()
    {
        UpdateReadyStateServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyStateServerRpc(ulong id)
    {
        if (ConnectionManager.instance.players[id] == null)
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
        cameraManager.StartGameLoop();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].StartGameLoop(initPlayerPositions[i]);
        }

        await CinematicCanvasManager.instance.IntroductionCinematic();

        if (NetworkManager.Singleton.IsHost)
        {
            if (tutorials.Length != 0) StartTutorialHostSide(0);
            else isRunning.Value = true;
        }

        while (!isRunning.Value) await UniTask.Yield();

        Debug.Log("Start game loop!");
        GridControlManager.instance.StartGameLoop();
        shipManager.StartGameLoop();
        workshopManager.StartGameLoop();
        eventsManager.StartGameLoop();
        timerManager.StartGameLoop();
    }

    private void LinkInstance()
    {
        eventsManager = EventsManager.instance;
        workshopManager = WorkshopManager.instance;
        shipManager = ShipManager.instance;
        canvasManager = CanvasManager.instance;
        timerManager = TimerManager.instance;
        cameraManager = CameraManager.instance;

        foreach (var kvp in ConnectionManager.instance.players)
        {
            players.Add(kvp.Value);
        }
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
        eventsManager.UpdateGameLoop();

        foreach (var player in players)
        {
            player.UpdateGameLoop();
        }

        timerManager.UpdateGameLoop();
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
            Debug.LogError("Should not end while running. Maybe two connected workshops were lost.");
        }

        var starCount = shipManager.EvaluateStarScore();
        if (victory)
        {
            LevelManager.instance.UpdateCurrentLevel(true, true, starCount);
        }

        isRunning.Value = false;
        GameEndsClientRpc(victory, starCount);
    }

    [ClientRpc]
    private void GameEndsClientRpc(bool victory, int starCount)
    {
        GameEndsFeedback(victory, starCount);
    }

    private async void GameEndsFeedback(bool victory, int starCount)
    {
        Debug.Log("End of game!");
        await CinematicCanvasManager.instance.EndCinematic();

        canvasManager.DisplayCanvas(CanvasType.EndGame);
        EndOfGameCanvasManager.instance.SetupCanvas(NetworkManager.Singleton.IsHost, victory, starCount);
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
        Debug.LogWarning("You can't move any more");
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

        currentTutorialIndex = tutorialMaxIndex;
    }

    private async void EndTutorial()
    {
        needToClick = false;

        await TutorialManager.instance.DisableTutorial();

        // Send to server that we're done
        UpdateTutorialReadyStateServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTutorialReadyStateServerRpc(ulong id)
    {
        if (ConnectionManager.instance.players[id] == null)
        {
            Debug.LogError("This client does not exist.");
            return;
        }

        Debug.LogWarning(ConnectionManager.instance.players.Count);

        hostReadyForTutorialClientCount++;
        if (hostReadyForTutorialClientCount != ConnectionManager.instance.players.Count) return;
        FinishTutorial();
    }

    private async void FinishTutorial()
    {
        await UniTask.Delay(500);
        isRunning.Value = true;
    }

    public bool IsGameRunning()
    {
        return isRunning.Value || !isGameCancelledBecauseOfDisconnection.Value;
    }

    #endregion
}