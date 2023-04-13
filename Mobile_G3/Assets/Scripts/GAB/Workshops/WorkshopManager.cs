using Unity.Netcode;
using UnityEngine;

public class WorkshopManager : NetworkMonoSingleton<WorkshopManager>
{
    private Workshop currentWorkshop;
    public MiniGame currentMiniGame;

    [SerializeField] private GameObject miniGameRenderingObject;
    public Transform miniGameEnvironmentCamera;
    [SerializeField] private GameObject miniGameRenderingCamera;

    [Header("TEMPORARY")] public RudderCircularSwipeManager rudderCircularSwipeManager;
    public ShrimpSwipeManager shrimpSwipeManager;
    public GyroscopeManager gyroscopeManager;
    public SwipeManager swipeManager;
    public CannonDragAndDropManager cannonDragAndDropManager;

    public void StartWorkshopInteraction(Workshop workshop)
    {
        currentWorkshop = workshop;
        currentWorkshop.SetOccupiedServerRpc(true);

        var seriesWorkshop = workshop as SeriesWorkshop;
        var connectedWorkshop = workshop as ConnectedWorkshop;

        if (seriesWorkshop)
        {
            StartMiniGame(seriesWorkshop.GetCurrentMiniGame());
        }
        else if (connectedWorkshop)
        {
            connectedWorkshop.SetCurrentPlayerIdServerRpc(NetworkManager.Singleton.LocalClientId);
            WaitForOtherPlayerInConnectedWorkshop(connectedWorkshop);
        }
        else StartMiniGame(currentWorkshop.associatedMiniGame);
    }

    private void StartMiniGame(MiniGame game)
    {
        currentMiniGame = game;

        miniGameRenderingObject.SetActive(true);

        var (pos, euler) = currentMiniGame.GetCameraPositionRotation();
        miniGameEnvironmentCamera.position = pos;
        miniGameEnvironmentCamera.eulerAngles = euler;
        miniGameEnvironmentCamera.gameObject.SetActive(true);
        miniGameRenderingCamera.SetActive(true);
        CanvasManager.instance.DisplayCanvas(CanvasType.None);

        currentMiniGame.StartMiniGame();
    }

    private void WaitForOtherPlayerInConnectedWorkshop(ConnectedWorkshop connectedWorkshop)
    {
        if (connectedWorkshop.IsOtherReady())
        {
            Debug.Log($"Connected workshop {connectedWorkshop.name} is ready!");
            // Start mini-game on this client and on other client
            StartConnectedMiniGame(connectedWorkshop.associatedMiniGame);
            InitializeConnectedMiniGameServerRpc(connectedWorkshop.GetOtherWorkshop().GetWorkshopId(),
                connectedWorkshop.GetOtherPlayerId());
        }
        else
        {
            Debug.Log($"Waiting for other connected workshop from {connectedWorkshop.name}...");
            CanvasManager.instance.DisplayCanvas(CanvasType.WorkshopCanvas);
            WorkshopCanvasManager.instance.DisplayWaitingMessage(connectedWorkshop.GetWaitingMessage());
        }
    }

    private void StartConnectedMiniGame(MiniGame miniGame)
    {
        WorkshopCanvasManager.instance.HideWaitingMessage();
        StartMiniGame(miniGame);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeConnectedMiniGameServerRpc(uint otherConnectedWorkshopId, ulong otherPlayerId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {otherPlayerId}
            }
        };
        StartConnectedMiniGameClientRpc(otherConnectedWorkshopId, clientRpcParams);
    }

    [ClientRpc]
    private void StartConnectedMiniGameClientRpc(uint otherConnectedWorkshopId, ClientRpcParams parameters)
    {
        var connectedWorkshop = currentWorkshop as ConnectedWorkshop;

        Debug.Log(NetworkManager.Singleton.LocalClient.ClientId);
        StartConnectedMiniGame(connectedWorkshop.associatedMiniGame);
    }
    
    public void UpdateGameLoop()
    {
        if (!currentMiniGame || !currentMiniGame.isRunning) return;
        currentMiniGame.ExecuteMiniGame();
    }

    public void ExitMiniGame(bool victory)
    {
        Debug.Log($"Exited with {victory}");
        miniGameRenderingObject.SetActive(false);
        miniGameEnvironmentCamera.gameObject.SetActive(false);
        miniGameRenderingCamera.SetActive(true);
        currentMiniGame = null;
        EndWorkshopInteraction(victory);
    }

    public void EndWorkshopInteraction(bool victory)
    {
        if (currentWorkshop == null)
        {
            Debug.LogError("There is not workshop associated with this mini-game!");
            return;
        }

        CanvasManager.instance.DisplayCanvas(CanvasType.ControlCanvas);
        var workshopToDeactivate =
            currentWorkshop; // Must set currentWorkshop to null for series workshop before deactivation
        currentWorkshop = null;
        workshopToDeactivate.Deactivate(victory);
    }

    public Workshop GetCurrentWorkshop()
    {
        return currentWorkshop;
    }
}