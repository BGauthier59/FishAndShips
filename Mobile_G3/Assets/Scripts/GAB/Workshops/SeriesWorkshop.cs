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

    [Tooltip(
        "WARNING! Index 0 is associatedMiniGame. Otherwise follows list indices. Activation events is for every mini-games")]
    [SerializeField]
    private UnityEvent[] activationEvents;

    [Tooltip("Same as for activation events tooltip")] [SerializeField]
    private UnityEvent[] deactivationEvents;

    public override void Start()
    {
        base.Start();
        isActive.OnValueChanged = OnSetActive;
        currentMiniGameIndex.OnValueChanged = OnServerModifyCurrentMiniGameIndex;
    }

    private void OnSetActive(bool _, bool current)
    {
        Debug.Log("Set active for cannon");
        if(current) WorkshopManager.instance.StartBulletFillersGlow();
        else WorkshopManager.instance.EndBulletFillersGlow();
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
        SeriesWorkshopGetActivatedClientRpc(index);
    }

    [ClientRpc]
    private void SeriesWorkshopGetActivatedClientRpc(int index)
    {
        activationEvents[index]?.Invoke();
    }

    protected override void Deactivate(bool? victory, ulong? playerId)
    {
        associatedMiniGame.AssociatedWorkshopGetDeactivatedHostSide();

        if (victory.HasValue && !victory.Value)
        {
            SetOccupiedServerRpc(false);
            return;
        }

        var index = currentMiniGameIndexSafe == -1 ? 0 : currentMiniGameIndexSafe + 1;
        SeriesWorkshopGetDeactivatedClientRpc(index);

        currentMiniGameIndex.Value++;
        if (currentMiniGameIndexSafe == nextMiniGames.Length ||
            !victory.HasValue) // If victory is null, it means the workshop has been lost because of timer
        {
            currentMiniGameIndex.Value = -1;
            SetActiveServerRpc(false);
            GetDeactivatedClientRpc(victory.HasValue);
            SetOccupiedServerRpc(false);
            EventsManager.instance.RemoveWorkshop();
            return;
        }

        ClientRpcParams parameters = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new ulong[] {playerId.Value}
            }
        };
        InitiateWorkshopInteractionClientRpc(currentMiniGameIndex.Value, parameters);
    }

    [ClientRpc]
    private void SeriesWorkshopGetDeactivatedClientRpc(int index)
    {
        //deactivationEvent?.Invoke();
        deactivationEvents[index]?.Invoke();
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
}