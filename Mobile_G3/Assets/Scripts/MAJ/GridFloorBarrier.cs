using System.Collections;
using System.Collections.Generic;
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
    
    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity,int direction)
    {
        switch (direction)
        {
            case 0:
                if (bottomBarrier && bottomBarrier.isClosed)
                {
                    MoveOnClosedBarrier();
                    return;
                } 
                break;
            case 1:
                if (leftBarrier && leftBarrier.isClosed)
                {
                    MoveOnClosedBarrier();
                    return;
                }
                break;
            case 2:
                if (topBarrier && topBarrier.isClosed)
                {
                    MoveOnClosedBarrier();
                    return;
                }
                break;
            case 3:
                if (rightBarrier && rightBarrier.isClosed)
                {
                    MoveOnClosedBarrier();
                    return;
                }
                break;
        }
        entity.SetPosition(positionX, positionY);
    }

    void MoveOnClosedBarrier()
    {
        
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
        
        if(pressurePlate)BarrierManager.instance.SwitchBarriers();
    }
}