using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectedWorkshop : Workshop
{
    [SerializeField] private ConnectedWorkshop other;
    [SerializeField] [TextArea(4, 4)] private string waitingMessage;
    public NetworkVariable<ulong> currentPlayerId = new(5, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    public void InitializeActivation()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No client should call this method!");
            return;
        }
        Activate();
        other.Activate();
    }

    /*
    public void InitializeDeactivation(bool victory)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No client should call this method!");
            return;
        }
        Deactivate(victory);
        other.Deactivate(victory);
    }
    */
    
    [ServerRpc(RequireOwnership = false)]
    public void SetCurrentPlayerIdServerRpc(ulong playerId)
    {
        currentPlayerId.Value = playerId;
    }

    public bool IsOtherReady()
    {
        return other.isOccupied.Value;
    }

    public ulong GetOtherPlayerId()
    {
        if (other.currentPlayerId.Value > 4)
        {
            Debug.LogWarning("Not valid player id!");
            return 0;
        }
        return other.currentPlayerId.Value;
    }

    public ConnectedWorkshop GetOtherWorkshop()
    {
        return other;
    }

    public string GetWaitingMessage()
    {
        return waitingMessage;
    }
}
