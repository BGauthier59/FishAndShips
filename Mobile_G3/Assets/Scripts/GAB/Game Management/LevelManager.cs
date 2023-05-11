using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager>
{
    public Level[] allLevels;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void StartLevel(int index)
    {
        var level = allLevels[index];
        // Start level with name stocked in SO
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
    private LevelSO so;
}
