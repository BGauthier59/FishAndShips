using System;
using UnityEngine;

public class MiniGame_Map : MiniGame
{
    public float sizeRatio;
    [SerializeField] private Transform map;
    [SerializeField] private Transform ship;
    private Vector3 shipPosition;
    private float posY;

    [SerializeField] private LineRenderer shipPath;

    public override void StartMiniGame()
    {
        base.StartMiniGame();
    }

    public override void ExecuteMiniGame()
    {
    }

    public static Action OnShipRotationChange;

    public void Initialize()
    {
        sizeRatio = map.localScale.x / ShipManager.instance.realMap.localScale.x;
        posY = ship.transform.localPosition.y;

        SetShipPosition();

        shipPath.positionCount = 2;
        shipPath.SetPosition(0, ship.transform.position);

        OnShipRotationChange += AddPointOnPath;
    }

    public void Refresh()
    {
        SetShipPosition();

        ship.eulerAngles = ShipManager.instance.GetShipAngle() * Vector3.up;

        shipPath.SetPosition(shipPath.positionCount - 1, ship.transform.position);
    }

    private void SetShipPosition()
    {
        shipPosition = ShipManager.instance.GetShipPositionOnMap() * sizeRatio;
        Debug.Log(shipPosition);
        shipPosition.z = shipPosition.y;
        shipPosition.y = posY;
        ship.transform.localPosition = shipPosition;
    }

    private void AddPointOnPath()
    {
        shipPath.SetPosition(shipPath.positionCount - 1, ship.transform.position);
        shipPath.positionCount++;
        shipPath.SetPosition(shipPath.positionCount - 1, ship.transform.position);
    }
}