using Unity.Netcode;
using UnityEngine;

public class TimerManager : MonoSingleton<TimerManager>
{
    [SerializeField] private int gameDuration;
    private int currentDuration;
    private float timer;

    public void StartGameLoop()
    {
        currentDuration = gameDuration;
        MainCanvasManager.instance.SetTimerOnDisplay(currentDuration);
    }

    public void UpdateGameLoop()
    {
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer -= 1;
            currentDuration -= 1;

            if (currentDuration <= -1)
            {
                currentDuration = 0;
                if(NetworkManager.Singleton.IsHost) GameManager.instance.GameEnds(false);
            }
            else MainCanvasManager.instance.SetTimerOnDisplay(currentDuration);
        }
    }
}