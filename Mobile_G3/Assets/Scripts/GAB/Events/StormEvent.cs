using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class StormEvent : RandomEvent
{
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
        enterStormEvent?.Invoke();
    }
    
    protected override void EndEvent()
    {
        EndStormEventFeedbackClientRpc();

        // Host-side logic
        base.EndEvent();
        Debug.Log("You exited a stormy area!");
    }

    [ClientRpc]
    private void EndStormEventFeedbackClientRpc()
    {
        exitStormEvent?.Invoke();
    }
}