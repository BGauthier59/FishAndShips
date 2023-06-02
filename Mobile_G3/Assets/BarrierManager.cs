using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class BarrierManager : MonoSingleton<BarrierManager>
{
    [SerializeField] private List<GridBarrier> barriers;
    [SerializeField] private List<int2> plates = new List<int2>();

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
        for (int i = 0; i < plates.Count; i++)
        {
            if (GridManager.instance.GetTile(plates[i].x, plates[i].y).entity == null)
            {
                plates.RemoveAt(i);
                break;
            }
        }

        if (plates.Count == 0)
        {
            for (int i = 0; i < barriers.Count; i++)
            {
                barriers[i].Close();
            }
        }
        
        foreach (var barrier in barriers)
        {
            barrier.Refresh();
        }
    }

    public void SwitchBarriers(int2 pos)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        for (int i = 0; i < barriers.Count; i++)
        {
            barriers[i].Open();
        }
        plates.Add(pos);
    }
}
