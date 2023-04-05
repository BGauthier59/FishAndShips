using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MiniGame_Cannon_Load : MiniGame
{
    [SerializeField] private Animation cannonAnim;
    [SerializeField] private AnimationClip cannonIsComing;
    [SerializeField] private AnimationClip cannonIsReady;

    private CannonState currentState;

    [SerializeField] private CannonSwipeData step1data;
    [SerializeField] private CannonDragAndDropData step2data;
    [SerializeField] private CannonSwipeData step3data;
    
    
    private enum CannonState
    {
        Step1, Step2, Step3, Transition
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(step1data.centralPoint.position, step1data.rightDirection);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(step3data.centralPoint.position, step3data.rightDirection);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        SwitchState(CannonState.Step1);
    }
    
    public override void ExecuteMiniGame()
    {
        switch (currentState)
        {
            case CannonState.Step1:
                // Swipe
                if (WorkshopManager.instance.cannonCannonSwipeManager.CalculateSwipe())
                {
                    SwitchState(CannonState.Step2);
                }
                break;
            case CannonState.Step2:
                // Drag'n'drop
                WorkshopManager.instance.cannonDragAndDropManager.CalculateBulletPosition();
                break;
            case CannonState.Step3:
                // Swipe
                if (WorkshopManager.instance.cannonCannonSwipeManager.CalculateSwipe())
                {
                    ExitMiniGame(true);
                }
                break;
            case CannonState.Transition:
                break;
        }
    }

    private async void SwitchState(CannonState state)
    {
        if (state == CannonState.Transition) return;
        currentState = CannonState.Transition;
        switch (state)
        {
            case CannonState.Step1:
                WorkshopManager.instance.cannonCannonSwipeManager.Enable(step1data);
                break;
            
            case CannonState.Step2:
                WorkshopManager.instance.cannonCannonSwipeManager.Disable();
                cannonAnim.Play(cannonIsComing.name);
                await Task.Delay(1000);
                WorkshopManager.instance.cannonDragAndDropManager.Enable(step2data);
                WorkshopManager.instance.cannonDragAndDropManager.OnBulletOnTargetPoint += OnBulletWellPlaced;
                break;
            
            case CannonState.Step3:
                WorkshopManager.instance.cannonDragAndDropManager.Disable();
                WorkshopManager.instance.cannonCannonSwipeManager.Enable(step3data);
                break;
        }

        currentState = state;
    }

    private void OnBulletWellPlaced()
    {
        WorkshopManager.instance.cannonDragAndDropManager.OnBulletOnTargetPoint -= OnBulletWellPlaced;
        SwitchState(CannonState.Step3);
    }
    
    public override async void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.cannonCannonSwipeManager.Disable();
        cannonAnim.Play(cannonIsReady.name);
        await Task.Delay(1000);
        base.ExitMiniGame(victory);
    }
}
