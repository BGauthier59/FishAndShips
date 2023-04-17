using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{

    // Gère les updates de toutes les grosses instances

    // Liste des scripts concernés :

    // ShipManager
    // WorkshopManager
    // EventsManager
    // Players - ???
    // Camera

    public bool isRunning;

    [Header("Instances")] private EventsManager eventsManager;
    private WorkshopManager workshopManager;
    private ShipManager shipManager;
    
    // Comment faire ? On stocke tous les joueurs de la partie ?
    // A chaque spawn d'un player, il est stocké dans une liste de player
    private List<PlayerManager> players = new List<PlayerManager>();

    private void Start()
    {
        StartGameLoop();
    }

    public void StartGameLoop()
    {
        eventsManager = EventsManager.instance;
        workshopManager = WorkshopManager.instance;
        shipManager = ShipManager.instance;

        shipManager.StartGameLoop();
        eventsManager.StartGameLoop();

        foreach (var player in players)
        {
            // Start Game Loop
        }

        isRunning = true;
    }

    public void Update()
    {
        if (!isRunning) return;
        UpdateGameLoop();
    }

    public void UpdateGameLoop()
    {
        shipManager.UpdateGameLoop();
        workshopManager.UpdateGameLoop();
        eventsManager.UpdateGameLoop();

        foreach (var player in players)
        {
            // Update Player
        }
    }
}