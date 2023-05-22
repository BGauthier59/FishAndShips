using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EventsManager : NetworkMonoSingleton<EventsManager>
{
    [SerializeField] private EventDataSO data;

    [SerializeField] private ShrimpWorkshop[] shrimpWorkshops;
    [SerializeField] private ReparationWorkshop[] reparationWorkshops;
    public SeriesWorkshop[] cannonWorkshops;
    public Workshop[] fireWorkshops;
    public ConnectedWorkshop mapWorkshop;
    public ConnectedWorkshop sailsWorkshop;

    private bool isRunning;

    [SerializeField] private RandomEvent[] softEvents;
    [SerializeField] private RandomEvent[] hardEvents;

    private RandomEvent lastSoftEvent;
    private RandomEvent lastHardEvent;

    private float durationBeforeNextEvent, timer;
    private uint softEventCount, totalEventRunningCount;

    private bool checkTimer;

    [SerializeField] private GridFloorNotWalkable notWalkable;
    [SerializeField] private UnityEvent startEvent;

    public void StartGameLoop()
    {
        // Récupérer les data du SO

        if (!data)
        {
            Debug.LogError("No data found!");
            return;
        }

        SetNewDuration();
    }

    private void SetNewDuration()
    {
        timer = 0;
        durationBeforeNextEvent = data.timerBetweenEvents +
                                  Random.Range(-data.randomTimerGapBetweenEvents, data.randomTimerGapBetweenEvents);
        checkTimer = true;
    }

    public void UpdateGameLoop()
    {
        if (!checkTimer) return;

        if (timer >= durationBeforeNextEvent)
        {
            StartNewEvent();
        }
        else timer += Time.deltaTime;
    }

    public bool waitingForInstantiating;

    private async void StartNewEvent()
    {
        checkTimer = false;
        waitingForInstantiating = true;

        RandomEvent nextEvent;
        if (softEventCount == data.hardEventsFrequency)
        {
            // Hard event

            do
            {
                nextEvent = hardEvents[Random.Range(0, hardEvents.Length)];
                await UniTask.DelayFrame(0);
            } while (nextEvent == lastHardEvent || !nextEvent.CheckConditions());

            lastHardEvent = nextEvent;
            softEventCount = 0;

            StartEventFeedbackClientRpc(nextEvent.startEventText);
            await UniTask.Delay(2500);
            if (!GameManager.instance.IsGameRunning()) return;
        }
        else
        {
            // Soft event

            do
            {
                nextEvent = softEvents[Random.Range(0, softEvents.Length)];
                await UniTask.DelayFrame(0);
            } while (nextEvent == lastSoftEvent || !nextEvent.CheckConditions());

            lastSoftEvent = nextEvent;
            softEventCount++;
        }

        waitingForInstantiating = false;

        nextEvent.StartEvent();
        SetNewDuration();
    }

    [ClientRpc]
    private void StartEventFeedbackClientRpc(string message)
    {
        CameraManager.instance.PlayStartEventAnimation(message);
        startEvent?.Invoke();
    }

    #region Shrimp ship Macro-Management

    public bool CanInstantiateShrimpWorkshop()
    {
        foreach (var shrimp in shrimpWorkshops)
        {
            if (!shrimp.isActive.Value) return true;
        }

        return false;
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
        return shrimpWorkshops[index];
    }

    public bool CanInstantiateHole()
    {
        foreach (var reparation in reparationWorkshops)
        {
            if (!reparation.isActive.Value) return true;
        }

        return false;
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

    public GridFloorNotWalkable GetNotWalkable()
    {
        return notWalkable;
    }
}