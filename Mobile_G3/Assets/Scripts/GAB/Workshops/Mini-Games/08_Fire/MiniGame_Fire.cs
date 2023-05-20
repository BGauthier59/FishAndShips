using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniGame_Fire : MiniGame
{
    /* 23/04
     * On ne produit pas ce mini-jeu pour l'instant !
     *
     * Priorité sur les choses pas encore produites
     * GameLoop, Events, etc
     */
    
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private FireGyroscopeSetupData fireData;
    [SerializeField] private float damagePerSecond;
    
    private Vector3 fireInitPos;
    
    public override void OnNetworkSpawn()
    {
        fireInitPos = fireData.fireSizeType[0].position;
    }

    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        
        // Enables Gyroscope
        await UniTask.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());
        WorkshopManager.instance.gyroscopeManager.Enable(data);
        StartExecutingMiniGame();
    }

    private void SetupHole()
    {
        
    }
    
    public override void AssociatedWorkshopGetActivatedHostSide()
    {
        base.AssociatedWorkshopGetActivatedHostSide();
        // Todo - Boat takes damages
    }
    

    public override void ExecuteMiniGame()
    {
        //  
    }

    public override void Reset()
    {
        
    }

    protected override void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void OnLeaveMiniGame()
    {
        ExitMiniGame(false);
    }
}
