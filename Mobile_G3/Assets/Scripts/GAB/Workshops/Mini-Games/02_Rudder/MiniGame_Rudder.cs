using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class MiniGame_Rudder : MiniGame
{
    [SerializeField] private RudderCircularSwipeSetupData data;
    [SerializeField] private MiniGame_Map mapMiniGame;

    private float currentRotation;
    [SerializeField] private float2 minMaxRotationPerSecond;
    [SerializeField] private TMP_Text questIndicator;
    [SerializeField] private string questStartText;

    public override void StartMiniGame()
    {
        // Map mini-game starts Rudder mini-game
    }

    private Vector3 nextValue;

    public override void ExecuteMiniGame()
    {
        var angle = WorkshopManager.instance.rudderCircularSwipeManager.CalculateCircularSwipe();
        if (angle.HasValue)
        {
            WorkshopManager.instance.StopMiniGameTutorial();
            nextValue = data.rudder.eulerAngles + Vector3.forward * angle.Value.eulerAngles;
            data.rudder.eulerAngles = nextValue;

            var ratio = angle.Value.degrees > 0
                ? math.lerp(.5, 1, angle.Value.degrees / data.maxRotationDegree)
                : math.lerp(0.5, 0, math.abs(angle.Value.degrees) / data.maxRotationDegree);

            currentRotation = math.lerp(minMaxRotationPerSecond.x, minMaxRotationPerSecond.y, (float) ratio);

            mapMiniGame.UpdateShipRotationServerRpc(currentRotation, GetOtherPlayerId());
        }
    }

    public override void Reset()
    {
        // Should not be reset
        currentRotation = 0;
        data.rudder.eulerAngles = Vector3.zero;
    }

    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        StopExecutingMiniGame();
        WorkshopManager.instance.rudderCircularSwipeManager.Disable();
        ExitMiniGame(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitiateStartOfGameServerRpc(ulong id, string name)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };
        InitiateStartOfGameClientRpc(name, parameters);
    }

    [ClientRpc]
    private void InitiateStartOfGameClientRpc(string name, ClientRpcParams parameters)
    {
        StartMiniGameNetwork(name);
    }

    private async void StartMiniGameNetwork(string name)
    {
        miniGameObject.SetActive(true);
        WorkshopManager.instance.SetGameIndicator(indicatorGame);

        questIndicator.text = $"{questStartText} {name}!";

        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        WorkshopManager.instance.StartMiniGameTutorial(6);
        WorkshopManager.instance.rudderCircularSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitiateEndOfGameServerRpc(ulong id)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };

        InitiateEndOfGameClientRpc(parameters);
    }

    [ClientRpc]
    private void InitiateEndOfGameClientRpc(ClientRpcParams parameter)
    {
        RudderIsSet();
    }

    private async void RudderIsSet()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.mapSwipeManager.Disable();
        WorkshopManager.instance.SetVictoryIndicator();
        HonorificManager.instance.AddHonorific(Honorifics.Sailor, Honorifics.TeamSpirit);

        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        ExitMiniGame(true);
    }

    private ulong GetOtherPlayerId()
    {
        return ((ConnectedWorkshop) WorkshopManager.instance.GetCurrentWorkshop()).GetOtherPlayerId();
    }
}