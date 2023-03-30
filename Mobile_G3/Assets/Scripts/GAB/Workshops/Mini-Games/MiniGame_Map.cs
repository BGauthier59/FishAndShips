using UnityEngine;

public class MiniGame_Map : MiniGame
{
    public float sizeRatio;
    [SerializeField] private Transform map;
    [SerializeField] private Transform ship;
    private Vector3 shipPosition;
    private float posY;
    
    public override void StartMiniGame()
    {
        base.StartMiniGame();
        sizeRatio = map.localScale.x / ShipManager.instance.realMap.localScale.x;
        posY = ship.transform.localPosition.y;
    }

    public override void ExecuteMiniGame()
    {
        shipPosition = ShipManager.instance.GetShipPositionOnMap() * sizeRatio;
        shipPosition.y = posY;
        ship.transform.localPosition = shipPosition;
        
        ship.transform.rotation = ShipManager.instance.GetShipRotation();
    }
}
