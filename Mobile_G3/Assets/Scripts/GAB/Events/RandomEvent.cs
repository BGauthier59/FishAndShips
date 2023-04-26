using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class RandomEvent : NetworkBehaviour
{
    // La gestion des events est gérée par le Host
    
    public bool isActive;
    
    public abstract bool CheckConditions();
    
    public abstract void StartEvent();

    public abstract void ExecuteEvent();

    public abstract void EndEvent();
}
