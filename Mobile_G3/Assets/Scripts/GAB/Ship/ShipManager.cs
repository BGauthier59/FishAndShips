using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ShipManager : NetworkMonoSingleton<ShipManager>
{
    private NetworkVariable<int> currentBoatLife = new NetworkVariable<int>();
    [SerializeField] private int maxLife;
    
    [SerializeField]
    private Transform leftTopCannonOrigin, rightTopCannonOrigin, leftBottomCannonOrigin, rightBottomCannonOrigin;

    [SerializeField] private LayerMask shrimpShipLayer;
    private NetworkVariable<bool> underAttack = new NetworkVariable<bool>();

    private bool canRegenerate;
    [SerializeField] private int regenerationPerSecond;
    private float regenerationTimer;
    
    [SerializeField] private UnityEvent shootEvent;
    [SerializeField] private Transform shootVfxTransform;

    #region Ship Behaviour

    public void StartGameLoop()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SetCurrentLifeServerRpc(maxLife);
            SetStarCountClientRpc(0);
        }
    }

    public void UpdateGameLoop()
    {
        if(canRegenerate) Regenerate();
    }

    private void Regenerate()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        if (regenerationTimer >= 1)
        {
            regenerationTimer -= 1;
            SetCurrentLifeServerRpc(currentBoatLife.Value + regenerationPerSecond);
        }
        else regenerationTimer += Time.deltaTime;
    }

    #endregion

    #region Data Setter
    
    public void SetUnderAttack(bool underAttack)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host should manage shrimp shrimp attack!");
            return;
        }

        this.underAttack.Value = underAttack;
    }

    public void TakeDamage(int damage)
    {
        SetCurrentLifeServerRpc(currentBoatLife.Value - damage);
    }

    public void SetRegenerationAbility(bool active)
    {
        canRegenerate = active;
    }

    #endregion

    #region Network

 
    [ServerRpc(RequireOwnership = true)]
    private void SetCurrentLifeServerRpc(int life)
    {
        if (life > maxLife) currentBoatLife.Value = maxLife;
        else currentBoatLife.Value = math.max(0, life);
        SetCurrentLifeClientRpc(currentBoatLife.Value);
        if (currentBoatLife.Value == 0)
            GameManager.instance.GameEnds(false,
                EndGameReason.ShipDestroyed); // Todo - Shouldn't end the game, only stop ship for some time
    }

    [ClientRpc]
    private void SetCurrentLifeClientRpc(int life)
    {
        MainCanvasManager.instance.SetLifeOnDisplay(life, maxLife);
    }

    [ClientRpc]
    private void SetStarCountClientRpc(int count)
    {
        MainCanvasManager.instance.SetStarOnDisplay(count);
    }
    

    #endregion

    #region Data Getter
    
    public bool IsUnderAttack()
    {
        return underAttack.Value;
    }

    #endregion

    #region Cannon Management

    [ServerRpc(RequireOwnership = false)]
    public void FireServerRpc(byte index)
    {
        Vector3 origin = index switch
        {
            0 => leftTopCannonOrigin.position,
            1 => rightTopCannonOrigin.position,
            2 => leftBottomCannonOrigin.position,
            3 => rightBottomCannonOrigin.position,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        FireClientRpc(origin, index);

        Vector3 direction = index switch
        {
            0 or 1 => Vector3.forward,
            2 or 3 => Vector3.back,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f, shrimpShipLayer))
        {
            var shrimpShipEvent = hit.transform.parent.parent.parent.GetComponent<ShrimpShipAttackEvent>();
            if (!shrimpShipEvent)
            {
                Debug.LogError(
                    "Shrimp ship event script was not found. Searched on parent's parent's parent of collision box.");
                return;
            }

            shrimpShipEvent.GetHit();
        }
    }

    [ClientRpc]
    private void FireClientRpc(Vector3 feedbackPos, byte index)
    {
        shootVfxTransform.position = feedbackPos;
        shootVfxTransform.eulerAngles = Vector3.up * (index is 0 or 1 ? 0 : 180);
        shootEvent?.Invoke();
    }

    #endregion
}