using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Gyroscope = UnityEngine.Gyroscope;

public class GyroscopeManager : MiniGameInput<GyroscopeSetupData>
{
    private Gyroscope gyroscope;
    private Quaternion correctionQuaternion;
    
    public override void Enable(GyroscopeSetupData data)
    {
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogWarning("This machine does not support gyroscope");
            Disable();
            return;
        }

        base.Enable(data);

        gyroscope = Input.gyro;
        gyroscope.enabled = true;
        correctionQuaternion = Quaternion.Euler(90f, 0, 0);
    }

    public Quaternion GetGyroRotation()
    {
        if (!isActive)
        {
            //Debug.Log("Can't use gyroscope");
            return Quaternion.identity;
        }

        var gyroQuaternion = GyroToUnity(gyroscope.attitude);
        var calculatedQuaternion = correctionQuaternion * gyroQuaternion;

        return calculatedQuaternion;
    }

    private Quaternion GyroToUnity(Quaternion q)
    {
        var rotation = new Quaternion(q.x, q.y, -q.z, -q.w);
        return rotation;
    }

    public override void Disable()
    {
        base.Disable();
        if (gyroscope != null) gyroscope.enabled = false;
    }
}

[Serializable]
public struct GyroscopeSetupData
{
    
}