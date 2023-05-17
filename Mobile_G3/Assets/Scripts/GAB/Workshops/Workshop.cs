using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

    [SerializeField] private float activationDuration;

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
    [SerializeField] protected UnityEvent winEvent;
    [SerializeField] protected UnityEvent loseEvent;
    [SerializeField] private Animation alertAnim;
    [SerializeField] private AnimationClip alertClip;
    [SerializeField] private Vector3 workshopObjectOffset;
    [SerializeField] private Animation requiredItemAnim;
    [SerializeField] private AnimationClip requiredItemClip;
    [SerializeField] protected Transform feedbackTransform;

    public virtual void Start()
    {
        isOccupied.OnValueChanged += OnSetOccupied;
        isActive.OnValueChanged += OnSetActivated;
        SetPosition(positionX, positionY);
    }

    public void StartGameLoop()
    {
        InitializeWorkshop();
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
        if (positionX == -1 &&
            positionY == -1) // SetPosition was called on a workshop removed from grid. Must set positions here
        {
            if (posX != -1 && posY != -1)
                workshopObject.position = feedbackTransform.position =
                    DEBUG_GridManager.GetTile(posX, posY).transform.position + workshopObjectOffset;
        }

        positionX = posX;
        positionY = posY;

        if (posX == -1 && posY == -1)
        {
            workshopObject.position = Vector3.up * 100; // Pas propre mais pour l'instant c'est ok
            currentTile = null;
            return;
        }

        currentTile = DEBUG_GridManager.GetTile(posX, posY);
        if (currentTile.transform == null)
        {
            // Set feedback position for workshops that are not on a tile
            feedbackTransform.position = workshopObject.position;
            return;
        }

        MoveToNewTile(currentTile.transform.position + workshopObjectOffset);
    }

    protected virtual void MoveToNewTile(Vector3 newPosition)
    {
        // Set workshop position, might be override to allow animations if they are needed
        workshopObject.position = newPosition;
        feedbackTransform.position = newPosition;
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

        if (isActive.Value)
        {
            Debug.LogError($"Can't activate a workshop that is already activated. Happened for {name}");
            // This happens with series workshop.
            return;
        }

        SetActiveServerRpc(true);
        associatedMiniGame.AssociatedWorkshopGetActivatedHostSide();
        GetActivatedClientRpc();
        StartActivationDuration();
    }

    protected virtual async void StartActivationDuration()
    {
        float duration = (int) (1000 * activationDuration);

        await UniTask.Delay((int) (duration * .75f));
        alertAnim.Play(alertClip.name);
        await UniTask.Delay((int) (duration * .25f));

        while (isOccupied.Value) await Task.Yield(); // Can't be lost if someone is playing

        await UniTask.Delay(100);
        //await Task.Delay(100);
        if (!isActive.Value)
        {
            Debug.LogWarning("You won workshop after timer is over. It's still supposed to be a victory.");
            return; // Means workshop has been won
        }

        Debug.Log($"You lost {name}");
        ShipManager.instance.TakeDamage(5);
        Deactivate(null, null); // This is not a victory, only means to disable mini-game
    }

    [ClientRpc]
    private void GetActivatedClientRpc()
    {
        activationEvent?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateServerRpc(bool victory, ulong playerId)
    {
        Deactivate(victory, playerId);
    }

    protected virtual void Deactivate(bool? victory, ulong? playerId)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("This should not be called on client!");
        }

        associatedMiniGame.AssociatedWorkshopGetDeactivatedHostSide();
        if (!victory.HasValue || victory.Value) // If victory is null, it means the workshop has been lost because of timer
        {
            SetActiveServerRpc(false);
            GetDeactivatedClientRpc(victory.HasValue);
            if (type == WorkshopType.Temporary)
            {
                RemoveWorkshopFromGridClientRpc();
            }
        }

        SetOccupiedServerRpc(false);
    }

    [ClientRpc]
    private void GetDeactivatedClientRpc(bool victory)
    {
        if (victory) winEvent?.Invoke();
        else loseEvent?.Invoke();
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

    public void PlayRequiredItemInvalid()
    {
        if (requiredItemAnim.isPlaying) return;
        requiredItemAnim.Play(requiredItemClip.name);
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