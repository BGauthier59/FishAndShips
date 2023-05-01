using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public bool isRunning;

    [Header("Instances")] private EventsManager eventsManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    private CanvasManager canvasManager;
    private TimerManager timerManager;
    private CameraManager cameraManager;

    private List<PlayerManager> players = new List<PlayerManager>();
    public static Action<bool> onGameEnds;

    private void Start()
    {
        onGameEnds += GameEnds;
        StartGameLoop();
    }

    private async void StartGameLoop()
    {
        LinkInstance();
        cameraManager.StartGameLoop();

        await CinematicCanvasManager.instance.IntroductionCinematic();

        Debug.Log("Start game loop!");
        shipManager.StartGameLoop();
        eventsManager.StartGameLoop();
        timerManager.StartGameLoop();

        foreach (var player in players)
        {
            player.StartGameLoop();
        }

        isRunning = true;
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

    public void Update()
    {
        if (!isRunning) return;
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

    private void GameEnds(bool victory)
    {
        // Host-side!
        GameEndsClientRpc(victory);
    }

    [ClientRpc]
    private void GameEndsClientRpc(bool victory)
    {
        Debug.Log("You won!!!");
        isRunning = false;
        GameEndsFeedback();
    }

    private async void GameEndsFeedback()
    {
        Debug.Log("End of game!");
        await CinematicCanvasManager.instance.EndCinematic();
    }
}