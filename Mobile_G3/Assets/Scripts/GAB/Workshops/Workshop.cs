using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Workshop : NetworkBehaviour, IGridEntity
{
    [SerializeField] private WorkshopType type;
    protected Tile currentTile;
    public int positionX, positionY;
    [SerializeField] protected Transform workshopObject;
    public MiniGame associatedMiniGame;

    public NetworkVariable<bool> isOccupied = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isActive = new(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private bool isActiveByDefault;

    [SerializeField] [Tooltip("WARNING! Might cause unexpected effects!")]
    private bool enableMultiUsing;

    [SerializeField] protected bool occupationRequireItem;
    [SerializeField] protected InventoryObject requiredItem;

    private PlayerManager playingPlayer;

    [Header("Feedbacks")] [SerializeField] protected UnityEvent activationEvent;
    [SerializeField] protected UnityEvent deactivationEvent;
    [SerializeField] private Vector3 workshopObjectOffset;

    public virtual void Start()
    {
        isOccupied.OnValueChanged += OnSetOccupied;
        isActive.OnValueChanged += OnSetActivated;
        InitializeWorkshop();
        SetPosition(positionX, positionY);
    }

    protected virtual void InitializeWorkshop()
    {
        // For host only
        if (!NetworkManager.Singleton.IsHost) return;
        if (isActiveByDefault) Activate();
    }

    public virtual void OnCollision(IGridEntity entity, int direction)
    {
        if (!isActive.Value)
        {
            Debug.LogWarning("This workshop is not active!");
            return;
        }

        if (isOccupied.Value)
        {
            Debug.LogWarning("This workshop is already used by someone!");
            return;
        }

        playingPlayer = (PlayerManager) entity;

        WorkshopManager.instance.StartWorkshopInteraction(this);
    }

    public GridManager DEBUG_GridManager;

    [ContextMenu("Set to right Position")]
    public void DEBUG_SetToRightPosition()
    {
        SetPosition(positionX, positionY);
    }

    public virtual void SetPosition(int posX, int posY)
    {
        if (positionX == -1 && positionY == -1) // SetPosition was called on a workshop removed from grid. Must set positions here
        {
            if (posX != -1 && posY != -1) workshopObject.position = DEBUG_GridManager.GetTile(posX, posY).transform.position + workshopObjectOffset;
        }
        
        positionX = posX;
        positionY = posY;

        if (posX == -1 && posY == -1)
        {
            Debug.Log("You removed this workshop from grid!");
            workshopObject.position = Vector3.up * 100; // Pas propre mais pour l'instant c'est ok
            currentTile = null;
            return;
        }

        currentTile = DEBUG_GridManager.GetTile(posX, posY);
        if (currentTile.transform == null)
        {
            Debug.LogWarning(
                $"The workshop {name} is attached to a tile without any Transform. That might cause an error.");
            return;
        }

        MoveToNewTile(currentTile.transform.position + workshopObjectOffset);
    }

    protected virtual void MoveToNewTile(Vector3 newPosition)
    {
        // Set workshop position, might be override to allow animations if they are needed
        workshopObject.position = newPosition;
        currentTile.SetTile(this, currentTile.GetFloor());
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateServerRpc()
    {
        Activate();
    }

    protected virtual void Activate()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("This should not be called on client!");
        }

        SetActiveServerRpc(true);
        associatedMiniGame.AssociatedWorkshopGetActivatedHostSide();
        GetActivatedClientRpc();
    }

    [ClientRpc]
    private void GetActivatedClientRpc()
    {
        activationEvent?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateServerRpc(bool victory, ulong playerId = 5)
    {
        Deactivate(victory, playerId);
    }

    protected virtual void Deactivate(bool victory, ulong playerId = 5)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("This should not be called on client!");
        }

        associatedMiniGame.AssociatedWorkshopGetDeactivatedHostSide();
        SetOccupiedServerRpc(false);
        if (victory)
        {
            SetActiveServerRpc(false);
            GetDeactivatedClientRpc();
            if (type == WorkshopType.Temporary)
            {
                RemoveWorkshopFromGridClientRpc();
            }
        }
    }

    [ClientRpc]
    private void GetDeactivatedClientRpc()
    {
        deactivationEvent?.Invoke();
    }

    #region Network

    [ServerRpc(RequireOwnership = false)]
    public void SetOccupiedServerRpc(bool occupied)
    {
        isOccupied.Value = occupied;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void SetActiveServerRpc(bool active)
    {
        isActive.Value = active;
    }

    [ClientRpc]
    private void RemoveWorkshopFromGridClientRpc()
    {
        RemoveWorkshopFromGrid();
    }

    protected virtual void RemoveWorkshopFromGrid()
    {
        currentTile.SetTile(null, currentTile.GetFloor());
        SetPosition(-1, -1);
    }

    private void OnSetOccupied(bool previous, bool current)
    {
        //Debug.Log($"{name}'s occupation state has been set to {current} (was {previous})");
    }

    private void OnSetActivated(bool previous, bool current)
    {
        //Debug.Log($"{name}'s activation state has been set to {current} (was {previous})");
    }

    #endregion

    public bool IsMultiUsingEnabled()
    {
        return enableMultiUsing;
    }

    public virtual InventoryObject? TryGetWorkshopRequireItem()
    {
        if (!occupationRequireItem) return null;
        return requiredItem;
    }

    public PlayerManager GetCurrentPlayer()
    {
        return playingPlayer;
    }
    
    protected bool IsActiveOnGrid()
    {
        if (!isActive.Value || isOccupied.Value || currentTile == null) return false;
        return true;
    }
}