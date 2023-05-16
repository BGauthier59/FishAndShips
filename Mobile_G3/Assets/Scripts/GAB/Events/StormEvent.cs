using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class StormEvent : RandomEvent
{
    [SerializeField] private PosRot posRotStorm;
    [SerializeField] private UnityEvent enterStormEvent;
    [SerializeField] private UnityEvent exitStormEvent;
    [SerializeField] private int2 fireMinMaxCount;
    private int fireCount, count;

    #region Main Methods

    public override bool CheckConditions()
    {
        if (EventsManager.instance.sailsWorkshop.isActive.Value) return false;
        return true;
    }

    public override async void StartEvent()
    {
        StartStormEventFeedbackClientRpc();

        // Host-side logic
        base.StartEvent();
        SetupEvent();
        
        GenerateSailsWorkshop();
        // todo - wait for fire mini-games

        await UniTask.Delay(2000);
        
        EndEvent();
    }

    private void SetupEvent()
    {
        fireCount = Random.Range(fireMinMaxCount.x, fireMinMaxCount.y);
        count = 0;
    }

    private void GenerateSailsWorkshop()
    {
        EventsManager.instance.sailsWorkshop.InitializeActivation();
    }

    #endregion

    [ClientRpc]
    private void StartStormEventFeedbackClientRpc()
    {
        CameraManager.instance.SetCurrentDeckCameraPosRot(posRotStorm.pos, posRotStorm.rot);
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
        enterStormEvent?.Invoke();
    }
    
    protected override void EndEvent()
    {
        EndStormEventFeedbackClientRpc();

        // Host-side logic
        base.EndEvent();
    }

    [ClientRpc]
    private void EndStormEventFeedbackClientRpc()
    {
        CameraManager.instance.ResetDeckPosRot();
        CameraManager.instance.SetZoomToCurrentCameraPosRot(BoatSide.Deck, 1);
        exitStormEvent?.Invoke();
    }
}