using Unity.Netcode;
using UnityEngine;

public class ConnectedWorkshop : Workshop
{
    [SerializeField] private ConnectedWorkshop other;
    [SerializeField] [TextArea(4, 4)] private string waitingMessage;
    public NetworkVariable<ulong> currentPlayerId = new(5, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public void InitializeActivation()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        Activate();
        other.Activate();
    }

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
