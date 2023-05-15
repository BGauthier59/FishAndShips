using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DEBUG_Disconnection : NetworkBehaviour
{
    public void Disconnect()
    {
        DisconnectServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisconnectServerRpc(ulong id)
    {
        NetworkManager.Singleton.DisconnectClient(id);
    }
}
