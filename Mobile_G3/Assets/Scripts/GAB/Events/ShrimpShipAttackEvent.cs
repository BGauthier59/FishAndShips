using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrimpShipAttackEvent : RandomEvent
{
    public override bool CheckConditions()
    {
        return false;
    }

    public override void StartEvent()
    {
        // Feedback, camera movement, début de l'event
    }

    public override void ExecuteEvent()
    {
        // Instantie workshop : crevettes
        
    }

    public override void EndEvent()
    {
        // Destroyed by 3 cannon bullets
    }

    private void GetHit()
    {
        
    }

    private void CheckShrimpSpawnTimer()
    {
        
    }

    private void SpawnShrimpWorkshop()
    {
        
    }

    private bool IsTileAvailableForShrimpWorkshop()
    {
        return false;
    }
}
