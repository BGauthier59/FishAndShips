using System;
using Unity.Netcode;
using UnityEngine;

public class MiniGame_Map : MiniGame
{
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

    public override void StartMiniGame()
    {
        base.StartMiniGame();
    }

    public override void ExecuteMiniGame()
    {
    }

    public override void Reset()
    {
        // Should not be reset
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }


    public void Initialize()
    {
        initMapPos = miniGameMap.position;
        sizeRatio = map.localScale.x / ShipManager.instance.realMap.localScale.x;
        posY = initMapPos.y;
        SetRelativeMapPosition();
        
        /*
        shipPath.positionCount = 2;
        shipPath.SetPosition(0, ship.transform.position);

        OnShipRotationChange += AddPointOnPath;
        OnGetStar += GetStar;
        */
    }

    public void Refresh()
    {
        SetRelativeMapPosition();
        ship.eulerAngles = ShipManager.instance.GetShipAngle() * Vector3.up;
    }

    private void SetRelativeMapPosition()
    {
        miniGameMapPosition = -ShipManager.instance.GetShipPositionOnMap() * sizeRatio;
        miniGameMapPosition.z = miniGameMapPosition.y;
        miniGameMapPosition.y = posY;

        miniGameMap.localPosition = miniGameMapPosition;

        // Todo - Set map position instead of boat position
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