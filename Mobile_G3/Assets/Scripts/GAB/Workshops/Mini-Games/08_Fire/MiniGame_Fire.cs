using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame_Fire : MiniGame
{
    /* 23/04
     * On ne produit pas ce mini-jeu pour l'instant !
     *
     * Priorité sur les choses pas encore produites
     * GameLoop, Events, etc
     */
    
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private float damagePerSecond;

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        
        // Enables Gyroscope
        WorkshopManager.instance.gyroscopeManager.Enable(data);
        
        StartExecutingMiniGame();
    }
    
    public override void AssociatedWorkshopGetActivated()
    {
        // Todo - Boat takes damages
    }
    

    public override void ExecuteMiniGame()
    {
        // 
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
