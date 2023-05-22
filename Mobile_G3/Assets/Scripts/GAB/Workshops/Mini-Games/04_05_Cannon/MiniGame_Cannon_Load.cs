using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class MiniGame_Cannon_Load : MiniGame
{
    [SerializeField] private Animation cannonAnim;
    [SerializeField] private AnimationClip cannonIsComing;
    [SerializeField] private AnimationClip cannonIsReady;

    private CannonState currentState;

    [SerializeField] private SwipeData step1data;
    [SerializeField] private CannonDragAndDropData step2data;
    [SerializeField] private SwipeData step3data;

    [Header("Environment Data")]
    [SerializeField] private Transform rotatingPart, cannon, muzzle;
    private Vector3 rotatingPartEulerAngles, cannonEulerAngles, cannonPos, muzzleEulerAngles;

    [SerializeField] private UnityEvent rotateEvent;
    [SerializeField] private UnityEvent filledEvent;
    
    private enum CannonState
    {
        Step1, Step2, Step3, Transition
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(step1data.centralPoint.position, step1data.rightDirection);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(step3data.centralPoint.position, step3data.rightDirection);
    }

    public override void OnNetworkSpawn()
    {
        rotatingPartEulerAngles = rotatingPart.eulerAngles;
        cannonEulerAngles = cannon.eulerAngles;
        cannonPos = cannon.position;
        muzzleEulerAngles = muzzle.eulerAngles;
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();

        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        SwitchState(CannonState.Step1);
    }
    
    public override void ExecuteMiniGame()
    {
        switch (currentState)
        {
            case CannonState.Step1:
                if (WorkshopManager.instance.swipeManager.CalculateSwipe()) SwitchState(CannonState.Step2);
                break;
            
            case CannonState.Step2:
                WorkshopManager.instance.cannonDragAndDropManager.CalculateBulletPosition();
                break;
            
            case CannonState.Step3:
                if (WorkshopManager.instance.swipeManager.CalculateSwipe()) ExitLastStep();
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
                WorkshopManager.instance.swipeManager.Enable(step1data);
                StartExecutingMiniGame();
                WorkshopManager.instance.StartMiniGameTutorial(0);
                break;
            
            case CannonState.Step2:
                WorkshopManager.instance.swipeManager.Disable();
                cannonAnim.Play(cannonIsComing.name);
                rotateEvent?.Invoke();
                await UniTask.Delay(1000);
                WorkshopManager.instance.cannonDragAndDropManager.Enable(step2data);
                WorkshopManager.instance.cannonDragAndDropManager.OnBulletOnTargetPoint = OnBulletWellPlaced;
                WorkshopManager.instance.StartMiniGameTutorial(1);
                break;
            
            case CannonState.Step3:
                filledEvent?.Invoke();
                WorkshopManager.instance.cannonDragAndDropManager.Disable();
                WorkshopManager.instance.swipeManager.Enable(step3data);
                await UniTask.Delay(100);
                WorkshopManager.instance.StartMiniGameTutorial(2);
                break;
        }

        currentState = state;
    }

    private void OnBulletWellPlaced()
    {
        if (currentState != CannonState.Step2)
        {
            Debug.LogError($"Should not invoke this when on {currentState}");
            return;
        }
        WorkshopManager.instance.cannonDragAndDropManager.OnBulletOnTargetPoint = null;
        SwitchState(CannonState.Step3);
    }

    private async void ExitLastStep()
    {
        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        cannonAnim.Play(cannonIsReady.name);
        
        WorkshopManager.instance.SetVictoryIndicator();

        await UniTask.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        
        ExitMiniGame(true);
    }
    
    protected override void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void Reset()
    {
        rotatingPart.eulerAngles = rotatingPartEulerAngles;
        cannon.eulerAngles = cannonEulerAngles;
        cannon.position = cannonPos;
        muzzle.eulerAngles = muzzleEulerAngles;
    }

    public override void OnLeaveMiniGame()
    {
        if (!isRunning) return;
        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        ExitMiniGame(false);
    }
}
