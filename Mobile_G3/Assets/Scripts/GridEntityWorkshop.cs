using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityWorkshop : MonoBehaviour,IGridEntity
{
    public int positionX,positionY;
    public bool charged;

    public void OnCollision(IGridEntity entity)
    {
        if (!charged)
        {
            OnAtelierCharge();
        }
        else
        {
            OnAtelierShoot();
        }
    }

    public void SetPosition(int posX, int posY)
    {
        
    }

    void OnAtelierCharge()
    {
        
    }
    
    void OnAtelierShoot()
    {
        
    }
}
