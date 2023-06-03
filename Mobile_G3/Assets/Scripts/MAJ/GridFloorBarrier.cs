using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridFloorBarrier : MonoBehaviour, IGridFloor
{
    public int positionX;
    public int positionY;

    public GridBarrier topBarrier;
    public GridBarrier bottomBarrier;
    public GridBarrier leftBarrier;
    public GridBarrier rightBarrier;

    public bool pressurePlate;
    public bool ice;
    
    public int directionSaved;

    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity, int direction)
    {
        switch (direction)
        {
            case 0:
                if (bottomBarrier && bottomBarrier.isClosed.Value)
                {
                    MoveOnClosedBarrier();
                    return;
                }

                break;
            case 1:
                if (leftBarrier && leftBarrier.isClosed.Value)
                {
                    MoveOnClosedBarrier();
                    return;
                }

                break;
            case 2:
                if (topBarrier && topBarrier.isClosed.Value)
                {
                    MoveOnClosedBarrier();
                    return;
                }

                break;
            case 3:
                if (rightBarrier && rightBarrier.isClosed.Value)
                {
                    MoveOnClosedBarrier();
                    return;
                }

                break;
        }

        entity.SetPosition(positionX, positionY);
        if(ice) directionSaved = direction;
    }

    void MoveOnClosedBarrier()
    {
    }

    // Quand entity a atteint cette tile
    public void OnLand(IGridEntity entity)
    {
        PlayerManager player = entity as PlayerManager;
        if (!ice)
        {
            if (player)
            {
                //Destroy(Instantiate(player.fxTest, transform.position + Vector3.up * 0.2f, Quaternion.identity), 2);
                player.isGliding = false;
            } 
        }
        else
        {
            if (player != null) player.isGliding = true;
            switch (directionSaved)
            {
                case 0:
                    GridManager.instance.GetTile(positionX, positionY+1).OnInteraction(entity,directionSaved,true);
                    break;
                case 1:
                    GridManager.instance.GetTile(positionX+1, positionY).OnInteraction(entity,directionSaved,true);
                    break;
                case 2:
                    GridManager.instance.GetTile(positionX, positionY-1).OnInteraction(entity,directionSaved,true);
                    break;
                case 3:
                    GridManager.instance.GetTile(positionX-1, positionY).OnInteraction(entity,directionSaved,true);
                    break;
            }
        }

        if (pressurePlate) BarrierManager.instance.SwitchBarriers(new int2(positionX,positionY));
    }
}