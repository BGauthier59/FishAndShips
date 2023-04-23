using System;
using Unity.Netcode;
using UnityEngine;

public abstract class MiniGame : NetworkBehaviour
{
    [SerializeField] private string miniGameName;
    public bool isRunning;
    public GameObject miniGameObject;
    [SerializeField] protected Vector3 miniGameCameraPosition;
    [SerializeField] protected Vector3 miniGameCameraEulerAngles;

    public WorkshopManager debugWorkshop;

    public virtual void StartMiniGame()
    {
        miniGameObject.SetActive(true);
    }

    public abstract void ExecuteMiniGame();

    protected void StartExecutingMiniGame()
    {
        isRunning = true;
    }

    protected void StopExecutingMiniGame()
    {
        isRunning = false;
    }

    protected virtual void ExitMiniGame(bool victory)
    {
        Reset();
        WorkshopManager.instance.ExitMiniGame(victory);
        miniGameObject.SetActive(false);
    }

    public (Vector3 pos, Vector3 euler) GetCameraPositionRotation()
    {
        return (miniGameCameraPosition, miniGameCameraEulerAngles);
    }

    [ContextMenu("Set Camera to correct pos")]
    private void SetCameraDebug()
    {
        var (pos, euler) = GetCameraPositionRotation();
        debugWorkshop.miniGameEnvironmentCamera.position = pos;
        debugWorkshop.miniGameEnvironmentCamera.eulerAngles = euler;
    }

    public abstract void Reset(); // Must be used to reset mini-game at initial state when exited

    public abstract void OnLeaveMiniGame(); // Must be called when leave button is clicked

    public virtual void AssociatedWorkshopGetActivated()
    {
    }

    public virtual void AssociatedWorkshopGetDeactivated()
    {
    }
}