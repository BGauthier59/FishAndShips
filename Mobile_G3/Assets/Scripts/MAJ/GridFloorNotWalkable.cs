using UnityEngine;

public class GridFloorNotWalkable : MonoBehaviour, IGridFloor
{
    public void SetPosition(int posX, int posY)
    {
    }

    public void OnMove(IGridEntity entity, int direction)
    {
        // Nothing should happen, or maybe a feedback?
    }

    public void OnLand(IGridEntity entity)
    {
    }
}
