using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource boSource;

    [Serializable]
    public class AudioData
    {
        public string DEBUG_name;
        public AudioType type;
        public AudioClip clip;
    }

    [Serializable]
    public class MusicData
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

    private void OnValidate()
    {
        foreach (var d in data)
        {
            d.DEBUG_name = d.type.ToString();
        }
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
        Debug.Log($"Played {type}, which is {sounds[type].name}");
        soundSource.PlayOneShot(sounds[type]);
    }

    private MusicType currentMusic;
    private bool isFading;

    public async void PlayMusic(MusicType type)
    {
        currentMusic = type;
        if (isFading) return;
        isFading = true;
        var initVolume = boSource.volume;
        
        // Fade out
        float timer = 0f;
        while (timer < 1)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
            boSource.volume = math.lerp(initVolume, 0, 1 / timer);
        }

        boSource.volume = 0;
        boSource.clip = bo[currentMusic];
        isFading = false;
        
        // Fade in
        timer = 0f;
        while (timer < 1)
        {
            if (isFading) return;
            await UniTask.Yield();
            timer += Time.deltaTime;
            boSource.volume = math.lerp(0, initVolume, 1 / timer);
        }
        
        boSource.volume = 1;
    }

    public void PlayMusicInstant(MusicType type)
    {
        boSource.clip = bo[type];
        boSource.Play();
    }
}