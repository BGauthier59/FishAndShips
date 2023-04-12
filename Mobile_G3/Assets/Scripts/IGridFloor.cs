using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridFloor
{
    void SetPosition(int posX, int posY);
    void OnMove(IGridEntity entity,int direction);
    void OnLand(IGridEntity entity);
}
