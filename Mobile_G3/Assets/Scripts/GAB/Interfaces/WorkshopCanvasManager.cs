using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WorkshopCanvasManager : MonoSingleton<WorkshopCanvasManager>
{
    [SerializeField] private GameObject waitingObject;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private UnityEvent waitEvent;

    public void DisplayWaitingMessage(string message)
    {
        waitEvent?.Invoke();
        waitingObject.SetActive(true);
        waitingText.text = message;
    }

    public void HideWaitingMessage()
    {
        waitingObject.SetActive(false);
        waitingText.text = "";
    }

    public void OnQuitConnectedWorkshop()
    {
        if (!WorkshopManager.instance)
        {
            Debug.LogError("No Workshop Manager!");
            return;
        }
        WorkshopManager.instance.EndWorkshopInteraction(false);
    }
}
