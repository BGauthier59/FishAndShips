using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Trigger", order = 4)]
public class AudioTriggerSO : ScriptableObject
{
    private string[] sfx;
    private string[] musics;
    
    public void PlaySound(string sound)
    {
        if (sfx == null)
        {
            sfx = Enum.GetNames(typeof(AudioType));
            Debug.Log("Created array of SFX.");
        }

        for (int i = 0; i < sfx.Length; i++)
        {
            if (sfx[i] != sound) continue;
            PlaySound((AudioType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {sound}");
    }

    public void PlayMusic(string music)
    {
        musics ??= Enum.GetNames(typeof(MusicType));

        for (int i = 0; i < musics.Length; i++)
        {
            if (musics[i] != music) continue;
            PlayMusic((MusicType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {music}");
    }
    
    public void PlayMusicNoFading(string music)
    {
        musics ??= Enum.GetNames(typeof(MusicType));

        for (int i = 0; i < musics.Length; i++)
        {
            if (musics[i] != music) continue;
            PlayMusicNoFading((MusicType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {music}");
    }

    public void PlaySound(AudioType type)
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("Audio Manager is missing.");
            return;
        }

        AudioManager.instance.PlaySound(type);
    }

    public void PlayMusic(MusicType type)
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("Audio Manager is missing.");
            return;
        }

        AudioManager.instance.PlayMusic(type);
    }

    public void PlayMusicNoFading(MusicType type)
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("Audio Manager is missing.");
            return;
        }
        
        AudioManager.instance.PlayMusicInstant(type);
    }
}

public enum AudioType
{
    WinWorkshop,
    LoseWorkshop,
    StartEvent,
    StartWorkshop,
    WinGame,
    LoseGame,
    SailUp,
    SailDown,
    WaitConnected,
    
    CannonRotate,
    CannonBulletDrop,
    ShrimpSlash,
    ShrimpTurn,
}

public enum MusicType
{
    Menu,
    Tutorial,
    InGame
}