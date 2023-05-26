using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class StormEvent : RandomEvent
{
    [SerializeField] private float stormDuration;
    [SerializeField] private PosRot posRotStorm;
    [SerializeField] private UnityEvent enterStormEvent;
    [SerializeField] private UnityEvent exitStormEvent;
    [SerializeField] private int2 fireMinMaxCount;
    private int fireCount, count;

    [SerializeField] private Volume stormyVolume;

    [SerializeField] private int2[] fireTargetTiles;

    #region Main Methods

    public override bool CheckConditions()
    {
        if (EventsManager.instance.sailsWorkshop != null &&
            EventsManager.instance.sailsWorkshop.isActive.Value) return false;
        if (EventsManager.instance.GetFireIndices().Length == 0) return false;
        return true;
    }

    public override async void StartEvent()
    {
        StartStormEventFeedbackClientRpc();

        // Host-side logic
        SetupEvent();

        await UniTask.Delay(2000); // Pour attendre que le fade se fasse

        GenerateSailsWorkshop();
        GenerateFireWorkshops();

        await UniTask.Delay((int) (stormDuration * 1000));

        EndEvent();
    }

    private void SetupEvent()
    {
        fireCount = Random.Range(fireMinMaxCount.x, fireMinMaxCount.y + 1);
        count = 0;
    }

    private void GenerateSailsWorkshop()
    {
        if (EventsManager.instance.sailsWorkshop == null) return;
        EventsManager.instance.sailsWorkshop.InitializeActivation();
    }

    private async void GenerateFireWorkshops()
    {
        int randomCooldown;
        var indices = EventsManager.instance.GetFireIndices();

        var count = 0;
        foreach (var index in indices)
        {
            if (count == fireCount) break;
            randomCooldown = Random.Range(300, 701);
            await UniTask.Delay(randomCooldown);
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            Tile targetedTile = GridManager.instance.GetRandomTile(fireTargetTiles);
            if (targetedTile == null)
            {
                Debug.LogWarning("Didn't instantiate any thunder.");
                continue;
            }
            int2 coord = targetedTile.GetTilePos();
            ThunderClientRpc(coord.x, coord.y, index);

            count++;
        }

        Debug.Log("Fire successfully instantiated!");
    }

    [ClientRpc]
    private void ThunderClientRpc(int x, int y, int index)
    {
        Workshop fireWorkshop = EventsManager.instance.GetFireWorkshop(index);
        fireWorkshop.SetPosition(x, y);
        if (NetworkManager.Singleton.IsHost) fireWorkshop.ActivateServerRpc();
    }

    #endregion

    private Vector4 defaultVector = new Vector4(1f, 1f, 1f, 0f);

    [ClientRpc]
    private void StartStormEventFeedbackClientRpc()
    {
        CameraManager.instance.SetCurrentDeckCameraPosRot(posRotStorm.pos, posRotStorm.rot);
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);

        stormyVolume.enabled = true;
        LerpPostProcessEffect(0, 1, 2);

        enterStormEvent?.Invoke();
    }

    private async UniTask LerpPostProcessEffect(float from, float to, float duration)
    {
        Debug.Log("start lerp");
        stormyVolume.weight = from;

        float timer = 0;
        while (timer < duration)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            timer += Time.deltaTime;
            stormyVolume.weight = Mathf.Lerp(from, to, timer / duration);
        }

        stormyVolume.weight = to;
    }

    protected override void EndEvent()
    {
        EndStormEventFeedbackClientRpc();
    }

    [ClientRpc]
    private void EndStormEventFeedbackClientRpc()
    {
        CameraManager.instance.ResetDeckPosRot();
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
        exitStormEvent?.Invoke();

        DisableEffect();
    }

    private async void DisableEffect()
    {
        await LerpPostProcessEffect(1, 0, 1);
        if (SceneLoaderManager.instance.CancelTaskInGame()) return;

        stormyVolume.enabled = false;
    }
}