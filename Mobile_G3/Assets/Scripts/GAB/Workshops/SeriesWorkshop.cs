using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SeriesWorkshop : Workshop
{
    public MiniGame[] nextMiniGames;

    [Tooltip("Index -1 is associatedMiniGame. Otherwise follows list indices")]
    public NetworkVariable<int> currentMiniGameIndex = new(-1);

    private int currentMiniGameIndexSafe = -1;

    [Tooltip("For next mini-games only")] [SerializeField]
    private InventoryObject[] requiredItems;

    [SerializeField] [Range(0, 3)] [Tooltip("0 is top left, 1 is top right, 2 is bottom left, 3 is bottom right")]
    private byte cannonIndex;

    [Tooltip(
        "WARNING! Index 0 is associatedMiniGame. Otherwise follows list indices. Activation events is for every mini-games")]
    [SerializeField]
    private UnityEvent[] activationEvents;

    [Tooltip("Same as for activation events tooltip")] [SerializeField]
    private UnityEvent[] deactivationEvents;

    public override void Start()
    {
        base.Start();
        currentMiniGameIndex.OnValueChanged = OnServerModifyCurrentMiniGameIndex;
    }

    public MiniGame GetCurrentMiniGame()
    {
        // Not safe to use it because of latency
        return currentMiniGameIndex.Value == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndex.Value];
    }

    public MiniGame GetCurrentMiniGameSafe()
    {
        return currentMiniGameIndexSafe == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndexSafe];
    }

    private void OnServerModifyCurrentMiniGameIndex(int previous, int current)
    {
        currentMiniGameIndexSafe = currentMiniGameIndex.Value;
    }

    public override void Activate()
    {
        base.Activate();
        var index = currentMiniGameIndexSafe == -1 ? 0 : currentMiniGameIndexSafe + 1;
        activationEvents[index]?.Invoke();
    }

    public override void Deactivate(bool victory)
    {
        associatedMiniGame.AssociatedWorkshopGetDeactivated();

        if (!victory)
        {
            SetOccupiedServerRpc(false);
            return;
        }

        deactivationEvent?.Invoke();
        var index = currentMiniGameIndexSafe == -1 ? 0 : currentMiniGameIndexSafe + 1;
        deactivationEvents[index]?.Invoke();

        currentMiniGameIndexSafe++;
        SetMiniGameIndexServerRpc(currentMiniGameIndex.Value + 1);
        if (currentMiniGameIndexSafe == nextMiniGames.Length)
        {
            currentMiniGameIndexSafe = -1;
            SetMiniGameIndexServerRpc(-1);
            SetActiveServerRpc(false);
            SetOccupiedServerRpc(false);
            Activate();
            return;
        }

        Activate();
        WorkshopManager.instance.StartWorkshopInteraction(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMiniGameIndexServerRpc(int index)
    {
        currentMiniGameIndex.Value = index;
    }

    public override InventoryObject? TryGetWorkshopRequireItem()
    {
        if (!occupationRequireItem) return null;
        if (currentMiniGameIndexSafe == -1) return requiredItem;
        if (requiredItems.Length == 0) return null;

        return requiredItems[currentMiniGameIndexSafe];
    }

    public byte GetCannonIndex()
    {
        return cannonIndex;
    }
}