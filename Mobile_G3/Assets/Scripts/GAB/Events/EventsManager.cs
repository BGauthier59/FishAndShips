using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EventsManager : MonoSingleton<EventsManager>
{
    // La gestion des events est gérée par le Host

    // EventsManager va régulièrement check s'il peut créer un nouvel event, et le faire si besoin

    // Il va check les events qu'il peut créer (avec check de CheckConditions) et les stocker dans une liste, puis en choisir un aléatoirement

    [SerializeField] private RandomEvent currentEvent;

    public void StartGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!

        // For test
        if (currentEvent == null) return;
        currentEvent.StartEvent();
    }

    public void UpdateGameLoop()
    {
        if (!NetworkManager.Singleton.IsHost) return; // Manage by Host only!

        // For test
        if (currentEvent == null) return;
        currentEvent.ExecuteEvent();
    }
}