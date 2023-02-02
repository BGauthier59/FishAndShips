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
}