using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public bool isRunning;

    [Header("Instances")] private EventsManager eventsManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    private CanvasManager canvasManager;
    private TimerManager timerManager;
    
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

    private async void GameEnds(bool victory)
    {
        isRunning = false;
        
        Debug.Log("End of game!");
        await CinematicCanvasManager.instance.EndCinematic();

    }
}