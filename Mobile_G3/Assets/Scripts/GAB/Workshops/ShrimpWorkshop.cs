using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrimpWorkshop : Workshop
{

    public void UpdateGameLoop()
    {
        // Todo - stocked and called on WorkshopManager
    }
    
    public override void SetPosition(int posX, int posY)
    {
        base.SetPosition(posX, posY);
        
    }

    protected override void RemoveWorkshopFromGrid()
    {
        // Called by every client when workshop is over
        base.RemoveWorkshopFromGrid();
        if (Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            EventsManager.instance.RemoveShrimp();
        }
    }
}
