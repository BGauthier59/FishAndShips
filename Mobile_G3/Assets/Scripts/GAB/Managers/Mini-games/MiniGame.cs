using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] private string miniGameName;
    public GameObject miniGameObject;
    
    public virtual void StartMiniGame()
    {
        miniGameObject.SetActive(true);
    }

    public abstract void ExecuteMiniGame();

    public virtual void ExitMiniGame(bool victory)
    {
        MiniGameManager.instance.ExitMiniGame(victory);
        miniGameObject.SetActive(false);
    }
}