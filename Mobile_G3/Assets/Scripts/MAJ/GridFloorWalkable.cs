using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFloorWalkable : MonoBehaviour, IGridFloor
{
    public int positionX;
    public int positionY;

    
    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity)
    {
        entity.SetPosition(positionX, positionY);
    }

    // Quand entity a atteint cette tile
    public void OnLand(IGridEntity entity)
    {
        PlayerManager player = entity as PlayerManager;
        if (player) Destroy(Instantiate(player.fxTest, transform.position + Vector3.up * 0.2f, Quaternion.identity), 2);
    }
}