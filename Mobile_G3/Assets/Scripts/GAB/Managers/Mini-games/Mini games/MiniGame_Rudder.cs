using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGame_Rudder : MiniGame
{
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private TMP_Text rotationText;
    private Vector3 currentEulerAngles;
    
    public override void StartMiniGame()
    {
        base.StartMiniGame();
        MiniGameManager.instance.gyroscopeManager.Enable(data);
        currentEulerAngles = Vector3.zero;
    }
    
    public override void ExecuteMiniGame()
    {
        currentEulerAngles = MiniGameManager.instance.gyroscopeManager.GetGyroRotation().eulerAngles;
        rotationText.text = currentEulerAngles.ToString();
    }
    
    public override void ExitMiniGame(bool victory)
    {
        MiniGameManager.instance.gyroscopeManager.Disable();
        base.ExitMiniGame(victory);
    }
}
