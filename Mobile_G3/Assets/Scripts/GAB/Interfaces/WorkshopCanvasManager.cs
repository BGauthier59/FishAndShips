using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorkshopCanvasManager : MonoSingleton<WorkshopCanvasManager>
{
    [SerializeField] private GameObject waitingObject;
    [SerializeField] private TMP_Text waitingText;

    public void DisplayWaitingMessage(string message)
    {
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
