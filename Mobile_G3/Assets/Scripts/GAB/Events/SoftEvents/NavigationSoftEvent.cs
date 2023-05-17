using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSoftEvent : RandomEvent
{
    public override bool CheckConditions()
    {
        if (EventsManager.instance.mapWorkshop.isActive.Value) return false;
        return true;
    }
    
    public override void StartEvent()
    {
        // Host-side logic
        ActivateMapAndRudder();
        EndEvent();
    }

    protected override void EndEvent()
    {
        
    }
    
    private void ActivateMapAndRudder()
    {
        EventsManager.instance.mapWorkshop.InitializeActivation();
    }
}
