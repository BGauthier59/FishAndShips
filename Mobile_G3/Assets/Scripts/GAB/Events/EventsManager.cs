using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public Workshop[] holeWorkshops;

    private bool isRunning;

    [SerializeField] private RandomEvent[] softEvents;
    [SerializeField] private RandomEvent[] hardEvents;

    private RandomEvent lastSoftEvent;
    private RandomEvent lastHardEvent;

    private float durationBeforeNextEvent, timer;
    private uint softEventCount, totalEventRunningCount;

    private bool checkTimer;

    private int currentActivatedWorkshopsCount;
    [SerializeField] private float maxWorkshops;
    [SerializeField] private float playerFactor;

    [SerializeField] private GridFloorNotWalkable notWalkable;
    [SerializeField] private UnityEvent startEvent;

    public void StartGameLoop()
    {
        // Host only
        if (!NetworkManager.Singleton.IsHost) return;
        
        // Récupérer les data du SO

        if (!data)
        {
            Debug.LogError("No data found!");
            return;
        }

        int playerCount = ConnectionManager.instance.players.Count;
        maxWorkshops = playerCount + 1;
        playerFactor = playerCount switch
        {
            2 => data.factorSpeed.p2,
            3 => data.factorSpeed.p3,
            4 => data.factorSpeed.p4,
            _ => 1 // Debug for one player
        };

        SetNewDuration();
    }

    private void SetNewDuration()
    {
        timer = 0;
        durationBeforeNextEvent = data.timerBetweenEvents +
                                  Random.Range(-data.randomTimerGapBetweenEvents, data.randomTimerGapBetweenEvents);

        durationBeforeNextEvent *=
            data.eventActivationSpeedCurve.Evaluate(TimerManager.instance.remainingDurationRatio)
            * playerFactor;
        checkTimer = true;
    }

    public void UpdateGameLoop()
    {
        // Host only
        if (!NetworkManager.Singleton.IsHost) return;

        if (!checkTimer) return;

        if (timer >= durationBeforeNextEvent)
        {
            StartNewEvent();
        }
        else timer += Time.deltaTime;
    }

    public void AddWorkshop()
    {
        currentActivatedWorkshopsCount++;
    }

    public void RemoveWorkshop()
    {
        currentActivatedWorkshopsCount--;
    }
    
    private async void StartNewEvent()
    {
        checkTimer = false;

        while (currentActivatedWorkshopsCount >= maxWorkshops)
        {
            await UniTask.Yield();
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;
        }

        RandomEvent nextEvent;
        if (softEventCount == data.hardEventsFrequency)
        {
            // Hard event

            do
            {
                nextEvent = hardEvents[Random.Range(0, hardEvents.Length)];
                await UniTask.DelayFrame(0);
                if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            } while (nextEvent == lastHardEvent || !nextEvent.CheckConditions());

            lastHardEvent = nextEvent;
            softEventCount = 0;

            StartEventFeedbackClientRpc(nextEvent.startEventText);
            await UniTask.Delay(2500);
            if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            if (!GameManager.instance.IsGameRunning()) return;
        }
        else
        {
            // Soft event

            do
            {
                nextEvent = softEvents[Random.Range(0, softEvents.Length)];
                await UniTask.DelayFrame(0);
                if (SceneLoaderManager.instance.CancelTaskInGame()) return;

            } while (nextEvent == lastSoftEvent || !nextEvent.CheckConditions());

            lastSoftEvent = nextEvent;
            softEventCount++;
        }

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

    #region Storm Macro-Management

    public int[] GetFireIndices()
    {
        List<int> availables = new List<int>();
        for (int i = 0; i < fireWorkshops.Length; i++)
        {
            if (fireWorkshops[i].isActive.Value) continue;
            availables.Add(i);
        }

        return availables.ToArray();
    }

    public Workshop GetFireWorkshop(int index)
    {
        return fireWorkshops[index];
    }

    #endregion

    public GridFloorNotWalkable GetNotWalkable()
    {
        return notWalkable;
    }

    public float GetScoreMultiplicationFactor()
    {
        return playerFactor;
    }
}