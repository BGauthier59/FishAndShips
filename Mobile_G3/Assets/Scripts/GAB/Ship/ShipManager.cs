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
        
    }

    #endregion

    #region Data Setter
    
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
}