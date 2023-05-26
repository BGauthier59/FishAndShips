using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : NetworkMonoSingleton<SceneLoaderManager>
{
    [SerializeField] private int mainMenuIndex;
    [SerializeField] private string mainMenuSceneName;

    private NetworkVariable<SceneState> sceneState = new NetworkVariable<SceneState>();

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        LoadMainMenuScene_FirstTime();
        sceneState.OnValueChanged = SceneStateChange;
    }

    public void LoadMainMenuScene_FirstTime()
    {
        SceneManager.LoadScene(mainMenuIndex);
        sceneState.Value = SceneState.MainMenuFirstTime;
    }

    public void LoadMainMenuScene_AlreadyConnected()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        sceneState.Value = SceneState.MainMenuAlreadyConnected;
    }

    public void LoadLevelScene(string name)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
        sceneState.Value = SceneState.InGameLevel;
    }

    public SceneState GetGlobalSceneState()
    {
        return sceneState.Value;
    }

    private void SceneStateChange(SceneState _, SceneState current)
    {
        Debug.Log($"Scene state has been updated to {current.ToString()}");
    }

    public bool CancelTaskInGame()
    {
        return GetGlobalSceneState() != SceneState.InGameLevel;
    }

    public enum SceneState
    {
        MainMenuFirstTime,
        MainMenuAlreadyConnected,
        InGameLevel
    }
}