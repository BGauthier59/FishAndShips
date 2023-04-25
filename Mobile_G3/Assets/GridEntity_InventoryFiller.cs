using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity_InventoryFiller : MonoBehaviour, IGridEntity
{
    public InventoryObject filling;
    

    public void OnCollision(IGridEntity entity, int direction)
    {
        PlayerManager player = entity as PlayerManager;
        if (player)
        {
            player.ChangeInventoryObject(filling);
        }
    }

    public void SetPosition(int posX, int posY)
    {
        
    }
}
