using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFloorIce : MonoBehaviour, IGridFloor
{
    public int positionX;
    public int positionY;

    public int directionSaved;

    
    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity,int direction)
    {
        entity.SetPosition(positionX, positionY);
        directionSaved = direction;
    }

    // Quand entity a atteint cette tile
    public void OnLand(IGridEntity entity)
    {
        PlayerManager player = entity as PlayerManager;
        if (player && !player.isGliding) Destroy(Instantiate(player.fxTest, transform.position + Vector3.up * 0.2f, Quaternion.identity), 2);
        if (player != null) player.isGliding = true;
        switch (directionSaved)
        {
            case 0:
                GridManager.instance.GetTile(positionX, positionY+1).OnInteraction(entity,directionSaved);
                break;
            case 1:
                GridManager.instance.GetTile(positionX+1, positionY).OnInteraction(entity,directionSaved);
                break;
            case 2:
                GridManager.instance.GetTile(positionX, positionY-1).OnInteraction(entity,directionSaved);
                break;
            case 3:
                GridManager.instance.GetTile(positionX-1, positionY).OnInteraction(entity,directionSaved);
                break;
        }
    }
}