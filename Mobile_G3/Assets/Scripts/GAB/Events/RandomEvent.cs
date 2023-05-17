using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public abstract class RandomEvent : NetworkBehaviour
{
    public string startEventText;

    public abstract bool CheckConditions();

    public abstract void StartEvent();

    protected abstract void EndEvent();
}