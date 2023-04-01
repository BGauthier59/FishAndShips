using Unity.Netcode;
using UnityEngine;

public class NetworkMonoSingleton<T> : NetworkBehaviour where T : Component
{
    public static T instance;

    public virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(instance.gameObject);
            return;
        }

        instance = this as T;
    }
}
