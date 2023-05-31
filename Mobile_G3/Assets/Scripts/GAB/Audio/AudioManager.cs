using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource boSource;

    [HideInInspector] public string[] effects;
    [HideInInspector] public string[] themes;

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
        effects = Enum.GetNames(typeof(AudioType));
        themes = Enum.GetNames(typeof(MusicType));
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
        soundSource.PlayOneShot(sounds[type]);
    }

    private MusicType currentMusic;
    private bool isFading;

    public async void PlayMusic(MusicType? type)
    {
        if (type.HasValue) currentMusic = type.Value;
        if (isFading) return;
        isFading = true;
        var initVolume = boSource.volume;

        // Fade out
        float timer = 0f;
        while (timer < 1)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
            boSource.volume = math.lerp(initVolume, 0, timer);
        }

        boSource.volume = 0;
        isFading = false;
        boSource.Stop();

        if (type == null)
        {
            boSource.volume = initVolume;
            return;
        }

        boSource.clip = bo[currentMusic];
        boSource.Play();

        // Fade in
        timer = 0f;
        while (timer < 1)
        {
            if (isFading) return;
            await UniTask.Yield();
            timer += Time.deltaTime;
            boSource.volume = math.lerp(0, initVolume, timer);
        }

        boSource.volume = initVolume;
    }

    public void PlayMusicInstant(MusicType type)
    {
        boSource.clip = bo[type];
        boSource.Play();
    }
}