using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Trigger", order = 4)]
public class AudioTriggerSO : ScriptableObject
{
    public void PlaySound(string sound)
    {
        var names = Enum.GetNames(typeof(AudioType));

        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] != sound) continue;
            PlaySound((AudioType) i);
            return;
        }
        
        Debug.LogWarning($"Sound not found. Given name is {sound}");
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

        // todo - start music
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
    InGame
}