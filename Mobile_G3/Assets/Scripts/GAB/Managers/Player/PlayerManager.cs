using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{

    #region Network

    public override void OnNetworkSpawn()
    {
        // TODO - Check if can join the room
        
        if(IsOwner) Debug.LogWarning("You've been connected!");
        ConnectionManager.instance.AddPlayerToDictionary(OwnerClientId, this);
    }
    
    public override void OnNetworkDespawn()
    {
        if(IsOwner) Debug.LogWarning("You've been disconnected");
        ConnectionManager.instance.RemovePlayerFromDictionary(OwnerClientId);
    }

    #endregion
}
