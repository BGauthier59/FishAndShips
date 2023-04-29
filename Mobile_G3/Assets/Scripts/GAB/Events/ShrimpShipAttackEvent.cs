using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrimpShipAttackEvent : RandomEvent
{
    #region Variables

    #region Ship Behaviour

    [SerializeField] private PosRot cameraPos;
    [SerializeField] private int totalLife;
    private int currentLife;

    #endregion

    #region Ship Displacement

    [SerializeField] private float moveDuration;

    [SerializeField] [Tooltip("WARNING! Don't edit this curve alone please.")]
    private AnimationCurve moveLook;

    [SerializeField] private float baseStationaryDuration;
    private float currentStationaryDuration;
    private float stationaryTimer;
    private bool isMovingToNextPoint;
    [SerializeField] private float randomStationaryDurationGap;

    [SerializeField] private Transform point1;
    [SerializeField] private Transform point2;
    private Transform currentPoint;
    private float distanceBetweenPoints;

    [SerializeField] private Transform shrimpShip;

    #endregion

    #region Fire Management

    [SerializeField] private float baseFireCooldownDuration;
    [SerializeField] private float randomFireCooldownDurationGap;
    private float fireCooldownTimer;
    private float currentFireCooldownDuration;
    [SerializeField] private float fireAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform cannonOrigin;
    [SerializeField] private Transform bullet;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;

    #endregion

    #region Shrimps Management

    [SerializeField] private float baseShrimpSpawnCooldownDuration;
    [SerializeField] private float randomShrimpSpawnCooldownDurationGap;
    private float currentShrimpSpawnCooldownDuration;
    private float currentShrimpSpawnCooldownTimer;
    private bool isSpawningShrimp;

    [SerializeField] private Transform spawnShrimpOrigin;
    [SerializeField] private float spawnDuration;
    [SerializeField] private Transform spawningShrimp;
    [SerializeField] private Transform initShrimpParent;
    
    #endregion

    public TMP_Text DEBUG_ShipLife;

    #endregion

    #region Main Methods

    public override bool CheckConditions()
    {
        // Can't run if ship is under attack
        if (ShipManager.instance.IsUnderAttack()) return false;

        // Is cooldown over
        if (!EventsManager.instance.IsShrimpShipCooldownOver()) return false;

        // Todo - Check distance between attacks

        return true;
    }

    private void Start()
    {
        //shipPosition.OnValueChanged += SyncCurrentShipPos;
    }

    public override void StartEvent()
    {
        // Todo - Play feedback to every client, but do logic on host only!
        StartEventFeedbackClientRpc();

        // Logic for Host
        base.StartEvent();
        ShipManager.instance.SetUnderAttack(true);
        //shipPosition.Value = point1.position;
        currentPoint = point1;
        currentLife = totalLife;
        SetNewStationaryDuration();
        SetNewFireCooldownDuration();
        SetNewShrimpSpawnCooldownDuration();

        // for test only
        DEBUG_ShipLife.text = $"{currentLife}/{totalLife}";
    }

    [ClientRpc]
    private void StartEventFeedbackClientRpc()
    {
        SetYPos();
        shrimpShip.position = point1.position;
        shrimpShip.gameObject.SetActive(true);

        CameraManager.instance.SetCurrentDeckCameraPosRot(cameraPos.pos, cameraPos.rot);
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
    }

    public override void ExecuteEvent()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        // Instantie workshop : crevettes
        CheckStationaryTimer();
        CheckShrimpSpawnTimer();
        CheckFireTimer();
    }

    protected override void EndEvent()
    {
        // Todo - Play feedback to every client, but do logic on host only!
        base.EndEvent();
        EndEventFeedbackClientRpc();
        EventsManager.instance.StartShrimpShipCooldown();
        ShipManager.instance.SetUnderAttack(false);
    }

    [ClientRpc]
    private void EndEventFeedbackClientRpc()
    {
        EndEventFeedback();
    }

    private async void EndEventFeedback()
    {
        // Todo - Implement destruction animation

        await Task.Delay(1000);

        shrimpShip.gameObject.SetActive(false);
        CameraManager.instance.ResetDeckPosRot();
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
    }

    #endregion

    #region Initialization

    private void SetYPos()
    {
        var pos1 = point1.position;
        var pos2 = point2.position;

        pos1.y = pos2.y = shrimpShip.position.y;
        point1.position = pos1;
        point2.position = pos2;
    }

    #endregion

    #region Displacement

    private void SetNewStationaryDuration()
    {
        currentStationaryDuration = baseStationaryDuration +
                                    Random.Range(-randomStationaryDurationGap, randomStationaryDurationGap);
        stationaryTimer = 0;
        isMovingToNextPoint = false;
    }

    private void CheckStationaryTimer()
    {
        if (isMovingToNextPoint) return;

        if (stationaryTimer >= currentStationaryDuration)
        {
            MoveToOtherPoint();
        }
        else stationaryTimer += Time.deltaTime;
    }

    private async void MoveToOtherPoint()
    {
        stationaryTimer = 0;
        isMovingToNextPoint = true;

        var nextPoint = currentPoint == point1 ? point2 : point1;

        MoveToOtherPointClientRpc(currentPoint.position, nextPoint.position);

        await Task.Delay((int) (moveDuration * 1000));
        currentPoint = nextPoint;

        SetNewStationaryDuration();
    }

    [ClientRpc]
    private void MoveToOtherPointClientRpc(Vector3 startPoint, Vector3 endPoint)
    {
        Move(startPoint, endPoint);
    }

    private async void Move(Vector3 start, Vector3 end)
    {
        var timer = 0f;
        float ratio;
        Vector3 nextPos;
        while (timer < moveDuration)
        {
            await Task.Yield();
            timer += Time.deltaTime;
            ratio = timer / moveDuration;
            nextPos = Vector3.Lerp(start, end, ratio);
            //nextPos.x *= moveLook.Evaluate(ratio);
            shrimpShip.position = nextPos;
        }

        shrimpShip.position = end;
    }

    #endregion

    #region Fire

    private void SetNewFireCooldownDuration()
    {
        currentFireCooldownDuration = baseFireCooldownDuration +
                                      Random.Range(-randomFireCooldownDurationGap, randomFireCooldownDurationGap);
        fireCooldownTimer = 0;
        isFiring = false;
    }

    private void CheckFireTimer()
    {
        if (isFiring) return;

        if (fireCooldownTimer > currentFireCooldownDuration)
        {
            Fire();
        }
        else fireCooldownTimer += Time.deltaTime;
    }

    private async void Fire()
    {
        isFiring = true;

        // Select a tile that is alright
        Tile targetedTile = GetAvailableTile();

        if (targetedTile == null)
        {
            SetNewFireCooldownDuration();
            return;
        }

        Vector3 p1, p2, p3, p4;
        p1 = cannonOrigin.position;
        p2 = p1 + Vector3.up * controlPoint1Height;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * controlPoint2Height;

        FireFeedbackClientRpc(p1, p2, p3, p4);

        await Task.Delay((int) (fireAnimationDuration * 1000));

        // Todo - Spawn workshop here

        SetNewFireCooldownDuration();
    }

    [ClientRpc]
    private void FireFeedbackClientRpc(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        CannonBulletAnimation(p1, p2, p3, p4);
    }

    private async void CannonBulletAnimation(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Todo - Implement VFX, screen shake...

        bullet.position = p1;
        bullet.gameObject.SetActive(true);

        var timer = 0f;
        while (timer < fireAnimationDuration)
        {
            await Task.Yield();
            timer += Time.deltaTime;
            bullet.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / fireAnimationDuration);
        }

        bullet.gameObject.SetActive(false);
    }

    #endregion

    #region InstantiateShrimp

    private void SetNewShrimpSpawnCooldownDuration()
    {
        currentShrimpSpawnCooldownDuration = baseShrimpSpawnCooldownDuration +
                                             Random.Range(-randomShrimpSpawnCooldownDurationGap,
                                                 randomShrimpSpawnCooldownDurationGap);
        currentShrimpSpawnCooldownTimer = 0;
        isSpawningShrimp = false;
    }

    private void CheckShrimpSpawnTimer()
    {
        if (isSpawningShrimp) return;

        if (currentShrimpSpawnCooldownTimer > currentShrimpSpawnCooldownDuration)
        {
            TrySpawnShrimpWorkshop();
        }
        else currentShrimpSpawnCooldownTimer += Time.deltaTime;
    }

    private async void TrySpawnShrimpWorkshop()
    {
        if (!EventsManager.instance.CanInstantiateShrimpWorkshop())
        {
            Debug.Log("Can't instantiate more shrimps.");
            SetNewShrimpSpawnCooldownDuration();
            return;
        }

        Tile targetedTile = GetAvailableTile();
        if (targetedTile == null)
        {
            Debug.Log("Couldn't spawn shrimp");
            SetNewShrimpSpawnCooldownDuration();
            return;
        }
        
        EventsManager.instance.AddShrimp();
        int? index = EventsManager.instance.GetShrimpWorkshopIndex();
        if (!index.HasValue)
        {
            Debug.LogError("Should not happen. There's no workshop available but still tried to instantiate one.");
            return;
        }
        Debug.Log($"index is {index}");
        isSpawningShrimp = true;

        Vector3 p1, p2, p3, p4;
        p1 = spawnShrimpOrigin.position;
        p2 = p1 + Vector3.up * 1;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * 1;

        int2 coord = targetedTile.GetTilePos();
        SpawnShrimpClientRpc(p1, p2, p3, p4, coord.x, coord.y, index.Value);

        await Task.Delay((int) (spawnDuration * 1000));
        
        SetNewShrimpSpawnCooldownDuration();
    }

    [ClientRpc]
    private void SpawnShrimpClientRpc(Vector3 p1, Vector3 p2, Vector3 p3, Vector4 p4, int coordX, int coordY, int index)
    {
        SpawnShrimp(p1, p2, p3, p4, coordX, coordY, index);
    }

    private async void SpawnShrimp(Vector3 p1, Vector3 p2, Vector3 p3, Vector4 p4, int coordX, int coordY, int index)
    {
        spawningShrimp.position = p1;
        spawningShrimp.SetParent(null);
        spawningShrimp.gameObject.SetActive(true);

        var timer = 0f;
        while (timer < spawnDuration)
        {
            await Task.Yield();
            timer += Time.deltaTime;
            spawningShrimp.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / spawnDuration);
        }

        spawningShrimp.gameObject.SetActive(false);
        spawningShrimp.SetParent(initShrimpParent);

        Tile targetedTile = GridManager.instance.GetTile(coordX, coordY);

        ShrimpWorkshop shrimpWorkshop = EventsManager.instance.GetShrimpWorkshop(index);
        targetedTile.SetTile(shrimpWorkshop, targetedTile.GetFloor());
        shrimpWorkshop.SetPosition(coordX, coordY);
        if (Unity.Netcode.NetworkManager.Singleton.IsHost) shrimpWorkshop.ActivateServerRpc();
    }

    private Tile GetAvailableTile()
    {
        Tile targetedTile;
        int randomX, randomY;
        int securityCount = 0;

        do
        {
            randomX = Random.Range(0, GridManager.instance.xSize);
            randomY = Random.Range(0, GridManager.instance.ySize);
            targetedTile = GridManager.instance.GetTile(randomX, randomY);

            securityCount++;
            if (securityCount == 100)
            {
                Debug.LogWarning("Didn't find any tile after trying 100 times. Returns null.");
                return null;
            }
        } while (targetedTile == null || targetedTile.GetEntity() != null ||
                 targetedTile.GetFloor() is not GridFloorWalkable);

        return targetedTile;
    }

    #endregion

    #region Life Management

    [ContextMenu("Get hit")]
    public void GetHit()
    {
        currentLife--;
        DEBUG_ShipLife.text = $"{currentLife}/{totalLife}";

        if (currentLife <= 0)
        {
            EndEvent();
        }
        else
        {
            if (!isMovingToNextPoint) MoveToOtherPoint();
        }
    }

    #endregion
}