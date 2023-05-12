using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public abstract class RandomEvent : NetworkBehaviour
{
    public bool isRunning;
    public string startEventText;
    

    public abstract bool CheckConditions();

    public virtual void StartEvent()
    {
        isRunning = true;
    }
    
    protected virtual void EndEvent()
    {
        isRunning = false;
        EventsManager.instance.EndEvent(this);
    }
}
