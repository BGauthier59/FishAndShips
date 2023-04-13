using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class MiniGame_Rudder : MiniGame
{
    [SerializeField] private RudderCircularSwipeSetupData data;

    private float currentRotationPerSecond;
    [SerializeField] private float2 minMaxRotationPerSecond;

    private void OnValidate()
    {
        data.availableZone.position = data.centralPoint.position;
        data.availableZone.localScale = new Vector3(data.minMaxMagnitude.y * 2, data.minMaxMagnitude.y * 2, 1);
        data.unavailableCenterZone.position = data.centralPoint.position;
        data.unavailableCenterZone.localScale = new Vector3(data.minMaxMagnitude.x * 2, data.minMaxMagnitude.x * 2, 1);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        WorkshopManager.instance.rudderCircularSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    private Vector3 nextValue;

    public override void ExecuteMiniGame()
    {
        var angle = WorkshopManager.instance.rudderCircularSwipeManager.CalculateCircularSwipe();
        if (angle.HasValue)
        {
            nextValue = data.rudder.eulerAngles + Vector3.forward * angle.Value.eulerAngles;
            data.rudder.eulerAngles = nextValue;

            var ratio = angle.Value.degrees > 0
                ? math.lerp(.5, 1, angle.Value.degrees / data.maxRotationDegree)
                : math.lerp(0.5, 0, math.abs(angle.Value.degrees) / data.maxRotationDegree);

            currentRotationPerSecond = math.lerp(minMaxRotationPerSecond.y, minMaxRotationPerSecond.x, (float) ratio);
            ShipManager.instance.SetRotation(currentRotationPerSecond);
        }
    }

    protected override void ExitMiniGame(bool victory)
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.rudderCircularSwipeManager.Disable();
        SetRudderRotationServerRpc(data.rudder.eulerAngles.z);
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        // Should not be reset
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRudderRotationServerRpc(float angle)
    {
        data.rudder.eulerAngles = Vector3.forward * angle;
    }
}