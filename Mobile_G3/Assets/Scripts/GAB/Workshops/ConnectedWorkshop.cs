using Unity.VisualScripting;
using UnityEngine;

public class ConnectedWorkshop : Workshop
{
    [SerializeField] private ConnectedWorkshop other;
    [SerializeField] [TextArea(4, 4)] private string waitingMessage;
    
    public void InitializeActivation()
    {
        Activate();
        other.Activate();
    }

    public void InitializeDeactivation(bool victory)
    {
        Deactivate(victory);
        other.Deactivate(victory);
    }

    public bool IsOtherReady()
    {
        return other.isOccupied.Value;
    }

    public ConnectedWorkshop GetOtherWorkshop()
    {
        return other;
    }

    public string GetWaitingMessage()
    {
        return waitingMessage;
    }
}
