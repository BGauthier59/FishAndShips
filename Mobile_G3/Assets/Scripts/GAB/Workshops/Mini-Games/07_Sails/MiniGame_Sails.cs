using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class MiniGame_Sails : MiniGame
{
    [SerializeField] private SwipeData firstPlayerData;
    [SerializeField] private SwipeData secondPlayerData;
    private SwipeData currentData;

    [SerializeField] private GameObject leftRope;
    [SerializeField] private GameObject rightRope;
    private GameObject currentRope;

    private bool isCooldown;
    [SerializeField] private float cooldown;
    private float timer;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(firstPlayerData.centralPoint.position, firstPlayerData.rightDirection);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(secondPlayerData.centralPoint.position, secondPlayerData.rightDirection);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        var connectedWorkshop = WorkshopManager.instance.GetCurrentWorkshop() as ConnectedWorkshop;
        if (connectedWorkshop == null)
        {
            Debug.LogWarning("Sails mini-game must be started as a connected worshop!");
            return;
        }

        if (IsFirstPlayer())
        {
            currentData = firstPlayerData;
            currentRope = leftRope;
            rightRope.SetActive(false);
        }
        else
        {
            currentData = secondPlayerData;
            currentRope = rightRope;
            leftRope.SetActive(false);
        }

        WorkshopManager.instance.swipeManager.Enable(currentData);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.swipeManager.CalculateSwipe())
        {
            Debug.Log("Swipe is okay!");
            WorkshopManager.instance.swipeManager.Disable();

            // Check if other is good by sending an RPC to this specific client
            SetSailStateServerRpc(GetOtherPlayerId());
            isCooldown = true;
        }

        if (isCooldown) CheckTimer();
    }

    private void CheckTimer()
    {
        if (timer >= cooldown)
        {
            Debug.Log("Too late");
            isCooldown = false;
            timer = 0;
            WorkshopManager.instance.swipeManager.Enable(currentData);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    public override async void ExitMiniGame(bool victory)
    {
        StopExecutingMiniGame();
        isCooldown = false;
        WorkshopManager.instance.swipeManager.Disable();
        await Task.Delay(1000);
        base.ExitMiniGame(victory);
    }

    #region Network

    [ServerRpc(RequireOwnership = false)]
    private void SetSailStateServerRpc(ulong id)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };
        SetSailStateClientRpc(parameters);
    }

    [ClientRpc]
    private void SetSailStateClientRpc(ClientRpcParams parameters)
    {
        if (isCooldown)
        {
            ExitMiniGame(true);
            ReceiveExitOrderServerRpc(GetOtherPlayerId());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceiveExitOrderServerRpc(ulong id)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };
        ReceiveExitOrderClientRpc(parameters);
    }

    [ClientRpc]
    private void ReceiveExitOrderClientRpc(ClientRpcParams parameters)
    {
        ExitMiniGame(true);
    }

    private ulong GetOtherPlayerId()
    {
        return ((ConnectedWorkshop) WorkshopManager.instance.GetCurrentWorkshop()).GetOtherPlayerId();
    }

    private bool IsFirstPlayer()
    {
        var connectedWorkshop = (ConnectedWorkshop) WorkshopManager.instance.GetCurrentWorkshop();
        return connectedWorkshop.GetOtherPlayerId() > NetworkManager.Singleton.LocalClientId;
    }

    #endregion
}