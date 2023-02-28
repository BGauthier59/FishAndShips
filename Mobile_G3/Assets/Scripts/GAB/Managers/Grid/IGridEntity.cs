using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridEntity
{
    public void SetToGrid(int x, int y);
    public void OnCollision(IGridEntity other);
}
