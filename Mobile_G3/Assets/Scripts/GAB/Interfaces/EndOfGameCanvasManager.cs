using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EndOfGameCanvasManager : MonoSingleton<EndOfGameCanvasManager>
{
    [SerializeField] private GameObject[] hostButtons, clientButtons;
    [SerializeField] private GameObject[] stars;
    [SerializeField] private GameObject[] victoryObjects, defeatObjects;

    [SerializeField] private UnityEvent victoryEvent;
    [SerializeField] private UnityEvent lostEvent;

    [SerializeField] private Image winnerImage;
    [SerializeField] private TMP_Text honorificMessageText;

    public void SetupCanvas(bool isHost, bool victory, int starCount, long[] honorificsData)
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
            victoryEvent?.Invoke();
            for (int i = 0; i < starCount; i++)
            {
                stars[i].SetActive(true);
            }
        }
        else lostEvent?.Invoke();

        ShowTitles(honorificsData);
    }

    private async void ShowTitles(long[] data)
    {
        int index = 0;
        PlayerManager winner;
        while (true)
        {
            if (SceneLoaderManager.instance.GetGlobalSceneState() != SceneLoaderManager.SceneState.InGameLevel) return;
            if (data[index] != -1)
            {
                winner = ConnectionManager.instance.players[(ulong) data[index]].player;
                winnerImage.sprite = winner.GetPlayerSprite();
                honorificMessageText.text = $"{winner.playerName.Value} {HonorificManager.instance.messages[index]}";
                await UniTask.Delay(4000);
            }

            await UniTask.Yield();
            index++;
            if (index == data.Length) index = 0;
        }
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

        // Disconnect in game, then can reset SceneLoaderManager value
        //SceneLoaderManager.instance.SetSceneStateBeforeDisconnection();
        GameManager.instance.PlayersGetDisconnected();
        // Should disconnect everyone and go back to main menu
    }
}