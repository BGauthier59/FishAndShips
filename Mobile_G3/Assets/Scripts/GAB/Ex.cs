using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ex
{
    // This is the class for extension methods

    #region Transform

    public static void SetPosX(this Transform tr, float x)
    {
        var pos = tr.position;
        pos.x = x;
        tr.position = pos;
    }

    public static void SetPosY(this Transform tr, float y)
    {
        var pos = tr.position;
        pos.y = y;
        tr.position = pos;
    }

    public static void SetPosZ(this Transform tr, float z)
    {
        var pos = tr.position;
        pos.z = z;
        tr.position = pos;
    }

    #endregion

    #region Euler Angles

    public static void SetEulerAnglesX(this Transform tr, float x)
    {
        var pos = tr.eulerAngles;
        pos.x = x;
        tr.eulerAngles = pos;
    }

    public static void SetEulerAnglesY(this Transform tr, float y)
    {
        var pos = tr.eulerAngles;
        pos.y = y;
        tr.eulerAngles = pos;
    }

    public static void SetEulerAnglesZ(this Transform tr, float z)
    {
        var pos = tr.eulerAngles;
        pos.z = z;
        tr.eulerAngles = pos;
    }

    #endregion
}