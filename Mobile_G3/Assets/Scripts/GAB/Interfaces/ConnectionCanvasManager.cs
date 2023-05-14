using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionCanvasManager : MonoSingleton<ConnectionCanvasManager>
{
    public void OnQuit()
    {
        SceneLoaderManager.instance.LoadMainMenuScene_FirstTime();
    }
}
