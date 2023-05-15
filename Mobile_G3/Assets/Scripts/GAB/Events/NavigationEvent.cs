using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationEvent : RandomEvent
{
    
    public override void StartEvent()
    {
        // Host-side logic
        base.StartEvent();
        ActivateMapAndRudder();
        EndEvent();
    }
    
    public override bool CheckConditions()
    {
        if (EventsManager.instance.mapWorkshop.isActive.Value) return false;
        return true;
    }

    private void ActivateMapAndRudder()
    {
        EventsManager.instance.mapWorkshop.InitializeActivation();
    }
}
