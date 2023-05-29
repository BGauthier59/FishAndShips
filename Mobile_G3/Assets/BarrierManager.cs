using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoSingleton<BarrierManager>
{
    [SerializeField] private List<GridBarrier> barriers;

    public void StartGameLoop()
    {
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
        for (int i = 0; i < barriers.Count; i++)
        {
            barriers[i].ToggleServerRpc();
        }
    }
}
