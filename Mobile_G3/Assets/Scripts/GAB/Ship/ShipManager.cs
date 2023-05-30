using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ShipManager : NetworkMonoSingleton<ShipManager>
{
    private NetworkVariable<float> currentBoatLife = new NetworkVariable<float>();
    [SerializeField] private int maxLife;

    [SerializeField] private int3 starScores;

    #region Ship Behaviour

    public void StartGameLoop()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SetCurrentLifeServerRpc(0);
            //SetStarCountClientRpc(0);
        }
    }

    public void UpdateGameLoop()
    {
    }

    #endregion

    #region Data Setter

    public void TakeDamage(float damage)
    {
        // Host only
        if (currentBoatLife.Value - damage < 0)
        {
            SetCurrentLifeServerRpc(0);
        }
        else
        {
            SetCurrentLifeServerRpc(currentBoatLife.Value - damage);
        }
    }
    
    public void GivePoint(float points)
    {
        if (currentBoatLife.Value + points > maxLife)
        {
            SetCurrentLifeServerRpc(maxLife);
        }
        else
        {
            SetCurrentLifeServerRpc(currentBoatLife.Value + points);
        }
    }

    #endregion

    #region Network

    [ServerRpc(RequireOwnership = true)]
    private void SetCurrentLifeServerRpc(float life)
    {
        if (life > maxLife) currentBoatLife.Value = maxLife;
        else currentBoatLife.Value = math.max(0, life);
        SetCurrentLifeClientRpc(currentBoatLife.Value);
        /*if (currentBoatLife.Value == 0)
            GameManager.instance.GameEnds(false);*/
    }

    [ClientRpc]
    private void SetCurrentLifeClientRpc(float life)
    {
        MainCanvasManager.instance.SetLifeOnDisplay(life, maxLife);
    }

    #endregion

    public int EvaluateStarScore()
    {
        var score = currentBoatLife.Value;
        Debug.Log($"Ship life value was {score}");

        var count = 0;
        if (score >= starScores.x) count++;
        if (score >= starScores.y) count++;
        if (score >= starScores.z) count++;
        return count;
    }
}