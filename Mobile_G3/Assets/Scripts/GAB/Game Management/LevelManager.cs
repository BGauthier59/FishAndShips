using System;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkMonoSingleton<LevelManager>
{
    public Level[] allLevels;
    private NetworkVariable<int> currentLevel = new NetworkVariable<int>();

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void StartLevel(int index)
    {
        currentLevel.Value = index;
        var level = allLevels[index];
        SceneLoaderManager.instance.LoadLevelScene(level.so.sceneName);
    }

    public void UpdateCurrentLevel(bool unlocked, bool victory, int starCount)
    {
        Debug.Log($"Updating level data for {currentLevel.Value}");
        var current = allLevels[currentLevel.Value];

        if (current.starCount < starCount)
        {
            Debug.Log($"We save stars!");
            current.starCount = starCount; // We only save data if star count is greater than before
        }

        Debug.Log($"We save data : {unlocked}, {victory}, {starCount}");
        SaveData.LevelData data = new SaveData.LevelData()
        {
            starCount = current.starCount
        };

        SaveManager.instance.UpdateCurrentLevelData(data, currentLevel.Value);

        if (currentLevel.Value + 1 == allLevels.Length)
        {
            Debug.Log("There's no level after this one");
            SaveManager.instance.SaveCurrentData();
            return;
        }
        
        // Here is to unlock new levels but we don't care
        SaveManager.instance.SaveCurrentData();
    }

    public void RefreshLevels(SaveData data)
    {
        for (int i = 0; i < allLevels.Length; i++)
        {
            var level = allLevels[i];
            var save = data.levelsData[i];
            level.starCount = save.starCount;
        }
    }
}

[Serializable]
public class Level
{
    public int starCount;
    public LevelSO so;
}