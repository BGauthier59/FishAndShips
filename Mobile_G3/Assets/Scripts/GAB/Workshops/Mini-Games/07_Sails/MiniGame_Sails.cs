using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
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

    private NetworkVariable<bool> isCooldown = new NetworkVariable<bool>();
    [SerializeField] private float cooldown;
    private float timer;

    [SerializeField] private byte stepNumber;
    private byte currentStep;

    public TMP_Text myStep;
    public TMP_Text cooldownTimer;

    [SerializeField] private MeshRenderer[] sailsRenderers;
    private bool canSwipe;
    private static readonly int Ratio = Shader.PropertyToID("_Ratio");

    public override void AssociatedWorkshopGetActivatedHostSide()
    {
        base.AssociatedWorkshopGetActivatedHostSide();
        SetSailShaderServerRpc(0);
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        //SetSailRenderers(0, 1);

        var connectedWorkshop = WorkshopManager.instance.GetCurrentWorkshop() as ConnectedWorkshop;
        if (connectedWorkshop == null)
        {
            Debug.LogError("Sails mini-game must be started as a connected workshop!");
            return;
        }

        if (IsFirstPlayer())
        {
            currentData = firstPlayerData;
            currentRope = leftRope;
            rightRope.SetActive(false);
            isCooldown.OnValueChanged += OnCooldownValueChanged;
        }
        else
        {
            currentData = secondPlayerData;
            currentRope = rightRope;
            leftRope.SetActive(false);
        }


        await Task.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());

        WorkshopManager.instance.swipeManager.Enable(currentData);
        StartExecutingMiniGame();
    }

    private async void SetSailRenderers(float ratioValue, float duration)
    {
        canSwipe = false;
        float currentValue = sailsRenderers[0].material.GetFloat(Ratio);
        float timer = 0f;

        while (timer < duration)
        {
            foreach (var rd in sailsRenderers)
            {
                rd.material.SetFloat(Ratio, math.lerp(currentValue, ratioValue, timer / duration));
            }

            await Task.Yield();
            timer += Time.deltaTime;
        }

        foreach (var rd in sailsRenderers)
        {
            rd.material.SetFloat(Ratio, ratioValue);
        }

        canSwipe = true;
    }

    public override void ExecuteMiniGame()
    {
        if (!canSwipe) return;
        if (WorkshopManager.instance.swipeManager.CalculateSwipe() && currentStep < stepNumber)
        {
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
            ResetSailServerRpc(NetworkManager.LocalClientId, GetOtherPlayerId());
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private async void SailIsRaised()
    {
        // Todo - play animation

        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        ExitMiniGame(true);
    }

    protected override void ExitMiniGame(bool victory)
    {
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
            SetSailShaderServerRpc(step);
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
    private void SetSailShaderServerRpc(byte step)
    {
        SetSailShaderClientRpc(step);
    }

    [ClientRpc]
    private void SetSailShaderClientRpc(byte step)
    {
        SetSailRenderers(step / (float) stepNumber, .5f);
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
        currentStep = 0;
        SetSailRenderers(0, .5f);
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