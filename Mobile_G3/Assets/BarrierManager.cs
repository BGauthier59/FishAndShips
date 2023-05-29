using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BarrierManager : MonoSingleton<BarrierManager>
{
    [SerializeField] private List<GridBarrier> barriers;

    public void StartGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        foreach (var barrier in barriers)
        {
            barrier.Setup();
        }
    }

    public void UpdateGameLoop()
    {
        foreach (var barrier in barriers)
        {
            barrier.Refresh();
        }
    }

    public void SwitchBarriers()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        for (int i = 0; i < barriers.Count; i++)
        {
            barriers[i].Toggle();
        }
    }
}
