using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TutorialHoleSoftEvent : RandomEvent
{
    [SerializeField] private float fireAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform cannonOrigin;
    [SerializeField] private Transform bullet;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;

    [Header("Feedbacks")] [SerializeField] private UnityEvent fireEvent;
    [SerializeField] private Transform cannonShootTargetedTile;
    
    [SerializeField] private int2[] spawnTiles, otherSpawnTiles;
    
    public override bool CheckConditions()
    {
        return true;
    }

    public override async void StartEvent()
    {
        // Activate hole one at the time

        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Host is managing event.");
            return;
        }
        
        if (!await Fire()) Debug.Log("Failure?!");
        else Debug.Log("Success!");
    }

    protected override void EndEvent()
    {
    }

    private async UniTask<bool> Fire()
    {
        for (int i = 0; i < TutorialEventManager.instance.playerNb; i++)
        {
            Tile targetedTile = GridManager.instance.GetTile(spawnTiles[i].x, spawnTiles[i].y);
            if (targetedTile.GetEntity() != null)
                targetedTile = GridManager.instance.GetTile(otherSpawnTiles[i].x, otherSpawnTiles[i].y);

            int index = i;

            isFiring = true;

            Vector3 p1, p2, p3, p4;
            p1 = cannonOrigin.position;
            p2 = p1 + Vector3.up * controlPoint1Height;
            p4 = targetedTile.transform.position;
            p3 = p4 + Vector3.up * controlPoint2Height;

            int2 coord = targetedTile.GetTilePos();
            FireFeedbackClientRpc(p1, p2, p3, p4, coord.x, coord.y, index);

            await UniTask.Delay((int) (fireAnimationDuration * 1000));
        }

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
        targetedTile.SetTile(targetedTile.GetEntity(), TutorialEventManager.instance.GetNotWalkable());

        cannonShootTargetedTile.position = p4;

        bullet.position = p1;
        bullet.gameObject.SetActive(true);

        var timer = 0f;
        while (timer < fireAnimationDuration)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
            bullet.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / fireAnimationDuration);
        }

        bullet.gameObject.SetActive(false);
        cannonShootTargetedTile.gameObject.SetActive(false);

        ReparationWorkshop reparationWorkshop = TutorialEventManager.instance.GetReparationWorkshop(index);

        reparationWorkshop.PlayExplosionEffect();
        targetedTile.SetTile(targetedTile.GetEntity(), walkable);
        reparationWorkshop.SetPosition(x, y);
        if (NetworkManager.Singleton.IsHost) reparationWorkshop.ActivateServerRpc();
    }
}