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

    private static Vector3 QuadraticBezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        var pos1 = Vector3.Lerp(p1, p2, t);
        var pos2 = Vector3.Lerp(p2, p3, t);
        return Vector3.Lerp(pos1, pos2, t);
    }

    public static Vector3 CubicBezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
        var pos1 = QuadraticBezierCurve(p1, p2, p3, t);
        var pos2 = QuadraticBezierCurve(p2, p3, p4, t);
        return Vector3.Lerp(pos1, pos2, t);
    }
}