using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrimpShipAttackEvent : RandomEvent
{
    #region Variables

    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private AnimationCurve moveSpeedLook;
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
    private PosRot cameraInitPos;

    [SerializeField] private int totalLife;
    private int currentLife;

    public TMP_Text DEBUG_ShipLife;

    #endregion
    
    #region Main Methods

    public override bool CheckConditions()
    {
        return false;
    }

    public override void StartEvent()
    {
        // Feedback, camera movement, début de l'event
        SetYPos();

        currentLife = totalLife;
        cameraInitPos.pos = Camera.main.transform.position;
        cameraInitPos.rot = Camera.main.transform.eulerAngles;

        Camera.main.transform.DOMove(cameraPos.pos, 1);
        Camera.main.transform.DORotate(cameraPos.rot, 1);

        // for test only
        DEBUG_ShipLife.text = $"{currentLife}/{totalLife}";
        shrimpShip.position = point1.position;
        currentPoint = point1;
        stationaryTimer = 0;
        isMovingToNextPoint = false;
    }

    public override void ExecuteEvent()
    {
        if (!Unity.Netcode.NetworkManager.Singleton.IsHost) return;
        
        // Instantie workshop : crevettes
        CheckStationaryTimer();
        CheckShrimpSpawnTimer();
        CheckFireTimer();
    }

    public override void EndEvent()
    {
        // Destroyed by 3 cannon bullets

        Camera.main.transform.DOMove(cameraInitPos.pos, 1);
        Camera.main.transform.DORotate(cameraInitPos.rot, 1);
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
        var globalDistance = Vector3.Distance(shrimpShip.position, nextPoint.position);
        var currentDistance = globalDistance;
        var ratio = 1f;
        var dir = -(currentPoint.position - nextPoint.position).normalized;

        while (!HasReachedNextPoint(nextPoint))
        {
            await Task.Yield();

            currentDistance = Vector3.Distance(shrimpShip.position, nextPoint.position);
            ratio = currentDistance / globalDistance;

            shrimpShip.position += dir * (baseMoveSpeed * Time.deltaTime * moveSpeedLook.Evaluate(ratio));
        }

        shrimpShip.position = nextPoint.position;
        currentPoint = nextPoint;

        SetNewStationaryDuration();
    }

    private bool HasReachedNextPoint(Transform point)
    {
        var xPos = point.position.x;
        if (point == point1 && shrimpShip.position.x < xPos) return true;
        if (point == point2 && shrimpShip.position.x > xPos) return true;
        return false;
    }

    #endregion

    #region Fire

    private void SetNewFireCooldownDuration()
    {
        currentFireCooldownDuration = baseFireCooldownDuration +
                                      Random.Range(-randomFireCooldownDurationGap, randomFireCooldownDurationGap);
        isFiring = false;
    }

    private void CheckFireTimer()
    {
        if (isFiring) return;

        if (fireCooldownTimer > currentFireCooldownDuration)
        {
            isFiring = true;
            fireCooldownTimer = 0;
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
            isFiring = false;
            return;
        }

        Debug.Log($"Targeted the tile {targetedTile.name}");

        Vector3 p1, p2, p3, p4;
        p1 = cannonOrigin.position;
        p2 = p1 + Vector3.up * controlPoint1Height;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * controlPoint2Height;

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
        Debug.Log($"Hit the tile {targetedTile.name}");

        SetNewFireCooldownDuration();

        // If no tile found, we give up

        // Placer les points de la bezier curve : les pts doivent être immobiles

        // Animation
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

    public void GetHit()
    {
        currentLife--;
        DEBUG_ShipLife.text = $"{currentLife}/{totalLife}";

        if (currentLife < 0)
        {
            // Todo - If 0, is killed
        }
        else
        {
            if (!isMovingToNextPoint) MoveToOtherPoint();
        }
    }

    #endregion

    [Serializable]
    public struct PosRot
    {
        public Vector3 pos;
        public Vector3 rot;
    }
}