using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EventsManager : MonoSingleton<EventsManager>
{
    [SerializeField] private float durationBetweenRandomEventGenerationTry;
    private float timerBetweenRandomEventGenerationTry;

    #region Shrimp ships main variables

    [SerializeField] private float shrimpShipAttackActivationDuration;
    [SerializeField] private float durationBetweenShrimpShipAttacks;
    private bool isShrimpShipCooldownOver;

    private float timerBetweenShrimpShipAttacks;

    [SerializeField] private Vector3 lastAttackPosition;
    [SerializeField] private float minDistanceBetweenShrimpShipAttacks;
    private float minSqrDistanceBetweenShrimpShipAttacks;
    private float currentSqrDistanceBetweenShrimpShipAttacks;

    [SerializeField] private int maxShrimpInstantiatedCount, maxHoleInstantiatedCount;
    private int currentShrimpCount, currentHoleCount;

    [SerializeField] private ShrimpWorkshop[] shrimpWorkshops;
    [SerializeField] private ReparationWorkshop[] reparationWorkshops;

    #endregion

    #region Storms

    public static Action<StormEvent> OnEnterStorm;
    public ConnectedWorkshop sailsWorkshop;

    #endregion

    [SerializeField] private RandomEvent[] allRandomEvents;
    [SerializeField] private StormEvent[] stormEvents;
    [SerializeField] private List<RandomEvent> currentEvent = new List<RandomEvent>();
    private bool isRunning;

    public void StartGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!
        minSqrDistanceBetweenShrimpShipAttacks =
            minDistanceBetweenShrimpShipAttacks * minDistanceBetweenShrimpShipAttacks;
        lastAttackPosition = ShipManager.instance.GetShipPositionOnMap();
        OnEnterStorm = StartNewEvent;
        InitiateEventsManager();
    }

    private async void InitiateEventsManager()
    {
        isRunning = true;
        await Task.Delay((int) (shrimpShipAttackActivationDuration * 1000));
        StartShrimpShipCooldown();
    }

    private void StartNewEvent(RandomEvent randomEvent)
    {
        randomEvent.StartEvent();
        currentEvent.Add(randomEvent);
    }

    public void EndEvent(RandomEvent randomEvent)
    {
        currentEvent.Remove(randomEvent);
    }

    public void UpdateGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!
        if (!isRunning) return;

        TryGenerateNewRandomEvent();
        RunCurrentRandomEvents();
    }

    #region Shrimp ship Macro-Management

    public async void StartShrimpShipCooldown()
    {
        isShrimpShipCooldownOver = false;
        await Task.Delay((int) (durationBetweenShrimpShipAttacks * 1000));
        isShrimpShipCooldownOver = true;
    }

    public bool IsShrimpShipCooldownOver()
    {
        return isShrimpShipCooldownOver;
    }

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

    public void SetLastAttackPos()
    {
        lastAttackPosition = ShipManager.instance.GetShipPositionOnMap();
    }

    public void ResetDistanceFromLastAttack()
    {
        currentSqrDistanceBetweenShrimpShipAttacks = 0;
    }

    public bool IsFarEnoughFromLastAttack()
    {
        Debug.Log((ShipManager.instance.GetShipPositionOnMap() - lastAttackPosition).sqrMagnitude);
        return (ShipManager.instance.GetShipPositionOnMap() - lastAttackPosition).sqrMagnitude >
               minSqrDistanceBetweenShrimpShipAttacks;
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
            if(reparationWorkshops[i].isActive.Value) continue;
            return i;
        }

        return null;
    }

    public ReparationWorkshop GetReparationWorkshop(int index)
    {
        return reparationWorkshops[index];
    }

    #endregion

    #region Storm Macro-Management

    public StormEvent GetStormEvent(byte index)
    {
        return stormEvents[index];
    }

    #endregion

    private RandomEvent tempEvent;

    private void TryGenerateNewRandomEvent()
    {
        if (timerBetweenRandomEventGenerationTry > durationBetweenRandomEventGenerationTry)
        {
            timerBetweenRandomEventGenerationTry = 0;
            tempEvent = allRandomEvents[Random.Range(0, allRandomEvents.Length)];
            if (tempEvent.CheckConditions()) StartNewEvent(tempEvent);
        }
        else timerBetweenRandomEventGenerationTry += Time.deltaTime;
    }

    private void RunCurrentRandomEvents()
    {
        foreach (var randomEvent in currentEvent)
        {
            if (randomEvent == null) continue;
            randomEvent.ExecuteEvent();
        }
    }
}