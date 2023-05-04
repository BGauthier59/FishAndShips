using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReparationWorkshop : Workshop
{
    public override void SetPosition(int posX, int posY)
    {
        if (currentTile == null)
        {
            Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null, currentTile.GetFloor());

        base.SetPosition(posX, posY);
    }
    
    protected override void RemoveWorkshopFromGrid()
    {
        // Called by every client when workshop is over
        Debug.Log("Remove workshop from grid HHH");

        base.RemoveWorkshopFromGrid();
        if (Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            EventsManager.instance.RemoveHole();
        }
    }
}
