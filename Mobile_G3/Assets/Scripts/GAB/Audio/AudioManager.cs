using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource boSource;

    [Serializable]
    public struct AudioData
    {
        public AudioType type;
        public AudioClip clip;
    }

    [Serializable]
    public struct MusicData
    {
        public MusicType type;
        public AudioClip clip;
    }

    [SerializeField] private AudioData[] data;
    [SerializeField] private MusicData[] musics;
    private Dictionary<AudioType, AudioClip> sounds;
    private Dictionary<MusicType, AudioClip> bo;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupDictionaries();
    }

    private void SetupDictionaries()
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

        bo = new Dictionary<MusicType, AudioClip>();

        foreach (var d in musics)
        {
            if (bo.ContainsKey(d.type))
            {
                Debug.LogError($"Can't add this data {d.type} twice!");
                continue;
            }

            bo.Add(d.type, d.clip);
        }
    }

    public void PlaySound(AudioType type)
    {
        soundSource.PlayOneShot(sounds[type]);
    }

    private MusicType currentMusic;
    private bool isFading;

    public async void PlayMusic(MusicType type)
    {
        currentMusic = type;
        if (isFading) return;
        isFading = true;

        // Fade out & fade in

        boSource.PlayOneShot(bo[currentMusic]);

        // Start the current music after the fade
    }
}