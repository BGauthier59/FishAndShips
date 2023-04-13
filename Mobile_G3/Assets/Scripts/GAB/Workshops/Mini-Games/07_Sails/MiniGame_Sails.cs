using System.Threading.Tasks;
using TMPro;
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

    //private bool isCooldown;
    private NetworkVariable<bool> isCooldown = new NetworkVariable<bool>();
    [SerializeField] private float cooldown;
    private float timer;

    [SerializeField] private byte stepNumber;
    private byte currentStep;

    public TMP_Text myStep;
    public TMP_Text cooldownTimer;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(firstPlayerData.centralPoint.position, firstPlayerData.rightDirection);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(secondPlayerData.centralPoint.position, secondPlayerData.rightDirection);
    }

    // Chaque joueur a une corde
    // Chaque joueur a une valeur de step (de 0 à 3)
    // Quand le joueur swipe, il monte d'une step, il envoie à l'autre joueur sa valeur de step, et si les deux valeurs sont différentes, alors un timer est enclenché
    // Si les deux valeurs de step sont les mêmes, le timer est reset et stoppé
    // Si le timer dépasse X secondes, les deux joueurs voient leur step revenir à 0
    // Si les deux joueurs ont leur step à 3, ils ont gagné

    // Le timer se lance sur le joueur 1 ?

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        var connectedWorkshop = WorkshopManager.instance.GetCurrentWorkshop() as ConnectedWorkshop;
        if (connectedWorkshop == null)
        {
            Debug.LogWarning("Sails mini-game must be started as a connected workshop!");
            return;
        }

        if (IsFirstPlayer())
        {
            currentData = firstPlayerData;
            currentRope = leftRope;
            rightRope.SetActive(false);
            // Timer is only checked on first player
            isCooldown.OnValueChanged += OnCooldownValueChanged;
            Debug.Log("You're player 1");
        }
        else
        {
            currentData = secondPlayerData;
            currentRope = rightRope;
            leftRope.SetActive(false);
            Debug.Log("You're player 2");
        }

        WorkshopManager.instance.swipeManager.Enable(currentData);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.swipeManager.CalculateSwipe())
        {
            Debug.Log("Swipe is okay!");
           //WorkshopManager.instance.swipeManager.Disable();
            currentStep++;
            
            myStep.text = currentStep.ToString();
            
            SetSailStateServerRpc(GetOtherPlayerId(), currentStep);
        }

        if (IsFirstPlayer() && isCooldown.Value) CheckTimer();
    }

    private void CheckTimer()
    {
        cooldownTimer.text = timer.ToString("F1");
        if (timer >= cooldown)
        {
            Debug.LogWarning("Too late!");
            ResetSailServerRpc(NetworkManager.LocalClientId, GetOtherPlayerId());
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private async void SailIsRaised()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        await Task.Delay(1000);
        ExitMiniGame(true);
    }

    protected override void ExitMiniGame(bool victory)
    {
        // Todo - Should disconnect other player too
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        if (IsFirstPlayer()) isCooldown.OnValueChanged -= OnCooldownValueChanged;
        SetCooldownServerRpc(false);
        timer = 0;
        currentStep = 0;
    }

    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        ExitMiniGame(false);
    }

    #region Network

    [ServerRpc(RequireOwnership = false)]
    private void SetSailStateServerRpc(ulong id, byte step)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };
        SetSailStateClientRpc(step, parameters);
    }

    [ClientRpc]
    private void SetSailStateClientRpc(byte step, ClientRpcParams parameters)
    {
        Debug.Log($"Other is at step {step}. You're at step {currentStep}");
        myStep.text = currentStep.ToString();
        
        if (step == currentStep)
        {
            SetCooldownServerRpc(false);
            if (step == stepNumber)
            {
                SailIsRaised();
                ReceiveExitOrderServerRpc(GetOtherPlayerId());
            }
        }
        else
        {
            SetCooldownServerRpc(true);
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
        SailIsRaised();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCooldownServerRpc(bool active)
    {
        Debug.Log($"Set cooldown value to {active}");
        isCooldown.Value = active;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetSailServerRpc(ulong id, ulong otherPlayerId)
    {
        SetCooldownServerRpc(false);

        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id, otherPlayerId}
            }
        };
        ResetSailClientRpc(parameters);
    }

    [ClientRpc]
    private void ResetSailClientRpc(ClientRpcParams parameters)
    {
        // Both players see their sail reset
        Debug.Log("I've been reset to 0");
        currentStep = 0;

        myStep.text = currentStep.ToString();
    }

    private void OnCooldownValueChanged(bool previous, bool current)
    {
        if (!current)
        {
            Debug.LogWarning("Timer has been reset! Steps are the same");
            timer = 0;
            cooldownTimer.text = timer.ToString("F1");
        }
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