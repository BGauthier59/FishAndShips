using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPerf : MonoBehaviour
{
    public AnimationCurve curve;
    public int x, y, speed;
    public bool countFPS;
    public TMP_Text counter;
    public Color green, red;
    void LateUpdate()
    {
        transform.position = new Vector3(x + Mathf.Sin(Time.time*speed) * 0.5f + 0.5f,
            0.4f + curve.Evaluate(Mathf.Sin(Time.time*speed) * 0.5f + 0.5f), y);

        if (countFPS)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            counter.text = "FPS : " + fps;
            counter.color = Color.Lerp(red,green,fps / 120f);
        }
    }
}
