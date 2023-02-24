using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoudnessDetector : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip microphoneClip;

    void Start()
    {
        MicrophoneToAudioClip();
    }

    private void MicrophoneToAudioClip()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("There's no microphone available!");
            return;
        }
        string microphoneName = Microphone.devices[0];
        microphoneClip = Microphone.Start(microphoneName,
            true, 20, AudioSettings.outputSampleRate);
    }

    public float GetLoudnessFromMicrophone()
    {
        if (Microphone.devices.Length == 0) return 0;
        return GetLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), microphoneClip);
    }

    private float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;

        if (startPosition < 0)
        {
            Debug.Log("Zero.");
            return 0;
        }

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        // compute loudness
        float totalLoudness = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]); // 0 = no sound
        }

        return totalLoudness / sampleWindow;
    }
}