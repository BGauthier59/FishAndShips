using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] private string miniGameName;

    public GameObject miniGameObject;
    public NetworkVariable<bool> isOccupied = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool isOccupiedTemp;
    
    public PlayerManager2 currentPlayingPlayer;
    public MiniGameType type;

    public virtual void StartMiniGame()
    {
        if (isOccupiedTemp) return;
        miniGameObject.SetActive(true);
        isOccupiedTemp = true;
    }

    public abstract void ExecuteMiniGame();

    public virtual void ExitMiniGame(bool victory)
    {
        MiniGameManager.instance.ExitMiniGame(victory);
        miniGameObject.SetActive(false);
        isOccupiedTemp = false;
    }
}