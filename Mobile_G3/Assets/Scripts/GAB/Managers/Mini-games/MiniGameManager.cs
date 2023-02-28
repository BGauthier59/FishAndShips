using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoSingleton<MiniGameManager>
{
    [SerializeField] private MiniGame[] passiveMiniGames;
    [SerializeField] private MiniGameEvent[] miniGamesEvents;
    private MiniGame currentMiniGame;
    [SerializeField] private GameObject popUpMiniGame;

    [Header("TEMPORARY")]
    public CircularSwipeManager circularSwipeManager;

    public GyroscopeManager gyroscopeManager;

    public void StartMiniGame(MiniGame game)
    {
        currentMiniGame = game;
        Debug.Log($"Started game {game}");
        popUpMiniGame.SetActive(true);
        currentMiniGame.StartMiniGame();
    }

    private void Update()
    {
        if (!currentMiniGame) return;
        currentMiniGame.ExecuteMiniGame();
    }

    public void ExitMiniGame(bool victory)
    {
        Debug.Log($"Exited with {victory}");
        popUpMiniGame.SetActive(false);
        currentMiniGame = null;
    }
}
