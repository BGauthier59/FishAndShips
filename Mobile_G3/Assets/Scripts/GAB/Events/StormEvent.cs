using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StormEvent : RandomEvent
{
    #region Variables

    #region Sails Management

    [SerializeField] private float baseSailsActivationDuration;
    [SerializeField] private float randomSailsActivationGap;
    private float sailsActivationTimer;
    private float currentSailsActivationDuration;
    private bool isActivatingSails;

    #endregion

    #endregion

    public override void StartEvent()
    {
        StartStormEventFeedbackClientRpc();

        // Host-side logic
        base.StartEvent();
        SetNewSailsActivationDuration();
        Debug.Log("You entered a stormy area!");
    }
    
    [ClientRpc]
    private void StartStormEventFeedbackClientRpc()
    {
        // Todo - Implement feedback storm begins
    }

    public override bool CheckConditions()
    {
        // This is not fully random, then no need to check any condition
        return true;
    }

    public override void ExecuteEvent()
    {
        // Host-side only
        CheckSailsActivationTimer();
    }

    public override void EndEvent()
    {
        EndStormEventFeedbackClientRpc();
        
        // Host-side logic
        base.EndEvent();
        Debug.Log("You exited a stormy area!");
    }
    
    [ClientRpc]
    private void EndStormEventFeedbackClientRpc()
    {
        // Todo - Implement feedbacks   
    }

    #region Sails Management

    private void SetNewSailsActivationDuration()
    {
        currentSailsActivationDuration = baseSailsActivationDuration +
                                         Random.Range(-randomSailsActivationGap, randomSailsActivationGap);
        sailsActivationTimer = 0;
        isActivatingSails = false;
    }

    private void CheckSailsActivationTimer()
    {
        if (isActivatingSails) return;

        if (sailsActivationTimer > currentSailsActivationDuration)
        {
            TryActivateSails();
        }
        else sailsActivationTimer += Time.deltaTime;
    }

    [ClientRpc]
    private void ActivateSailsFeedbackClientRpc()
    {
        // Todo - Implement feedbacks

        // Pas le feedback du workshop, mais plutôt un truc global comme du vent fort qui souffle
        Debug.Log("Sails are activated");
    }

    private void TryActivateSails()
    {
        SetNewSailsActivationDuration();

        // Check conditions
        if (EventsManager.instance.sailsWorkshop.isActive.Value)
        {
            Debug.Log("Sails are already activated");
            return;
        }
        
        ActivateSailsFeedbackClientRpc();

        EventsManager.instance.sailsWorkshop
            .InitializeActivation(); // Special method for connected workshops to activate both
    }

    #endregion
}