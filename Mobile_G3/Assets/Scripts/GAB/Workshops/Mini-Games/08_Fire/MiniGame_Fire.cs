using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame_Fire : MiniGame
{
    public override void StartMiniGame()
    {
        base.StartMiniGame();
        
        // Enables Gyroscope
        
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        
    }

    public override void Reset()
    {
        
    }

    protected override void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }
}
