using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ShrimpShipAttackEvent : RandomEvent
{
    #region Variables

    #region Ship Behaviour

    [SerializeField] private PosRot cameraPos;

    #endregion

    [SerializeField] private Transform ship;

    [SerializeField] private int2 shrimpsMinMaxCount, reparationMinMaxCount;

    private int shrimpsCount, reparationCount, totalCount, count;

    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private float moveDuration;

    #region Fire Management

    [SerializeField] private float fireAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform cannonOrigin;
    [SerializeField] private Transform bullet;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;

    #endregion

    #region Shrimps Management

    private bool isSpawningShrimp;

    [SerializeField] private Transform spawnShrimpOrigin;
    [SerializeField] private float spawnDuration;
    [SerializeField] private Transform spawningShrimp;
    [SerializeField] private Animation spawningShrimpAnim;
    [SerializeField] private AnimationClip spawningShrimpJump;
    [SerializeField] private Transform initShrimpParent;

    #endregion
    
    [SerializeField] private int2[] shrimpTargetTiles;
    [SerializeField] private int2[] holeTargetTiles;

    [Header("Feedbacks")] [SerializeField] private UnityEvent fireEvent;
    [SerializeField] private Transform cannonShootTargetedTile, shrimpTargetedTile;
    [SerializeField] private Animation mortarAnim;

    #endregion

    #region Main Methods

    public override bool CheckConditions()
    {
        if (!EventsManager.instance.CanInstantiateHole() && !EventsManager.instance.CanInstantiateHole())
            return false;

        return true;
    }

    public override async void StartEvent()
    {
        StartShrimpShipEventFeedbackClientRpc();

        // Logic for Host
        SetupWorkshops();

        await MoveToWayPoint(wayPoints[0].position, wayPoints[1].position);
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        GenerateReparationWorkshops();
        GenerateShrimpWorkshops();

        await WaitForEveryWorkshopInstantiation();
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        EndEvent();
    }

    private void SetupWorkshops()
    {
        shrimpsCount = Random.Range(shrimpsMinMaxCount.x, shrimpsMinMaxCount.y);
        reparationCount = Random.Range(reparationMinMaxCount.x, reparationMinMaxCount.y);
        totalCount = shrimpsCount + reparationCount;
        count = 0;
    }

    private async void GenerateReparationWorkshops()
    {
        int randomCooldown;
        for (int i = 0; i < reparationCount; i++)
        {
            randomCooldown = Random.Range(300, 701);
            await UniTask.Delay(randomCooldown);
            if (await Fire()) continue;
            break;
        }

        count += reparationCount;
    }

    private async void GenerateShrimpWorkshops()
    {
        int randomCooldown;
        for (int i = 0; i < shrimpsCount; i++)
        {
            randomCooldown = Random.Range(300, 701);
            await UniTask.Delay(randomCooldown);
            if (await ComeAlongside()) continue;
            break;
        }

        count += shrimpsCount;
    }

    private async UniTask WaitForEveryWorkshopInstantiation()
    {
        while (count != totalCount) await UniTask.Yield();
    }

    [ClientRpc]
    private void StartShrimpShipEventFeedbackClientRpc()
    {
        ship.gameObject.SetActive(true);

        cannonShootTargetedTile.SetParent(null);
        shrimpTargetedTile.SetParent(null);

        CameraManager.instance.SetCurrentDeckCameraPosRot(cameraPos.pos, cameraPos.rot);
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);

        if (!NetworkManager.Singleton.IsHost) MoveToWayPoint(wayPoints[0].position, wayPoints[1].position);
    }

    private async UniTask MoveToWayPoint(Vector3 start, Vector3 end)
    {
        float timer = 0;
        ship.position = start;

        while (timer < moveDuration)
        {
            ship.position = Ex.CubicBezierCurve(start, start, end, end, timer / moveDuration);
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            timer += Time.deltaTime;
        }

        ship.position = end;
    }

    protected override async void EndEvent()
    {
        EndShrimpShipEventFeedbackClientRpc();

        await MoveToWayPoint(wayPoints[1].position, wayPoints[2].position);
    }
    

    [ClientRpc]
    private void EndShrimpShipEventFeedbackClientRpc()
    {
        EndEventFeedback();
    }

    private async void EndEventFeedback()
    {
        if (!NetworkManager.Singleton.IsHost) await MoveToWayPoint(wayPoints[1].position, wayPoints[2].position);
        else await UniTask.Delay((int) (moveDuration * 1000));
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        ship.gameObject.SetActive(false);
        CameraManager.instance.ResetDeckPosRot();
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
    }

    #endregion

    #region Fire

    private async UniTask<bool> Fire()
    {
        if (!EventsManager.instance.CanInstantiateHole())
        {
            Debug.Log("Couldn't spawn reparation");
            return false;
        }

        // Select a tile that is alright
        Tile targetedTile = GridManager.instance.GetRandomTile(holeTargetTiles);
        if (targetedTile == null)
        {
            Debug.Log("Couldn't spawn reparation");
            return false;
        }

        int? index = EventsManager.instance.GetReparationWorkshopIndex();
        if (!index.HasValue)
        {
            Debug.LogError("Should not happen. There's no workshop available but still tried to instantiate one.");
            return false;
        }

        isFiring = true;

        Vector3 p1, p2, p3, p4;
        p1 = cannonOrigin.position;
        p2 = p1 + Vector3.up * controlPoint1Height;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * controlPoint2Height;

        int2 coord = targetedTile.GetTilePos();
        FireFeedbackClientRpc(p1, p2, p3, p4, coord.x, coord.y, index.Value);

        await UniTask.Delay((int) (fireAnimationDuration * 1000));
        if (SceneLoaderManager.instance.CancelTaskInGame()) return false;

        return true;
    }

    [ClientRpc]
    private void FireFeedbackClientRpc(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int x, int y, int index)
    {
        Fire(p1, p2, p3, p4, x, y, index);
    }

    private async void Fire(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int x, int y, int index)
    {
        fireEvent?.Invoke();

        cannonShootTargetedTile.gameObject.SetActive(true);
        var targetedTile = GridManager.instance.GetTile(x, y);
        var walkable = targetedTile.GetFloor();
        targetedTile.SetTile(targetedTile.GetEntity(), EventsManager.instance.GetNotWalkable());

        cannonShootTargetedTile.position = p4;

        bullet.position = p1;
        bullet.gameObject.SetActive(true);

        var timer = 0f;
        while (timer < fireAnimationDuration)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            timer += Time.deltaTime;
            bullet.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / fireAnimationDuration);
        }

        bullet.gameObject.SetActive(false);
        cannonShootTargetedTile.gameObject.SetActive(false);

        ReparationWorkshop reparationWorkshop = EventsManager.instance.GetReparationWorkshop(index);
        
        reparationWorkshop.PlayExplosionEffect();
        targetedTile.SetTile(targetedTile.GetEntity(), walkable);
        reparationWorkshop.SetPosition(x, y);
        if (NetworkManager.Singleton.IsHost) reparationWorkshop.ActivateServerRpc();
    }

    public void PlayMortarAnim()
    {
        mortarAnim.Play(mortarAnim.clip.name);
    }

    #endregion

    #region InstantiateShrimp

    private async UniTask<bool> ComeAlongside()
    {
        if (!EventsManager.instance.CanInstantiateShrimpWorkshop())
        {
            Debug.Log("Couldn't spawn shrimp");
            return false;
        }

        Tile targetedTile = GridManager.instance.GetRandomTile(shrimpTargetTiles);
        if (targetedTile == null)
        {
            Debug.Log("Couldn't spawn shrimp");
            return false;
        }

        int? index = EventsManager.instance.GetShrimpWorkshopIndex();
        if (!index.HasValue)
        {
            Debug.LogError("Should not happen. There's no workshop available but still tried to instantiate one.");
            return false;
        }

        isSpawningShrimp = true;

        Vector3 p1, p2, p3, p4;
        p1 = spawnShrimpOrigin.position;
        p2 = p1 + Vector3.up * 1;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * 1;

        int2 coord = targetedTile.GetTilePos();
        SpawnShrimpClientRpc(p1, p2, p3, p4, coord.x, coord.y, index.Value);

        await UniTask.Delay((int) (spawnDuration * 1000));
        if (SceneLoaderManager.instance.CancelTaskInGame()) return false;

        return true;
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

        shrimpTargetedTile.gameObject.SetActive(true);
        shrimpTargetedTile.position = p4;
        Tile targetedTile = GridManager.instance.GetTile(coordX, coordY);
        var walkable = targetedTile.GetFloor();
        targetedTile.SetTile(targetedTile.GetEntity(), EventsManager.instance.GetNotWalkable());

        spawningShrimpAnim.Play(spawningShrimpJump.name);

        var timer = 0f;
        while (timer < spawnDuration)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            timer += Time.deltaTime;
            spawningShrimp.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / spawnDuration);
        }

        spawningShrimp.gameObject.SetActive(false);
        spawningShrimp.SetParent(initShrimpParent);

        shrimpTargetedTile.gameObject.SetActive(false);

        ShrimpWorkshop shrimpWorkshop = EventsManager.instance.GetShrimpWorkshop(index);
        targetedTile.SetTile(targetedTile.GetEntity(), walkable);
        shrimpWorkshop.SetPosition(coordX, coordY);
        if (NetworkManager.Singleton.IsHost) shrimpWorkshop.ActivateServerRpc();
    }

    #endregion
}