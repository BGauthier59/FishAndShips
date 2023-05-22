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

    [SerializeField] private Volume volume;
    private ShadowsMidtonesHighlights shadowsMidtonesHighlights;

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

            Tile targetedTile = GridManager.instance.GetRandomWalkableTile();
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

    private Vector4 defaultVector = Vector4.zero;

    [ClientRpc]
    private void StartStormEventFeedbackClientRpc()
    {
        CameraManager.instance.SetCurrentDeckCameraPosRot(posRotStorm.pos, posRotStorm.rot);
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);

        if (volume.profile.TryGet(out shadowsMidtonesHighlights))
        {
            shadowsMidtonesHighlights.active = true;

            saveShadowsValue = shadowsMidtonesHighlights.shadows.value;
            saveMidtonesValue = shadowsMidtonesHighlights.midtones.value;
            saveHighlightsValue = shadowsMidtonesHighlights.highlights.value;

            LerpStormTones(defaultVector, saveShadowsValue, shadowsMidtonesHighlights);
            LerpStormTones(defaultVector, saveMidtonesValue, shadowsMidtonesHighlights);
            LerpStormTones(defaultVector, saveHighlightsValue, shadowsMidtonesHighlights);
        }
        else Debug.LogWarning("Can't access shadows midtones highlights!");

        enterStormEvent?.Invoke();
    }

    private Vector4 saveShadowsValue, saveMidtonesValue, saveHighlightsValue;

    private async UniTask LerpStormTones(Vector4 from, Vector4 to, ShadowsMidtonesHighlights s)
    {
        var timer = 0f;
        while (timer < 1)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
            s.shadows.Interp(from, to, 1 / timer);
        }
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
        if (volume.profile.TryGet(out shadowsMidtonesHighlights))
        {
            LerpStormTones(saveShadowsValue, defaultVector, shadowsMidtonesHighlights);
            LerpStormTones(saveMidtonesValue, defaultVector, shadowsMidtonesHighlights);
            await LerpStormTones(saveHighlightsValue, defaultVector, shadowsMidtonesHighlights);

            shadowsMidtonesHighlights.active = false;
        }
        else Debug.LogWarning("Can't access shadows midtones highlights!");
    }
}