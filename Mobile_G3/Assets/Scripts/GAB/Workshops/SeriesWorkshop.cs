using Unity.Netcode;
using UnityEngine;

public class SeriesWorkshop : Workshop
{
    public MiniGame[] nextMiniGames;

    [Tooltip("Index -1 is associatedMiniGame. Otherwise follows list indices")]
    public NetworkVariable<int> currentMiniGameIndex = new(-1);

    [Tooltip("For next mini-games only")] [SerializeField]
    private InventoryObject[] requiredItems;
    
    [SerializeField] [Range(0, 3)] [Tooltip("0 is top left, 1 is top right, 2 is bottom left, 3 is bottom right")]
    private byte cannonIndex;

    public MiniGame GetCurrentMiniGame()
    {
        return currentMiniGameIndex.Value == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndex.Value];
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
        
        SetMiniGameIndexServerRpc(currentMiniGameIndex.Value + 1);
        if (currentMiniGameIndex.Value == nextMiniGames.Length)
        {
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
        if (currentMiniGameIndex.Value == -1) return requiredItem;
        if (requiredItems.Length == 0)
        {
            Debug.LogWarning("No item in required items list!");
            return null;
        }

        return requiredItems[currentMiniGameIndex.Value];
    }

    public byte GetCannonIndex()
    {
        return cannonIndex;
    }
}