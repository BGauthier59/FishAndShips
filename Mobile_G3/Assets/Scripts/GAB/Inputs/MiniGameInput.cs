using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameInput<T> : MonoBehaviour where T : struct
{
    protected bool isActive;
    
    public virtual void Enable(T setupData)
    {
        isActive = true;
    }

    public virtual void Disable()
    {
        isActive = false;
    }
}
