using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoSingleton<LevelManager>
{
    public Level[] allLevels;
    private int currentLevel;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void StartLevel(int index)
    {
        currentLevel = index;
        var level = allLevels[index];
        // Start level with name stocked in SO
        NetworkManager.Singleton.SceneManager.LoadScene(level.so.sceneName, LoadSceneMode.Single);
    }

    public void UpdateCurrentLevel(bool unlocked, bool victory, int starCount)
    {
        var current = allLevels[currentLevel];
        current.isWon = victory;
        current.starCount = starCount;

        SaveData.LevelData data = new SaveData.LevelData()
        {
            isUnlocked = unlocked,
            isWon = victory,
            starCount = starCount
        };
        
        SaveManager.instance.UpdateCurrentLevelData(data, currentLevel);

        if (currentLevel + 1 == allLevels.Length)
        {
            Debug.Log("There's no level after this one");
            SaveManager.instance.SaveCurrentData();
            return;
        }
        
        var next = allLevels[currentLevel + 1];
        next.isUnlocked = true;
        next.isWon = false;
        next.starCount = 0;
        
        SaveData.LevelData dataNext = new SaveData.LevelData()
        {
            isUnlocked = true,
            isWon = false,
            starCount = 0
        };
        
        SaveManager.instance.UpdateCurrentLevelData(dataNext, currentLevel + 1);
        SaveManager.instance.SaveCurrentData();
    }

    public void RefreshLevels(SaveData data)
    {
        // Mettre à jour l'array de Levels en fonction de la save
        for (int i = 0; i < allLevels.Length; i++)
        {
            var level = allLevels[i];
            var save = data.levelsData[i];
            level.isUnlocked = save.isUnlocked;
            level.isWon = save.isWon;
            level.starCount = save.starCount;
        }
    }
}

[Serializable]
public class Level
{
    public bool isUnlocked;
    public bool isWon;
    public int starCount;
    public LevelSO so;
}