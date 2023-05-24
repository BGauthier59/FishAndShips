using Unity.Netcode;
using UnityEngine;

public abstract class MiniGame : NetworkBehaviour
{
    [SerializeField] private string miniGameName;
    public bool isRunning;
    public GameObject miniGameObject;
    [SerializeField] protected Vector3 miniGameCameraPosition;
    [SerializeField] protected Vector3 miniGameCameraEulerAngles;

    [SerializeField] protected string indicatorGame;

    public WorkshopManager debugWorkshop;

    public virtual void StartMiniGame()
    {
        miniGameObject.SetActive(true);
        WorkshopManager.instance.SetGameIndicator(indicatorGame);
    }

    public abstract void ExecuteMiniGame();

    protected void StartExecutingMiniGame()
    {
        isRunning = true;
    }

    protected void StopExecutingMiniGame()
    {
        WorkshopManager.instance.StopMiniGameTutorial();
        isRunning = false;
    }

    protected virtual void ExitMiniGame(bool victory)
    {
        Reset();
        if (victory) HonorificManager.instance.AddHonorific(Honorifics.Captain);
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

    public virtual void AssociatedWorkshopGetActivatedHostSide()
    {
        if (!NetworkManager.Singleton.IsHost) Debug.LogError("Host only should call this method!");
    }

    public virtual void AssociatedWorkshopGetDeactivatedHostSide()
    {
        if (!NetworkManager.Singleton.IsHost) Debug.LogError("Host only should call this method!");
    }

    public virtual void TransferDataFromWorkshopWhenMiniGameStarts(Workshop workshop)
    {
    }
}