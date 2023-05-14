using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndOfGameCanvasManager : MonoSingleton<EndOfGameCanvasManager>
{
    [SerializeField] private GameObject[] hostButtons, clientButtons;

    public void SetupCanvas(bool isHost, bool victory)
    {
        if (isHost) foreach (var go in clientButtons) go.SetActive(false);
        else foreach (var go in hostButtons) go.SetActive(false);
        
        // Todo - show stars
        // Todo - show titles
        
    }

    public void OnContinue()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No host should call this method!");
            return;
        }
        
        SceneLoaderManager.instance.LoadMainMenuScene_AlreadyConnected();
        // Should load main menu with right configuration
    }
    
    public void OnQuit()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogError("No host should call this method!");
            return;
        }
        
        GameManager.instance.PlayersGetDisconnected();
        // Should disconnect everyone and go back to main menu
    }
}