using Unity.Netcode;
using UnityEngine;

public class MiniGameManager : NetworkBehaviour
{
    public static MiniGameManager instance;

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
        //currentWorkshop.isOccupied.Value = true;

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
        //Debug.Log($"Started game {game}");
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
        
        Debug.Log("1 ok");
        // Checks if client's current workshop is a connected workshop
        var connectedWorkshop = currentWorkshop as ConnectedWorkshop;
        if (!connectedWorkshop) return;
        
        Debug.Log("2 ok");
        // Checks if client's connected workshop has the right id
        var id = connectedWorkshop.GetWorkshopId();
        if (id != otherConnectedWorkshopId) return;
        
        Debug.Log("3 ok ! should start");

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

        currentWorkshop.Deactivate(victory);
    }
}