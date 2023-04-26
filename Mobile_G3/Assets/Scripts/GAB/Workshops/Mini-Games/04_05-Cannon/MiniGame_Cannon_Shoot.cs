using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MiniGame_Cannon_Shoot : MiniGame
{
    [SerializeField] private CannonDragAndDropData data;
    private byte index;
    
    public override void StartMiniGame()
    {
        base.StartMiniGame();
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

        // Todo - Feedback

        ShipManager.instance.FireServerRpc(index);
        
        await Task.Delay(1000);
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