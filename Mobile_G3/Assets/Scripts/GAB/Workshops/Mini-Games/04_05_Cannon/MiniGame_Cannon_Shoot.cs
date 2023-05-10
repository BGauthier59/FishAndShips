using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class MiniGame_Cannon_Shoot : MiniGame
{
    [SerializeField] private CannonDragAndDropData data;
    private byte index;

    [Header("Feedbacks")] [SerializeField] private UnityEvent inGameShootEvent;
    [SerializeField] private ParticleSystem fireStart, fireStop;
    
    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        WorkshopManager.instance.cannonDragAndDropManager.OnMatchstickFireStart = () =>
        {
            fireStart.Play();
            fireStop.Stop();
        };
        WorkshopManager.instance.cannonDragAndDropManager.OnMatchstickFireStop = () =>
        {
            fireStart.Stop();
            fireStop.Play();
        };
        
        await Task.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        WorkshopManager.instance.cannonDragAndDropManager.Enable(data);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.cannonDragAndDropManager.CalculateMatchStickPosition())
        {
            Fire();
        }
    }

    public override void TransferDataFromWorkshopWhenMiniGameStarts(Workshop workshop)
    {
        index = ((SeriesWorkshop) workshop).GetCannonIndex();
    }

    private async void Fire()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.cannonDragAndDropManager.Disable();

        inGameShootEvent?.Invoke();
        
        ShipManager.instance.FireServerRpc(index);
        
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
    }

    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        StopExecutingMiniGame();
        WorkshopManager.instance.cannonDragAndDropManager.Disable();
        ExitMiniGame(false);
    }
}