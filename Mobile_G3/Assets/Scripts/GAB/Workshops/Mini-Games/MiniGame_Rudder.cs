using TMPro;
using Unity.Mathematics;
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
    }

    public override void ExecuteMiniGame()
    {
        var angle = WorkshopManager.instance.rudderCircularSwipeManager.CalculateCircularSwipe();
        if (angle.HasValue)
        {
            data.rudder.eulerAngles += Vector3.forward * angle.Value;
            
            // Todo - Check ça en dessous c'est placeholder
            
            currentRotationPerSecond = math.lerp(minMaxRotationPerSecond.x, minMaxRotationPerSecond.y, data.rudder.eulerAngles.z / 180);
            ShipManager.instance.SetRotation(currentRotationPerSecond);
        }
    }

    public override void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.rudderCircularSwipeManager.Disable();
        base.ExitMiniGame(victory);
    }
}