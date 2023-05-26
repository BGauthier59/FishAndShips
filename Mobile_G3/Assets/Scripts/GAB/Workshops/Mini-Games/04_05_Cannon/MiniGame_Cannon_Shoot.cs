using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class MiniGame_Cannon_Shoot : MiniGame
{
    [SerializeField] private CannonDragAndDropData data;

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
        
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        WorkshopManager.instance.cannonDragAndDropManager.Enable(data);
        WorkshopManager.instance.StartMiniGameTutorial(3);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        if (WorkshopManager.instance.cannonDragAndDropManager.CalculateMatchStickPosition())
        {
            Fire();
        }
    }

    private async void Fire()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.cannonDragAndDropManager.Disable();

        inGameShootEvent?.Invoke();
        
        //ShipManager.instance.FireServerRpc(index);
        
        WorkshopManager.instance.SetVictoryIndicator();
        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

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