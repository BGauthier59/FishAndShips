using UnityEngine;

public class ConnectedWorkshop : Workshop
{
    [SerializeField] private ConnectedWorkshop other;
    
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
}
