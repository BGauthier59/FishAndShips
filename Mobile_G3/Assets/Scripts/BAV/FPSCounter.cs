using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public bool useGUI = false;
    public TextMeshPro fpsText;
    public TextMeshProUGUI fpsTextGUI;
    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;
        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            if (useGUI)
            {
                fpsTextGUI.text = frameRate + "FPS";
            }
            else
            {
                fpsText.text = frameRate + "FPS";
            }

            time -= pollingTime;
            frameCount = 0;
        }
    }
}
