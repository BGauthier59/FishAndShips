using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkMonoSingleton<GameManager>
{
    public bool isRunning;

    [Header("Instances")] private EventsManager eventsManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    private CanvasManager canvasManager;
    private TimerManager timerManager;
    private CameraManager cameraManager;

    private List<PlayerManager> players = new List<PlayerManager>();

    private void Start()
    {
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

    public void GameEnds(bool victory)
    {
        // Host-side!
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No client should call GameEnds()");
            return;
        }
        
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
        
        // Todo - Le Host peut choisir de poursuivre la partie ou de couper ?
        // Todo - Le client, pendant ce temps, peut voir des trucs sur la partie, son titre, etc
    }

    private bool isEveryoneDisconnected;
    public void PlayerGetsDisconnected()
    {
        // Should stop game for everyone
        StopGameClientRpc();
    }

    [ClientRpc]
    private void StopGameClientRpc()
    {
        Debug.Log("A client got disconnected. disconnecting the client.");
        NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
    }
}