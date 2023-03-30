using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameInput<T> : MonoBehaviour where T : struct
{
    protected T data;
    protected bool isActive;
    
    public virtual void Enable(T setupData)
    {
        data = setupData;
        isActive = true;
    }

    public virtual void Disable()
    {
        isActive = false;
    }
}
