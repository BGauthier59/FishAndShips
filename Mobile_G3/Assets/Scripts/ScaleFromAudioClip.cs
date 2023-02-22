using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ScaleFromAudioClip : MonoBehaviour
{
    [SerializeField] private AudioLoudnessDetector detector;

    [SerializeField] private float loudnessSensibility = 100;
    [SerializeField] private float threshold = .1f;

    [SerializeField] private TMP_Text loudnessText;
    [SerializeField] private Gradient loudnessGradient;
    [SerializeField] private SpriteRenderer rd;
    
    private float loudness;

    void Update()
    {
        loudness = detector.GetLoudnessFromMicrophone() * loudnessSensibility;

        if (loudness < threshold)
        {
            loudness = 0;
        }

        loudnessText.text = loudness.ToString("F2");
        rd.color = loudnessGradient.Evaluate(loudness / loudnessSensibility);
    }
}