using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class EventsManager : MonoSingleton<EventsManager>
{
    // La gestion des events est gérée par le Host

    // EventsManager va régulièrement check s'il peut créer un nouvel event, et le faire si besoin

    // Il va check les events qu'il peut créer (avec check de CheckConditions) et les stocker dans une liste, puis en choisir un aléatoirement

    [SerializeField] private float durationBetweenRandomEventGenerationTry;
    private float timerBetweenRandomEventGenerationTry;
    
    [SerializeField] private float shrimpShipAttackActivationDuration;
    [SerializeField] private float durationBetweenShrimpShipAttacks;
    private bool isShrimpShipCooldownOver;

    private float timerBetweenShrimpShipAttacks;
    private float currentDistanceBetweenShrimpShipAttacks;

    [SerializeField] private RandomEvent[] allEvents;
    [SerializeField] private List<RandomEvent> currentEvent = new List<RandomEvent>();
    private bool isRunning;

    public RandomEvent DEBUG_selectedEvent;

    public void StartGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!
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

    [ContextMenu("Start current event")]
    public void DEBUG_StartEvent()
    {
        if (currentEvent == null) return;

        if (DEBUG_selectedEvent.CheckConditions())
        {
            isRunning = true;
            StartNewEvent(DEBUG_selectedEvent);
        }
        else Debug.LogWarning("Couldn't start selected event.");
    }

    [ContextMenu("Start Running")]
    public void DEBUG_StartRunning()
    {
        isRunning = true;
    }

    [ContextMenu("Stop Running")]
    public void DEBUG_StopRunning()
    {
        isRunning = false;
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

    #endregion
    
    private RandomEvent tempEvent;

    private void TryGenerateNewRandomEvent()
    {
        if (timerBetweenRandomEventGenerationTry > durationBetweenRandomEventGenerationTry)
        {
            timerBetweenRandomEventGenerationTry = 0;
            tempEvent = allEvents[Random.Range(0, allEvents.Length)];
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