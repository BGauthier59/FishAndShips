using Unity.Netcode;
using UnityEngine;

public class WorkshopManager : NetworkBehaviour
{
    public static WorkshopManager instance;

    private Workshop currentWorkshop;
    private MiniGame currentMiniGame;
    [SerializeField] private GameObject popUpMiniGame;

    [Header("TEMPORARY")] public CircularSwipeManager circularSwipeManager;
    public GyroscopeManager gyroscopeManager;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
    }

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
            WaitForOtherPlayerInConnectedWorkshop(connectedWorkshop);
        }
        else StartMiniGame(currentWorkshop.associatedMiniGame);
    }

    private void StartMiniGame(MiniGame game)
    {
        currentMiniGame = game;
        CanvasManager.instance.DisplayCanvas(CanvasType.None);
        popUpMiniGame.SetActive(true);
        currentMiniGame.StartMiniGame();
    }


    private void WaitForOtherPlayerInConnectedWorkshop(ConnectedWorkshop connectedWorkshop)
    {
        if (connectedWorkshop.IsOtherReady())
        {
            Debug.LogError($"Connected workshop {connectedWorkshop.name} is ready!");
            
            // Start mini-game on this client and on other client
            StartMiniGame(connectedWorkshop.associatedMiniGame);
            
            Debug.LogError("Send RPC...");
            InitializeConnectedMiniGameServerRpc(connectedWorkshop.GetOtherWorkshop().GetWorkshopId());
        }
        else
        {
            Debug.Log($"Waiting for other connected workshop from {connectedWorkshop.name}...");
            CanvasManager.instance.DisplayCanvas(CanvasType.WorkshopCanvas);
            // Feedbacks d'attente
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeConnectedMiniGameServerRpc(uint otherConnectedWorkshopId)
    {
        StartConnectedMiniGameClientRpc(otherConnectedWorkshopId);
    }

    [ClientRpc]
    private void StartConnectedMiniGameClientRpc(uint otherConnectedWorkshopId)
    {
        Debug.Log("Rpc sent to this client!");
        // Checks if client has a current workshop linked
        if (!currentWorkshop) return;
        
        // Checks if client's current workshop is a connected workshop
        var connectedWorkshop = currentWorkshop as ConnectedWorkshop;
        if (!connectedWorkshop) return;
        
        // Checks if client's connected workshop has the right id
        var id = connectedWorkshop.GetWorkshopId();
        if (id != otherConnectedWorkshopId) return;
        
        // Must be the right client, then starts their mini-game
        StartMiniGame(connectedWorkshop.associatedMiniGame);
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
        EndWorkshopInteraction(victory);
    }

    private void EndWorkshopInteraction(bool victory)
    {
        if (currentWorkshop == null)
        {
            Debug.Log("There is not workshop associated with this mini-game!");
            return;
        }

        CanvasManager.instance.DisplayCanvas(CanvasType.ControlCanvas);
        currentWorkshop.Deactivate(victory);
    }
}