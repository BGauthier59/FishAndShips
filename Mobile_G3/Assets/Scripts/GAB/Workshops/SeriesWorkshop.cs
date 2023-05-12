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

    public MiniGame GetCurrentMiniGameSafe()
    {
        return currentMiniGameIndexSafe == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndexSafe];
    }

    private void OnServerModifyCurrentMiniGameIndex(int previous, int current)
    {
        currentMiniGameIndexSafe = currentMiniGameIndex.Value;
    }

    protected override void Activate()
    {
        base.Activate();
        var index = currentMiniGameIndexSafe == -1 ? 0 : currentMiniGameIndexSafe + 1;
        activationEvents[index]?.Invoke();
    }

    protected override void Deactivate(bool victory, ulong playerId = 5)
    {
        associatedMiniGame.AssociatedWorkshopGetDeactivatedHostSide();

        if (!victory)
        {
            SetOccupiedServerRpc(false);
            return;
        }

        deactivationEvent?.Invoke();
        var index = currentMiniGameIndexSafe == -1 ? 0 : currentMiniGameIndexSafe + 1;
        deactivationEvents[index]?.Invoke();

        currentMiniGameIndex.Value++;
        if (currentMiniGameIndexSafe == nextMiniGames.Length)
        {
            currentMiniGameIndex.Value = -1;
            SetActiveServerRpc(false);
            SetOccupiedServerRpc(false);
            Activate();
            return;
        }

        Activate();
        ClientRpcParams parameters = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new ulong[] {playerId}
            }
        };
        InitiateWorkshopInteractionClientRpc(currentMiniGameIndex.Value, parameters);
    }

    [ClientRpc]
    private void InitiateWorkshopInteractionClientRpc(int newMiniGameIndex, ClientRpcParams parameters)
    {
        currentMiniGameIndexSafe = newMiniGameIndex;
        WorkshopManager.instance.StartWorkshopInteraction(this);
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