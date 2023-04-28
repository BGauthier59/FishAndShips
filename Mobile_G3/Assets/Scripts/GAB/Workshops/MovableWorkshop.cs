using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableWorkshop : Workshop
{
    // Used for Shrimp for instance

    public void UpdateGameLoop()
    {
        // Todo - stocked and called on WorkshopManager
    }
    
    public override void SetPosition(int posX, int posY)
    {
        base.SetPosition(posX, posY);
        
    }
}
