using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EventsManager : NetworkMonoSingleton<EventsManager>
{
    [SerializeField] private float durationBetweenRandomEventGenerationTry;
    private float timerBetweenRandomEventGenerationTry;

    #region Shrimp ships main variables

    [SerializeField] private int maxShrimpInstantiatedCount, maxHoleInstantiatedCount;
    private int currentShrimpCount, currentHoleCount;

    [SerializeField] private ShrimpWorkshop[] shrimpWorkshops;
    [SerializeField] private ReparationWorkshop[] reparationWorkshops;

    #endregion

    #region Storms

    public ConnectedWorkshop sailsWorkshop;

    #endregion

    [SerializeField] private float durationBetweenEvents;
    [SerializeField] private RandomEvent[] allRandomEvents;
    [SerializeField] private RandomEvent lastEvent;
    private bool isRunning;

    #region Main Methods

    public void StartGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!
        TryGenerateNewRandomEvent();
    }

    private void StartNewEvent(RandomEvent randomEvent)
    {
        randomEvent.StartEvent();
    }

    public void EndEvent(RandomEvent randomEvent)
    {
        TryGenerateNewRandomEvent();
    }
    
    #endregion

    #region Shrimp ship Macro-Management

    public bool CanInstantiateShrimpWorkshop()
    {
        return currentShrimpCount < maxShrimpInstantiatedCount;
    }

    public void AddShrimp()
    {
        currentShrimpCount++;
    }

    public void RemoveShrimp()
    {
        currentShrimpCount--;
    }

    public int? GetShrimpWorkshopIndex()
    {
        for (int i = 0; i < shrimpWorkshops.Length; i++)
        {
            if (shrimpWorkshops[i].isActive.Value) continue;
            return i;
        }

        return null;
    }

    public ShrimpWorkshop GetShrimpWorkshop(int index)
    {
        // WARNING! The index must be the one sent by Host as currentShrimpCount is not modified on clients

        return shrimpWorkshops[index];
    }

    public bool CanInstantiateHole()
    {
        return currentHoleCount < maxHoleInstantiatedCount;
    }

    public void AddHole()
    {
        currentHoleCount++;
        ShipManager.instance.SetRegenerationAbility(false);
    }

    public void RemoveHole()
    {
        currentHoleCount--;
        if (currentHoleCount == 0) ShipManager.instance.SetRegenerationAbility(true);
    }

    public int? GetReparationWorkshopIndex()
    {
        for (int i = 0; i < reparationWorkshops.Length; i++)
        {
            if (reparationWorkshops[i].isActive.Value) continue;
            return i;
        }

        return null;
    }

    public ReparationWorkshop GetReparationWorkshop(int index)
    {
        return reparationWorkshops[index];
    }

    #endregion

    private RandomEvent tempEvent;

    private async void TryGenerateNewRandomEvent()
    {
        await Task.Delay((int) (1000 * durationBetweenEvents));

        do
        {
            tempEvent = allRandomEvents[Random.Range(0, allRandomEvents.Length)];
            await Task.Yield();
            // While loop in async methods should not be a problem as long as we await for Task.Yield()
        } while (!tempEvent.CheckConditions() || tempEvent == lastEvent);

        StartEventFeedbackClientRpc(tempEvent.startEventText);
        await Task.Delay(2500);
        lastEvent = tempEvent;
        StartNewEvent(tempEvent);
    }

    [ClientRpc]
    private void StartEventFeedbackClientRpc(string message)
    {
        CameraManager.instance.PlayStartEventAnimation(message);
    }
}