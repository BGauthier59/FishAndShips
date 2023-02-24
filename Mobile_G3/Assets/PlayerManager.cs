using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        MenuManager.instance.icons[OwnerClientId].obj.SetActive(true);
        transform.position = new Vector3(OwnerClientId, 0, 0);
    }

    public override void OnNetworkDespawn()
    {
        MenuManager.instance.icons[OwnerClientId].obj.SetActive(false);
    }
}
