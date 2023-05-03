using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WorkshopManager : NetworkMonoSingleton<WorkshopManager>
{
    private Workshop currentWorkshop;
    public MiniGame currentMiniGame;

    public Transform miniGameEnvironmentCamera;

    [SerializeField] private Animation miniGameIndicatorAnim, victoryIndicatorAnimation;
    [SerializeField] private AnimationClip indicatorClip, victoryClip;
    [SerializeField] private TMP_Text miniGameIndicatorText;

    [SerializeField] private List<IUpdateWorkshop> updatedWorkshop = new List<IUpdateWorkshop>();

    [Header("TEMPORARY")] public RudderCircularSwipeManager rudderCircularSwipeManager;
    public ShrimpSwipeManager shrimpSwipeManager;
    public GyroscopeManager gyroscopeManager;
    public SwipeManager swipeManager;
    public CannonDragAndDropManager cannonDragAndDropManager;
    public MapSwipeManager mapSwipeManager;

    #region Game Loop

    public void AddUpdatedWorkshop(IUpdateWorkshop workshop)
    {
        // Host-side only
        if (!NetworkManager.Singleton.IsHost) return;
        updatedWorkshop.Add(workshop);
        workshop.StartGameLoopHostOnly();
    }

    public void UpdateGameLoop()
    {
        if (currentMiniGame && currentMiniGame.isRunning)
        {
            currentMiniGame.ExecuteMiniGame();
        }

        // Updated Workshops are managed by Host only
        if (NetworkManager.Singleton.IsHost)
        {
            foreach (var workshop in updatedWorkshop)
            {
                workshop.UpdateGameLoopHostOnly();
            }
        }
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
        workshopToDeactivate.DeactivateServerRpc(victory, NetworkManager.Singleton.LocalClientId);
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

    public void SetGameIndicator(string text)
    {
        miniGameIndicatorText.text = text;
        victoryIndicatorAnimation.gameObject.SetActive(false);
        miniGameIndicatorAnim.gameObject.SetActive(true);
        miniGameIndicatorAnim.Play(indicatorClip.name);
    }

    public void SetVictoryIndicator()
    {
        miniGameIndicatorAnim.gameObject.SetActive(false);
        victoryIndicatorAnimation.gameObject.SetActive(true);
        victoryIndicatorAnimation.Play(victoryClip.name);
    }

    public int GetIndicatorAnimationLength()
    {
        return ((int) (indicatorClip.length * 1000) - 200); // For async await only
    }

    public int GetVictoryAnimationLength()
    {
        return ((int) (victoryClip.length * 1000) - 100); // For async await only
    }
}