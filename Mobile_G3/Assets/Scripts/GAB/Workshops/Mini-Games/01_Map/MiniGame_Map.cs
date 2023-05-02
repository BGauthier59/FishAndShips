using System;
using Unity.Netcode;
using UnityEngine;

public class MiniGame_Map : MiniGame
{
    [SerializeField] private MapSwipeData data;
    public float sizeRatio;
    [SerializeField] private Transform map;
    [SerializeField] private Transform ship;
    [SerializeField] private Transform miniGameMap;
    private Vector3 shipPosition;
    private Vector3 miniGameMapPosition;
    private float posY;
    private Vector3 initMapPos;

    [SerializeField] private LineRenderer shipPath;

    [SerializeField] private SpriteRenderer[] stars;

    [SerializeField] private Vector4 leftRightBottomTopBorders;

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        SetRelativeMapPosition();
        
        // Enable input
        WorkshopManager.instance.mapSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        // Can move the map
        miniGameMapPosition += WorkshopManager.instance.mapSwipeManager.CalculateMoveDelta();

        if (miniGameMapPosition.x < leftRightBottomTopBorders.x) miniGameMapPosition.x = leftRightBottomTopBorders.x;
        else if (miniGameMapPosition.x > leftRightBottomTopBorders.y) miniGameMapPosition.x = leftRightBottomTopBorders.y;

        if (miniGameMapPosition.z < leftRightBottomTopBorders.z) miniGameMapPosition.z = leftRightBottomTopBorders.z;
        else if (miniGameMapPosition.z > leftRightBottomTopBorders.w) miniGameMapPosition.z = leftRightBottomTopBorders.w;

        miniGameMap.localPosition = miniGameMapPosition;
    }

    public override void Reset()
    {
        // Should not be reset
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }

    protected override void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.mapSwipeManager.Disable();
        StopExecutingMiniGame();
        base.ExitMiniGame(victory);
    }

    public void Initialize()
    {
        initMapPos = miniGameMap.position;
        sizeRatio = map.localScale.x / ShipManager.instance.realMap.localScale.x;
        posY = initMapPos.y;
        SetRelativeMapPosition();
        
        OnGetStar += GetStar;

        /*
        shipPath.positionCount = 2;
        shipPath.SetPosition(0, ship.transform.position);

        OnShipRotationChange += AddPointOnPath;
        */
    }

    public void Refresh()
    {
        //SetRelativeMapPosition();
        SetRelativeBoatPosition();
        ship.eulerAngles = ShipManager.instance.GetShipAngle() * Vector3.up;
    }

    private void SetRelativeMapPosition()
    {
        miniGameMapPosition = -ShipManager.instance.GetShipPositionOnMap() * sizeRatio;
        miniGameMapPosition.z = miniGameMapPosition.y;
        miniGameMapPosition.y = posY;

        miniGameMap.localPosition = miniGameMapPosition;
    }

    private void SetRelativeBoatPosition()
    {
        shipPosition = ShipManager.instance.GetShipPositionOnMap() * sizeRatio;
        shipPosition.z = shipPosition.y;
        shipPosition.y = 0.01f; // We can hardcode Y value as it must be really close to 0

        ship.localPosition = shipPosition;
    }

    #region Callbacks

    public static Action OnShipRotationChange;
    public static Action<byte> OnGetStar;

    private void AddPointOnPath()
    {
        shipPath.SetPosition(shipPath.positionCount - 1, ship.transform.position);
        shipPath.positionCount++;
        shipPath.SetPosition(shipPath.positionCount - 1, ship.transform.position);
    }

    private void GetStar(byte index)
    {
        GetStarServerRpc(index);
    }

    #endregion

    #region Network

    [ServerRpc]
    private void GetStarServerRpc(byte index)
    {
        GetStarClientRpc(index);
    }

    [ClientRpc]
    private void GetStarClientRpc(byte index)
    {
        // Feedback get star
        stars[index].color = Color.red;
    }

    #endregion
}