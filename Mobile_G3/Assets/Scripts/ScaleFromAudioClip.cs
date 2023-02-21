using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFromAudioClip : MonoBehaviour
{
    [SerializeField] private AudioLoudnessDetector detector;

    [SerializeField] private float loudnessSensibility = 100;
    [SerializeField] private float threshold = .1f;

    private float loudness;

    void Update()
    {
        loudness = detector.GetLoudnessFromMicrophone() * loudnessSensibility;

        if (loudness < threshold)
        {
            loudness = 0;
        }
        
        Debug.Log(loudness);
    }
}