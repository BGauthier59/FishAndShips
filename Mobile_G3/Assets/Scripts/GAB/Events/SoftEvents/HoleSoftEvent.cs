using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class HoleSoftEvent : RandomEvent
{
    [SerializeField] private float fireAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform cannonOrigin;
    [SerializeField] private Transform bullet;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;
    [SerializeField] private int2[] holeTargetTiles;
    [Header("Feedbacks")] [SerializeField] private UnityEvent fireEvent;
    [SerializeField] private Transform cannonShootTargetedTile;
    
    public override bool CheckConditions()
    {
        if (!EventsManager.instance.CanInstantiateHole()) return false;
        return true;
    }

    public override async void StartEvent()
    {
        if (await Fire()) Debug.Log("Success");
        else Debug.Log("Failure?");
        EndEvent();
    }

    protected override void EndEvent()
    {
        
    }
    
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
}
