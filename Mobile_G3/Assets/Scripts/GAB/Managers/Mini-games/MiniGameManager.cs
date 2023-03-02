using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoSingleton<MiniGameManager>
{
    private Workshop currentWorkshop;
    private MiniGame currentMiniGame;
    [SerializeField] private GameObject popUpMiniGame;

    [Header("TEMPORARY")]
    public CircularSwipeManager circularSwipeManager;
    public GyroscopeManager gyroscopeManager;

    public void StartWorkshopInteraction(Workshop workshop)
    {
        currentWorkshop = workshop;
        
        // Todo - envoyer en réseau que le workshop est occupé
        currentWorkshop.isOccupied.Value = true;
        
        // Todo - gérer ici les ateliers qui se jouent à plusieurs ?
        
        StartMiniGame(currentWorkshop.associatedMiniGame);
    }

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
        EndWorkshopInteraction();
    }

    private void EndWorkshopInteraction()
    {
        if (currentWorkshop == null)
        {
            Debug.Log("There is not workshop associated with this mini-game!");
            return;
        }
        
        // Todo - Désactive-t-on systématiquement le workshop ?
        
        currentWorkshop.isOccupied.Value = false;

        currentWorkshop.Deactivate();
    }
}
