using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrimpShipAttackEvent : RandomEvent
{
    #region Variables

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

    [SerializeField] private float baseFireCooldownDuration;
    [SerializeField] private float randomFireCooldownDurationGap;
    private float fireCooldownTimer;
    private float currentFireCooldownDuration;
    [SerializeField] private float fireAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform cannonOrigin;
    [SerializeField] private Transform bullet;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;

    [SerializeField] private PosRot cameraPos;

    [SerializeField] private int totalLife;
    private int currentLife;

    public TMP_Text DEBUG_ShipLife;

    #endregion

    #region Main Methods

    public override bool CheckConditions()
    {
        Debug.Log(ShipManager.instance.IsUnderAttack());
        Debug.Log(EventsManager.instance.IsShrimpShipCooldownOver());
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
            isFiring = true;
            Fire();
        }
        else fireCooldownTimer += Time.deltaTime;
    }

    [ContextMenu("Fire")]
    private async void Fire()
    {
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

    private void CheckShrimpSpawnTimer()
    {
    }

    private void SpawnShrimpWorkshop()
    {
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