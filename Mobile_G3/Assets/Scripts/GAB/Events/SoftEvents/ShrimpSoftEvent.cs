using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ShrimpSoftEvent : RandomEvent
{
    private bool isSpawningShrimp;

    [SerializeField] private Transform shrimpTargetedTile;

    [SerializeField] private Transform spawnShrimpOrigin;
    [SerializeField] private float spawnDuration;
    [SerializeField] private Transform spawningShrimp;
    [SerializeField] private Animation spawningShrimpAnim;
    [SerializeField] private AnimationClip spawningShrimpJump;
    [SerializeField] private Transform initShrimpParent;
    [SerializeField] private int2[] shrimpTargetedTiles;

    public override bool CheckConditions()
    {
        return EventsManager.instance.CanInstantiateShrimpWorkshop();
    }

    public override async void StartEvent()
    {
        if (!await ComeAlongside()) Debug.Log("Failure?!");
        else Debug.Log("Success!");
    }

    protected override void EndEvent()
    {
    }

    private async UniTask<bool> ComeAlongside()
    {
        if (!EventsManager.instance.CanInstantiateShrimpWorkshop())
        {
            Debug.Log("Couldn't spawn shrimp");
            return false;
        }

        Tile targetedTile = GridManager.instance.GetRandomTile(shrimpTargetedTiles);
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
        p2 = p1 + Vector3.up * 5;
        p4 = targetedTile.transform.position;
        p3 = p4 + Vector3.up * 1;

        int2 coord = targetedTile.GetTilePos();
        SpawnShrimpClientRpc(p1, p2, p3, p4, coord.x, coord.y, index.Value);

        await UniTask.Delay((int) (spawnDuration * 1000));

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
}