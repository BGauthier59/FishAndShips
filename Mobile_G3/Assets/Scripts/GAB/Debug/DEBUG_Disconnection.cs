using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DEBUG_Disconnection : NetworkBehaviour
{
    public void Disconnect()
    {
       NetworkManager.Singleton.Shutdown();
    }

}
