using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
    
    public ConnectedWorkshop mapWorkshop;
    public SeriesWorkshop[] cannonWorkshops;
    
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
    }

    public void RemoveHole()
    {
        currentHoleCount--;
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

    #region Cannon Macro-Management

    public int[] GetCannonIndices()
    {
        List<int> availables = new List<int>();
        for (int i = 0; i < cannonWorkshops.Length; i++)
        {
            if (cannonWorkshops[i].isActive.Value) continue;
            availables.Add(i);
        }

        return availables.ToArray();
    }
    public SeriesWorkshop GetCannonWorkshop(int index)
    {
        return cannonWorkshops[index];
    }

    #endregion

    private RandomEvent tempEvent;

    private async void TryGenerateNewRandomEvent()
    {
        // Todo - prevent events if game is over
        
        await UniTask.Delay((int) (1000 * durationBetweenEvents));

        if (!GameManager.instance.IsGameRunning())
        {
            Debug.Log("Can't start any event");
            return;
        }
        do
        {
            tempEvent = allRandomEvents[Random.Range(0, allRandomEvents.Length)];
            await UniTask.Yield();
            // While loop in async methods should not be a problem as long as we await for Task.Yield()
        } while (!tempEvent.CheckConditions() || tempEvent == lastEvent);

        StartEventFeedbackClientRpc(tempEvent.startEventText);
        await UniTask.Delay(2500);
        lastEvent = tempEvent;
        StartNewEvent(tempEvent);
    }

    [ClientRpc]
    private void StartEventFeedbackClientRpc(string message)
    {
        CameraManager.instance.PlayStartEventAnimation(message);
    }
}