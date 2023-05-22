using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Trigger", order = 4)]
public class AudioTriggerSO : ScriptableObject
{
    public void PlaySound(string sound)
    {
        for (int i = 0; i < AudioManager.instance.effects.Length; i++)
        {
            if (AudioManager.instance.effects[i] != sound) continue;
            PlaySound((AudioType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {sound}");
    }

    public void PlayMusic(string music)
    {
        for (int i = 0; i < AudioManager.instance.themes.Length; i++)
        {
            if (AudioManager.instance.themes[i] != music) continue;
            PlayMusic((MusicType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {music}");
    }
    
    public void PlayMusicNoFading(string music)
    {
        for (int i = 0; i < AudioManager.instance.themes.Length; i++)
        {
            if (AudioManager.instance.themes[i] != music) continue;
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
    ShrimpMove,
    Thunder,
    PressurePlate,
    PickItem,
    Move,
    StartFire,
    CannonShot,
    
    SpawnHole,
    StartStorm,
    SetPlank
}

public enum MusicType
{
    Menu,
    Tutorial,
    InGame
}