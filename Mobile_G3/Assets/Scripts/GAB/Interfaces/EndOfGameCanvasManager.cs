using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGameCanvasManager : MonoSingleton<EndOfGameCanvasManager>
{
    [SerializeField] private GameObject[] hostButtons, clientButtons;
    [SerializeField] private GameObject[] stars;
    [SerializeField] private GameObject[] victoryObjects, defeatObjects;

    public void SetupCanvas(bool isHost, bool victory, int starCount)
    {
        if (isHost)
            foreach (var go in clientButtons)
                go.SetActive(false);
        else
            foreach (var go in hostButtons)
                go.SetActive(false);
        
        foreach (var go in victoryObjects) go.SetActive(victory);
        foreach (var go in defeatObjects) go.SetActive(!victory);
        
        foreach (var star in stars) star.SetActive(false);
        
        if (victory)
        {
            for (int i = 0; i < starCount; i++)
            {
                stars[i].SetActive(true);
            }
        }

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