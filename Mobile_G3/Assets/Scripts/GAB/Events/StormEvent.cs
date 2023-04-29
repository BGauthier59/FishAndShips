using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormEvent : RandomEvent
{
    public override void StartEvent()
    {
        base.StartEvent();
        Debug.Log("You entered a stormy area!");
    }
    
    public override bool CheckConditions()
    {
        // This is not fully random, then no need to check any condition
        return true;
    }

    public override void ExecuteEvent()
    {
        
    }
    
    public override void EndEvent()
    {
        base.EndEvent();
        Debug.Log("You exited a stormy area!");
    }
}
