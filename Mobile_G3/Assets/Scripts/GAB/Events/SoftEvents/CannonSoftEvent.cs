using UnityEngine;

public class CannonSoftEvent : RandomEvent
{
    public override bool CheckConditions()
    {
        if (EventsManager.instance.GetCannonIndices().Length == 0) return false;
        return true;
    }
    
    public override void StartEvent()
    {
        // Host-side logic
        ActivateCannon();
        EndEvent();
    }

    protected override void EndEvent()
    {
        
    }

    private void ActivateCannon()
    {
        var indices = EventsManager.instance.GetCannonIndices();
        
        var index = indices[Random.Range(0, indices.Length)];
        var workshop = EventsManager.instance.GetCannonWorkshop(index);
        workshop.ActivateServerRpc();
    }
}
