using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FireSoftEvent : RandomEvent
{
    [SerializeField] private float sparkAnimationDuration;
    private bool isFiring;

    [Header("Feedbacks")] [SerializeField] private UnityEvent sparkEvent;
    [SerializeField] private Transform sparkOrigin;
    [SerializeField] private Transform spark;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;
    [SerializeField] private int2[] fireTargetTiles;
    [SerializeField] private Transform sparkTargetedTile;

    public override bool CheckConditions()
    {
        if (EventsManager.instance.GetFireIndices().Length == 0) return false;
        return true;
    }

    public override async void StartEvent()
    {
        if (await CastSpark()) Debug.Log("Success");
        else Debug.Log("Failure?");
        EndEvent();
    }

    protected override void EndEvent()
    {
        
    }
    
    private async UniTask<bool> CastSpark()
    {
        if (!EventsManager.instance.CanInstantiateHole())
        {
            Debug.Log("Couldn't spawn reparation");
            return false;
        }

        // Select a tile that is alright
        Tile targetedTile = GridManager.instance.GetRandomTile(fireTargetTiles);
        if (targetedTile == null)
        {
            Debug.Log("Couldn't spawn reparation");
            return false;
        }

        int[] indices = EventsManager.instance.GetFireIndices();
        if (indices.Length == 0)
        {
            Debug.LogError("Should not happen. There's no workshop available but still tried to instantiate one.");
            return false;
        }

        int index = indices[Random.Range(0, indices.Length)];

        isFiring = true;

        Vector3 p1, p2, p3, p4;
        p1 = sparkOrigin.position;
        p2 = p1 + Vector3.up * controlPoint1Height;
        p4 = targetedTile.transform.position + Vector3.up * .1f;
        p3 = p4 + Vector3.up * controlPoint2Height;

        int2 coord = targetedTile.GetTilePos();
        FireFeedbackClientRpc(p1, p2, p3, p4, coord.x, coord.y, index);

        await UniTask.Delay((int) (sparkAnimationDuration * 1000));
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
        sparkEvent?.Invoke();

        sparkTargetedTile.gameObject.SetActive(true);
        var targetedTile = GridManager.instance.GetTile(x, y);
        var walkable = targetedTile.GetFloor();
        targetedTile.SetTile(targetedTile.GetEntity(), EventsManager.instance.GetNotWalkable());

        sparkTargetedTile.position = p4;

        spark.position = p1;
        spark.gameObject.SetActive(true);

        var timer = 0f;
        while (timer < sparkAnimationDuration)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            timer += Time.deltaTime;
            spark.position = Ex.CubicBezierCurve(p1, p2, p3, p4, timer / sparkAnimationDuration);
        }

        spark.gameObject.SetActive(false);
        sparkTargetedTile.gameObject.SetActive(false);

        Workshop fireWorkshop = EventsManager.instance.GetFireWorkshop(index);
        
        targetedTile.SetTile(targetedTile.GetEntity(), walkable);
        fireWorkshop.SetPosition(x, y);
        if (NetworkManager.Singleton.IsHost) fireWorkshop.ActivateServerRpc();
    }
}
