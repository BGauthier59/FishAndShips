using UnityEngine;

public class TutorialCannonSoftEvent : RandomEvent
{
    public override bool CheckConditions()
    {
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
        for (int i = 0; i < TutorialEventManager.instance.playerNb; i++)
        {
            var workshop = TutorialEventManager.instance.GetCannonWorkshop(i);
            workshop.ActivateServerRpc();
        }
    }
}
