using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridEntity
{
    void OnCollision(IGridEntity entity,int direction);
    
    void SetPosition(int posX,int posY);
}
