using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkMonoSingleton<GameManager>
{
    //public bool isRunning;
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

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

        await CinematicCanvasManager.instance.IntroductionCinematic();

        if (NetworkManager.Singleton.IsHost)
        {
            if (tutorials.Length != 0) StartTutorialHostSide(0);
            else isRunning.Value = true;
        }

        while (!isRunning.Value) await Task.Yield();
        
        Debug.Log("Start game loop!");
        shipManager.StartGameLoop();
        workshopManager.StartGameLoop();
        eventsManager.StartGameLoop();
        timerManager.StartGameLoop();
        
        for (int i = 0; i < players.Count; i++)
        {
            players[i].StartGameLoop(initPlayerPositions[i]);
        }
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
        //eventsManager.UpdateGameLoop();

        foreach (var player in players)
        {
            player.UpdateGameLoop();
        }

        timerManager.UpdateGameLoop();
    }

    #endregion

    #region End Game Loop

    public void GameEnds(bool victory, EndGameReason reason)
    {
        // Host-side!
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No client should call GameEnds()");
            return;
        }

        if (victory)
        {
            LevelManager.instance.UpdateCurrentLevel(true, true, 3); // todo set star count
        }

        Debug.LogWarning(reason.ToString());

        isRunning.Value = false;
        GameEndsClientRpc(victory);
    }

    [ClientRpc]
    private void GameEndsClientRpc(bool victory)
    {
        GameEndsFeedback();
    }

    private async void GameEndsFeedback()
    {
        Debug.Log("End of game!");
        await CinematicCanvasManager.instance.EndCinematic();

        // Todo - Le Host peut choisir de poursuivre la partie ou de couper ?
        // Todo - Le client, pendant ce temps, peut voir des trucs sur la partie, son titre, etc
    }

    #endregion

    #region Disconnection Management

    public void PlayerGetsDisconnected()
    {
        // Should stop game for everyone
        isRunning.Value = false;
        StopGameClientRpc();
        NetworkManager.Singleton.Shutdown();
    }

    [ClientRpc]
    private void StopGameClientRpc()
    {
        Debug.Log("A client got disconnected. disconnecting the client.");
        
    }

    #endregion

    #region Tutorial Management

    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (!needToClick) return;
        if (ctx.started) EndTutorial();
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

    private async void DisplayTutorial(int index)
    {
        Debug.LogWarning("You can't move any more");
        TutorialSO data = tutorials[index];

        await TutorialManager.instance.DisplayTutorial(data);

        needToClick = true;
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
        isRunning.Value = true;
    }

    #endregion
}