using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Map : MiniGame
{
    [SerializeField] private MapSwipeData data;
    [SerializeField] private MiniGame_Rudder rudderMiniGame;

    [SerializeField] private Transform ship;
    [SerializeField] private Transform miniGameMap;
    private Vector3 shipPosition;
    private Vector3 miniGameMapPosition;
    private float posY;
    private Vector3 initMapPos;

    private float currentRotationPerSecond;
    private Vector3 rightDirection;

    [SerializeField] private float validationDuration;
    private float timer;
    [SerializeField] private float collinearFactorThreshold;

    [SerializeField] private Vector4 leftRightBottomTopBorders;
    [SerializeField] private Island[] islands;
    private Island currentIsland;
    [SerializeField] private float spawnRadius;

    [Serializable]
    public struct Island
    {
        public Transform transform;
        public string name;
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        SetupIslandAndRotation();

        await Task.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        WorkshopManager.instance.mapSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    public override void ExecuteMiniGame()
    {
        // Can move the map
        miniGameMapPosition += WorkshopManager.instance.mapSwipeManager.CalculateMoveDelta();

        if (miniGameMapPosition.x < leftRightBottomTopBorders.x) miniGameMapPosition.x = leftRightBottomTopBorders.x;
        else if (miniGameMapPosition.x > leftRightBottomTopBorders.y)
            miniGameMapPosition.x = leftRightBottomTopBorders.y;

        if (miniGameMapPosition.z < leftRightBottomTopBorders.z) miniGameMapPosition.z = leftRightBottomTopBorders.z;
        else if (miniGameMapPosition.z > leftRightBottomTopBorders.w)
            miniGameMapPosition.z = leftRightBottomTopBorders.w;

        miniGameMapPosition.y = posY;
        miniGameMap.localPosition = miniGameMapPosition;

        RotateShip();
        if (IsRotationAlright()) SetCorrectRotation();
    }

    private void RotateShip()
    {
        ship.eulerAngles += Vector3.up * currentRotationPerSecond * 2 * Time.deltaTime;
    }

    private bool isRight;
    private bool IsRotationAlright()
    {
        if (Vector3.Dot(ship.right, rightDirection) > collinearFactorThreshold)
        {
            if (!isRight)
            {
                timer = 0;
                isRight = true;
            }
            if (timer >= validationDuration) return true;
            timer += Time.deltaTime;
            return false;
        }

        isRight = false;
        return false;
    }

    public override void Reset()
    {
        // Should not be reset
    }

    public override void OnLeaveMiniGame()
    {
        WorkshopManager.instance.mapSwipeManager.Disable();
        StopExecutingMiniGame();
        ExitMiniGame(false);
    }

    private void SetupIslandAndRotation()
    {
        posY = miniGameMap.localPosition.y;

        currentIsland = islands[Random.Range(0, islands.Length)];
        Vector3 pos;
        foreach (var island in islands)
        {
            pos = ship.position + Random.insideUnitSphere * spawnRadius;
            pos.y = ship.position.y;
            island.transform.position = pos;
        }

        rightDirection = (currentIsland.transform.position - ship.position).normalized;

        rudderMiniGame.InitiateStartOfGameServerRpc(GetOtherPlayerId(), currentIsland.name);
    }

    private async void SetCorrectRotation()
    {
        rudderMiniGame.InitiateEndOfGameServerRpc(GetOtherPlayerId());

        StopExecutingMiniGame();
        WorkshopManager.instance.mapSwipeManager.Disable();
        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());

        ExitMiniGame(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateShipRotationServerRpc(float rotation, ulong id)
    {
        var parameters = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {id}
            }
        };

        UpdateShipRotationClientRpc(rotation, parameters);
    }

    [ClientRpc]
    private void UpdateShipRotationClientRpc(float rotation, ClientRpcParams parameter)
    {
        currentRotationPerSecond = rotation;
    }

    private ulong GetOtherPlayerId()
    {
        return ((ConnectedWorkshop) WorkshopManager.instance.GetCurrentWorkshop()).GetOtherPlayerId();
    }
}