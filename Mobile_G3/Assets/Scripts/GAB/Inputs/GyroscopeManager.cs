using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Gyroscope = UnityEngine.Gyroscope;

public class GyroscopeManager : MiniGameInput<GyroscopeSetupData>
{
    private Gyroscope gyroscope;
    private Quaternion correctionQuaternion;
    
    public override void Enable(GyroscopeSetupData data)
    {
        base.Enable(data);
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogWarning("This machine does not support gyroscope");
            Disable();
            return;
        }
        gyroscope = Input.gyro;
        gyroscope.enabled = true;
        correctionQuaternion = Quaternion.Euler(90f, 0, 0);
    }

    public Quaternion GetGyroRotation()
    {
        var gyroQuaternion = GyroToUnity(gyroscope.attitude);
        return correctionQuaternion * gyroQuaternion;
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
    
    public override void Disable()
    {
        base.Disable();
        gyroscope.enabled = false;
    }
}

[Serializable]
public struct GyroscopeSetupData
{
    public bool hasConstraint;
    public Vector3 leftConstraint;
    public Vector3 rightConstraint;
    public float sensibility;
    
}
