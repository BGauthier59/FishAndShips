using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierManager : MonoSingleton<BarrierManager>
{
    [SerializeField] private List<GridBarrier> barriers;

    public void SwitchBarriers()
    {
        for (int i = 0; i < barriers.Count; i++)
        {
            barriers[i].isClosed = !barriers[i].isClosed;
        }
    }
}
