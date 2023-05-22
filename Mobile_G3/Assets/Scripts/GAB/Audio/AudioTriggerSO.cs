using UnityEngine;

[CreateAssetMenu(menuName = "Audio Trigger", order = 4)]
public class AudioTriggerSO : ScriptableObject
{
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
    Sound1,
    Sound2,
    Sound3
}

public enum MusicType
{
    Menu,
    InGame
}