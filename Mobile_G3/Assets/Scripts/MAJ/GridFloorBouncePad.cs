using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFloorBouncePad : MonoBehaviour, IGridFloor
{
    public int positionX;
    public int positionY;

    public int bounceDirection;

    
    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity,int direction)
    {
        entity.SetPosition(positionX, positionY);
    }

    // Quand entity a atteint cette tile
    public void OnLand(IGridEntity entity)
    {
        PlayerManager player = entity as PlayerManager;
        if (player)
        {
            //Destroy(Instantiate(player.fxTest, transform.position + Vector3.up * 0.2f, Quaternion.identity), 2);
            player.isGliding = false;
        }
        switch (bounceDirection)
        {
            case 0:
                GridManager.instance.GetTile(positionX, positionY+1).OnInteraction(entity,bounceDirection);
                break;
            case 1:
                GridManager.instance.GetTile(positionX+1, positionY).OnInteraction(entity,bounceDirection);
                break;
            case 2:
                GridManager.instance.GetTile(positionX, positionY-1).OnInteraction(entity,bounceDirection);
                break;
            case 3:
                GridManager.instance.GetTile(positionX-1, positionY).OnInteraction(entity,bounceDirection);
                break;
        }
    }
}