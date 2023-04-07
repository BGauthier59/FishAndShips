using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame_Cannon_Shoot : MiniGame
{
    public override void StartMiniGame()
    {
        base.StartMiniGame();
        // Enable input
    }
    
    public override void ExecuteMiniGame()
    {
        // Approcher allumette lentement sur une zone spécifique
        
        // Bool : false : l'allumette n'est pas sur la cible, ou elle est allée trop vite et s'est éteinte
        // true : l'allumette a atteint la cible
        
        // Si alumette s'éteint, il faut en reprendre une dans la zone
    }
    
    public override void ExitMiniGame(bool victory)
    {
        // Disable input
        base.ExitMiniGame(victory);
    }
}
