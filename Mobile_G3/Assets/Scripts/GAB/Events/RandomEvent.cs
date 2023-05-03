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

<<<<<<< Updated upstream
    public virtual void EndEvent()
=======
    protected virtual void EndEvent()
>>>>>>> Stashed changes
    {
        isRunning = false;
        EventsManager.instance.EndEvent(this);
    }
}
