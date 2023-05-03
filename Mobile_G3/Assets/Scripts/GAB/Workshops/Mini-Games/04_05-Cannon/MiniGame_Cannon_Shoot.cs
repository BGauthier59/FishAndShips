using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class MiniGame_Cannon_Shoot : MiniGame
{
    [SerializeField] private CannonDragAndDropData data;
    private byte index;
<<<<<<< Updated upstream

    [Header("Feedbacks")] [SerializeField] private UnityEvent inGameShootEvent;
    
    public override async void StartMiniGame()
=======
    
    public override void StartMiniGame()
>>>>>>> Stashed changes
    {
        base.StartMiniGame();
        
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

<<<<<<< Updated upstream
        inGameShootEvent?.Invoke();
        
        ShipManager.instance.FireServerRpc(index);
        
        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
=======
        // Todo - Feedback

        ShipManager.instance.FireServerRpc(index);
>>>>>>> Stashed changes
        
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