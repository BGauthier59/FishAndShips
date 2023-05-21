using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource source;

    [Serializable]
    public struct AudioData
    {
        public AudioType type;
        public AudioClip clip;
    }

    [SerializeField] private AudioData[] data;
    private Dictionary<AudioType, AudioClip> sounds;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupDictionary();
    }

    private void SetupDictionary()
    {
        sounds = new Dictionary<AudioType, AudioClip>();
        foreach (var d in data)
        {
            if (sounds.ContainsKey(d.type))
            {
                Debug.LogError($"Can't add this data {d.type} twice!");
                continue;
            }
            sounds.Add(d.type, d.clip);
        }
    }

    public void PlaySound(AudioType type)
    {
        source.PlayOneShot(sounds[type]);
    }
}
