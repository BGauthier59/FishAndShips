using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonEvent : RandomEvent
{
    public override void StartEvent()
    {
        // Host-side logic
        base.StartEvent();
        ActivateCannon();
        EndEvent();
    }
    
    public override bool CheckConditions()
    {
        if (EventsManager.instance.GetCannonIndices().Length == 0) return false;
        return true;
    }

    private void ActivateCannon()
    {
        var indices = EventsManager.instance.GetCannonIndices();
        var index = indices[Random.Range(0, indices.Length)];
        var workshop = EventsManager.instance.GetCannonWorkshop(index);
        workshop.ActivateServerRpc();
    }
}
