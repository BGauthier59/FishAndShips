using System.Threading.Tasks;
using UnityEngine;

public class MiniGame_Cannon_Load : MiniGame
{
    [SerializeField] private Animation cannonAnim;
    [SerializeField] private AnimationClip cannonIsComing;
    [SerializeField] private AnimationClip cannonIsReady;

    private CannonState currentState;

    [SerializeField] private SwipeData step1data;
    [SerializeField] private CannonDragAndDropData step2data;
    [SerializeField] private SwipeData step3data;
    
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
                if (WorkshopManager.instance.swipeManager.CalculateSwipe())
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
                if (WorkshopManager.instance.swipeManager.CalculateSwipe())
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
                WorkshopManager.instance.swipeManager.Enable(step1data);
                StartExecutingMiniGame();
                break;
            
            case CannonState.Step2:
                WorkshopManager.instance.swipeManager.Disable();
                cannonAnim.Play(cannonIsComing.name);
                await Task.Delay(1000);
                WorkshopManager.instance.cannonDragAndDropManager.Enable(step2data);
                WorkshopManager.instance.cannonDragAndDropManager.OnBulletOnTargetPoint += OnBulletWellPlaced;
                break;
            
            case CannonState.Step3:
                WorkshopManager.instance.cannonDragAndDropManager.Disable();
                WorkshopManager.instance.swipeManager.Enable(step3data);
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
        StopExecutingMiniGame();
        WorkshopManager.instance.swipeManager.Disable();
        cannonAnim.Play(cannonIsReady.name);
        await Task.Delay(1000);
        base.ExitMiniGame(victory);
    }
}