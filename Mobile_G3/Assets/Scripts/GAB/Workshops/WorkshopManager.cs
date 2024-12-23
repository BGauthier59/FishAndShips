using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class WorkshopManager : NetworkMonoSingleton<WorkshopManager>
{
    private Workshop currentWorkshop;
    public MiniGame currentMiniGame;

    public Transform miniGameEnvironmentCamera;

    [SerializeField] private Animation miniGameIndicatorAnim, victoryIndicatorAnimation, gyroscopeIndicator;
    [SerializeField] private AnimationClip indicatorClip, victoryClip, gyroscopeFire, gyroscopeCannon;
    [SerializeField] private TMP_Text miniGameIndicatorText;

    private List<IUpdateWorkshop> updatedWorkshop = new List<IUpdateWorkshop>();
    [SerializeField] private Workshop[] allWorkshops;

    [SerializeField] private RectTransform referenceCanvas;
    [SerializeField] private float canvasScaleFactor;

    [SerializeField] private Transform swipeIndicatorTransform;

    [Header("TEMPORARY")] public RudderCircularSwipeManager rudderCircularSwipeManager;
    public ShrimpSwipeManager shrimpSwipeManager;
    public GyroscopeManager gyroscopeManager;
    public SwipeManager swipeManager;
    public CannonDragAndDropManager cannonDragAndDropManager;
    public MapSwipeManager mapSwipeManager;
    public ReparationDragAndDropManager reparationDragAndDrop;

    [SerializeField] private MiniGameSwipeIndicatorPoints[] swipeTutorialData;

    [SerializeField] private GridEntity_InventoryFiller[] bulletsFillers;
    [SerializeField] private GridEntity_InventoryFiller[] plankFillers;

    [SerializeField] private UnityEvent startWorkshopEvent;

    [Serializable]
    public struct MiniGameSwipeIndicatorPoints
    {
        public string DEBUG_name;
        public Transform p1;
        public Transform p2;
        public Transform p3;
        public Transform p4;
        public float swipeDuration;
    }

    #region Game Loop

    public void AddUpdatedWorkshop(IUpdateWorkshop workshop)
    {
        // Host-side only
        if (!NetworkManager.Singleton.IsHost) return;
        updatedWorkshop.Add(workshop);
        workshop.StartGameLoopHostOnly();
    }

    public void StartGameLoop()
    {
        canvasScaleFactor = referenceCanvas.lossyScale.x;

        foreach (var workshop in allWorkshops)
        {
            workshop.StartGameLoop();
        }
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
                workshop.PlayRequiredItemInvalid();
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

        startWorkshopEvent?.Invoke();
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

    #region Animations & Tutorial

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

    public float GetCanvasFactor()
    {
        return canvasScaleFactor;
    }

    private bool isShowingTutorial;
    private int currentIndex = -1;
    private int askedIndex;

    private async void ExecuteMiniGameTutorial()
    {
        if (isShowingTutorial) return;

        isShowingTutorial = true;

        data = swipeTutorialData[currentIndex];
        p1 = data.p1.position;
        p2 = data.p2.position;
        p3 = data.p3.position;
        p4 = data.p4.position;

        swipeIndicatorTransform.position = Ex.CubicBezierCurve(p1, p2, p3, p4, 0);
        swipeIndicatorTransform.gameObject.SetActive(true);

        await UniTask.Delay(200);

        float timer = 0;
        while (timer < data.swipeDuration)
        {
            swipeIndicatorTransform.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / data.swipeDuration);
            await UniTask.Yield();
            timer += Time.deltaTime;
        }

        swipeIndicatorTransform.position = Ex.CubicBezierCurve(p1, p2, p3, p4, 1);
        await UniTask.Delay(200);
        swipeIndicatorTransform.gameObject.SetActive(false);
        await UniTask.Delay(1000);

        if (askForStopTutorial)
        {
            isShowingTutorial = false;
            askForStopTutorial = false;
            askForStartTutorial = false;
            return;
        }

        if (askForStartTutorial)
        {
            currentIndex = askedIndex;
            askForStartTutorial = false;
        }

        isShowingTutorial = false;

        ExecuteMiniGameTutorial();
    }

    public void StopMiniGameTutorial()
    {
        if (!isShowingTutorial) return;
        askForStopTutorial = true;
    }

    Vector3 p1, p2, p3, p4;
    private MiniGameSwipeIndicatorPoints data;
    private bool askForStartTutorial;
    private bool askForStopTutorial;

    public void StartMiniGameTutorial(int index)
    {
        // 0 is Cannon / Load / Step 1
        // 1 is Cannon / Load / Step 2
        // 2 is Cannon / Load / Step 3
        // 3 is Cannon / Shoot
        // 4 is Reparation
        // 5 is map
        // 6 is rudder
        // 7 is Sails (left)
        // 8 is Sails (right)

        // 9 is shrimp from left
        // 10 is shrimp from right
        // 11 is shrimp from bottom left
        // 12 is shrimp from bottom right
        // 13 is shrimp from top left
        // 14 is shrimp from top right

        if (isShowingTutorial)
        {
            askForStartTutorial = true;
            askedIndex = index;
        }
        else
        {
            currentIndex = index;
            ExecuteMiniGameTutorial();
        }
    }

    public async void StartMiniGameGyroscopeFireTutorial()
    {
        gyroscopeIndicator.gameObject.SetActive(true);
        await UniTask.Delay(200);
        gyroscopeIndicator.Play(gyroscopeFire.name);
        await UniTask.Delay((int) (gyroscopeFire.length * 1000));
        await UniTask.Delay(200);
        gyroscopeIndicator.gameObject.SetActive(false);
    }
    
    public async void StartMiniGameGyroscopeCannonTutorial()
    {
        gyroscopeIndicator.gameObject.SetActive(true);
        await UniTask.Delay(200);
        gyroscopeIndicator.Play(gyroscopeCannon.name);
        await UniTask.Delay((int) (gyroscopeCannon.length * 1000));
        await UniTask.Delay(200);
        gyroscopeIndicator.gameObject.SetActive(false);
    }

    #endregion

    #region Fillers

    private int bulletFillersCount;

    public void StartBulletFillersGlow()
    {
        bulletFillersCount++;
        if (bulletFillersCount > 1) return;
        foreach (var filler in bulletsFillers)
        {
            filler.SetGlow(true);
        }
    }

    public void EndBulletFillersGlow()
    {
        if (bulletFillersCount == 0) return;
        bulletFillersCount--;
        if (bulletFillersCount != 0) return;
        foreach (var filler in bulletsFillers)
        {
            filler.SetGlow(false);
        }
    }

    private int plankFillersCount;

    public void StartPlankFillersGlow()
    {
        plankFillersCount++;
        if (plankFillersCount > 1) return;
        foreach (var filler in plankFillers)
        {
            filler.SetGlow(true);
        }
    }

    public void EndPlankFillersGlow()
    {
        if (plankFillersCount == 0) return;
        plankFillersCount--;
        if (plankFillersCount != 0) return;
        foreach (var filler in plankFillers)
        {
            filler.SetGlow(false);
        }
    }

    #endregion
}