using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MiniGame_Cannon_Shoot : MiniGame
{
    [SerializeField] private CannonDragAndDropData data;

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
            ExitMiniGame(true);
        }
    }

    public override async void ExitMiniGame(bool victory)
    {
        StopExecutingMiniGame();
        await Task.Delay(1000);
        WorkshopManager.instance.cannonDragAndDropManager.Disable();
        base.ExitMiniGame(victory);
    }
}