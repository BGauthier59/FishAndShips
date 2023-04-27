using Unity.Netcode;
using UnityEngine;

public class WorkshopManager : NetworkMonoSingleton<WorkshopManager>
{
    private Workshop currentWorkshop;
    public MiniGame currentMiniGame;

    public Transform miniGameEnvironmentCamera;

    [Header("TEMPORARY")] public RudderCircularSwipeManager rudderCircularSwipeManager;
    public ShrimpSwipeManager shrimpSwipeManager;
    public GyroscopeManager gyroscopeManager;
    public SwipeManager swipeManager;
    public CannonDragAndDropManager cannonDragAndDropManager;
    public MapSwipeManager mapSwipeManager;

    #region Game Loop

    public void UpdateGameLoop()
    {
        if (!currentMiniGame || !currentMiniGame.isRunning) return;
        currentMiniGame.ExecuteMiniGame();
    }

    #endregion

    #region Main methods

    public void StartWorkshopInteraction(Workshop workshop)
    {
        var requiredItem = workshop.TryGetWorkshopRequireItem();
        if (requiredItem.HasValue)
        {
            if (workshop.GetCurrentPlayer().GetInventoryObject() != requiredItem.Value)
            {
                Debug.LogWarning("You don't owe the required item!");
                return;
            }
        }

        var seriesWorkshop = workshop as SeriesWorkshop;
        var connectedWorkshop = workshop as ConnectedWorkshop;
        currentWorkshop = workshop;

        if (!workshop.IsMultiUsingEnabled()) currentWorkshop.SetOccupiedServerRpc(true);

        if (seriesWorkshop)
        {
            Debug.Log($"Get safe : {seriesWorkshop.GetCurrentMiniGameSafe().name}");
            StartMiniGame(seriesWorkshop.GetCurrentMiniGameSafe());
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

        var (pos, euler) = currentMiniGame.GetCameraPositionRotation();
        miniGameEnvironmentCamera.position = pos;
        miniGameEnvironmentCamera.eulerAngles = euler;
        miniGameEnvironmentCamera.gameObject.SetActive(true);
        CanvasManager.instance.DisplayCanvas(CanvasType.None);

        currentMiniGame.TransferDataFromWorkshopWhenMiniGameStarts(currentWorkshop);
        currentMiniGame.StartMiniGame();
    }

    public void ExitMiniGame(bool victory)
    {
        Debug.Log($"Exited with {victory}");
        miniGameEnvironmentCamera.gameObject.SetActive(false);
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

        if (victory && currentWorkshop.TryGetWorkshopRequireItem().HasValue)
        {
            Debug.Log("You consumed your item!");
            currentWorkshop.GetCurrentPlayer().SetInventoryObject(InventoryObject.None);
        }

        CanvasManager.instance.DisplayCanvas(CanvasType.ControlCanvas, CanvasType.TimerCanvas);

        var workshopToDeactivate =
            currentWorkshop; // Must set currentWorkshop to null for series workshop before deactivation
        currentWorkshop = null;
        workshopToDeactivate.Deactivate(victory);
    }

    #endregion

    #region Utilities

    public Workshop GetCurrentWorkshop()
    {
        return currentWorkshop;
    }

    #endregion

    #region Connected Workshops

    private void WaitForOtherPlayerInConnectedWorkshop(ConnectedWorkshop connectedWorkshop)
    {
        if (connectedWorkshop.IsOtherReady())
        {
            Debug.Log($"Connected workshop {connectedWorkshop.name} is ready!");
            // Start mini-game on this client and on other client
            StartConnectedMiniGame(connectedWorkshop.associatedMiniGame);
            InitializeConnectedMiniGameServerRpc(connectedWorkshop.GetOtherPlayerId());
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

    #endregion

    #region Network

    [ServerRpc(RequireOwnership = false)]
    private void InitializeConnectedMiniGameServerRpc(ulong otherPlayerId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {otherPlayerId}
            }
        };
        StartConnectedMiniGameClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void StartConnectedMiniGameClientRpc(ClientRpcParams parameters)
    {
        var connectedWorkshop = currentWorkshop as ConnectedWorkshop;
        if (connectedWorkshop) StartConnectedMiniGame(connectedWorkshop.associatedMiniGame);
        else Debug.LogError("Safe cast did not work!");
    }

    #endregion
}