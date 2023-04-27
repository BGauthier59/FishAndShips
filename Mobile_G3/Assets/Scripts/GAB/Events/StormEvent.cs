using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormEvent : RandomEvent
{
    public override void StartEvent()
    {
        base.StartEvent();
    }
    
    public override bool CheckConditions()
    {

        return true;
    }

    public override void ExecuteEvent()
    {
        
    }
    
    protected override void EndEvent()
    {
        base.EndEvent();
    }
}
