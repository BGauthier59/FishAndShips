using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
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
        WorkshopManager.instance.gyroscopeManager.Enable(data);
        currentEulerAngles = Vector3.zero;
    }

    public override void ExecuteMiniGame()
    {
        currentRotation = WorkshopManager.instance.gyroscopeManager.GetGyroRotation();
        currentEulerAngles = currentRotation.eulerAngles;
        currentEulerAngles.x = currentEulerAngles.y = 0;

        if (data.hasConstraint)
        {
            if (currentEulerAngles.z > data.leftConstraint &&
                currentEulerAngles.z < data.rightConstraint)
            {
                if (currentEulerAngles.z > (data.leftConstraint + data.rightConstraint) / 2)
                    currentEulerAngles.z = data.rightConstraint;
                else currentEulerAngles.z = data.leftConstraint;
            }
        }

        data.rotatingPoint.eulerAngles = currentEulerAngles;

        rotationText.text = currentEulerAngles.z.ToString("F1");
    }

    public override void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.gyroscopeManager.Disable();
        base.ExitMiniGame(victory);
    }
}