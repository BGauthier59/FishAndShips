using Unity.Netcode;

public abstract class RandomEvent : NetworkBehaviour
{
    public bool isRunning;

    public abstract bool CheckConditions();

    public virtual void StartEvent()
    {
        isRunning = true;
    }

    public abstract void ExecuteEvent();

    protected virtual void EndEvent()
    {
        isRunning = false;
        EventsManager.instance.EndEvent(this);
    }
}
