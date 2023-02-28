using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGame_Rudder : MiniGame
{
    [SerializeField] private GyroscopeSetupData data;
    [SerializeField] private TMP_Text rotationText;
    private Vector3 currentEulerAngles;
    private Quaternion currentRotation;
    
    public override void StartMiniGame()
    {
        base.StartMiniGame();
        MiniGameManager.instance.gyroscopeManager.Enable(data);
        currentEulerAngles = Vector3.zero;
    }
    
    public override void ExecuteMiniGame()
    {
        currentRotation = MiniGameManager.instance.gyroscopeManager.GetGyroRotation();
        currentEulerAngles = currentRotation.eulerAngles;
        currentEulerAngles.x = currentEulerAngles.y = 0;
        data.rotatingPoint.eulerAngles = currentEulerAngles;
        rotationText.text = currentEulerAngles.z.ToString("F1");
    }
    
    public override void ExitMiniGame(bool victory)
    {
        MiniGameManager.instance.gyroscopeManager.Disable();
        base.ExitMiniGame(victory);
    }
}
