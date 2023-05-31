using System;
using Cysharp.Threading.Tasks;
using TMPro;
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

    [SerializeField] private Transform[] points;
    [SerializeField] private Vector4 bordersLeftRightBottomTop;

    [Serializable]
    public class Island
    {
        public Transform transform;
        public TMP_Text text;
        public string name;
    }

    public override async void StartMiniGame()
    {
        await SetupIslandAndRotation();

        base.StartMiniGame();
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        WorkshopManager.instance.StartMiniGameTutorial(5);
        WorkshopManager.instance.mapSwipeManager.Enable(data);
        StartExecutingMiniGame();
    }

    private Vector3 delta;

    public override void ExecuteMiniGame()
    {
        // Can move the map
        delta = WorkshopManager.instance.mapSwipeManager.CalculateMoveDelta();

        if (delta != Vector3.zero) WorkshopManager.instance.StopMiniGameTutorial();

        miniGameMapPosition += delta;

        if (miniGameMapPosition.x < leftRightBottomTopBorders.x) miniGameMapPosition.x = leftRightBottomTopBorders.x;
        else if (miniGameMapPosition.x > leftRightBottomTopBorders.y)
            miniGameMapPosition.x = leftRightBottomTopBorders.y;

        if (miniGameMapPosition.z < leftRightBottomTopBorders.z) miniGameMapPosition.z = leftRightBottomTopBorders.z;
        else if (miniGameMapPosition.z > leftRightBottomTopBorders.w)
            miniGameMapPosition.z = leftRightBottomTopBorders.w;

        miniGameMapPosition.y = posY;
        miniGameMap.localPosition = miniGameMapPosition;
        UpdateIslandTextColors();

        RotateShip();
        if (IsRotationAlright()) SetCorrectRotation();
    }

    private Color white = Color.white;
    private Color whiteZeroAlpha = new Color(1, 1, 1, 0);

    private void UpdateIslandTextColors()
    {
        foreach (var island in islands)
        {
            island.text.color = Color.Lerp(island.text.color,
                IsIslandTextColorInVisibleArea(island.text.transform.position) ? 
                    white : whiteZeroAlpha,
                Time.deltaTime * 5f);
        }
    }

    private bool IsIslandTextColorInVisibleArea(Vector3 position)
    {
        if (position.x < bordersLeftRightBottomTop.x || position.x > bordersLeftRightBottomTop.y ||
            position.z < bordersLeftRightBottomTop.z || position.z > bordersLeftRightBottomTop.w) return false;
        return true;
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
        isRight = false;
        ship.eulerAngles = Vector3.zero;
        rightDirection = Vector3.zero;
        currentRotationPerSecond = 0;
    }

    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        WorkshopManager.instance.mapSwipeManager.Disable();
        StopExecutingMiniGame();
        ExitMiniGame(false);
    }

    private async UniTask SetupIslandAndRotation()
    {
        // WARNING! This operation can be very long?
        currentIsland = islands[Random.Range(0, islands.Length)];

        posY = miniGameMap.localPosition.y;
        bool[] availablePoints = new bool[points.Length];

        Vector3 pos;
        int random;
        foreach (var t in islands)
        {
            do
            {
                random = Random.Range(0, availablePoints.Length);
                await UniTask.Yield();
                if (SceneLoaderManager.instance.CancelTaskInGame()) return;
            } while (availablePoints[random]);

            availablePoints[random] = true;
            pos = points[random].position;
            pos.y = posY;
            t.transform.position = pos;
        }

        rightDirection = (currentIsland.transform.position - ship.position);

        rudderMiniGame.InitiateStartOfGameServerRpc(GetOtherPlayerId(), currentIsland.name);
    }

    private async void SetCorrectRotation()
    {
        rudderMiniGame.InitiateEndOfGameServerRpc(GetOtherPlayerId());

        StopExecutingMiniGame();
        WorkshopManager.instance.mapSwipeManager.Disable();
        WorkshopManager.instance.SetVictoryIndicator();
        HonorificManager.instance.AddHonorific(Honorifics.Explorer, Honorifics.TeamSpirit);

        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

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