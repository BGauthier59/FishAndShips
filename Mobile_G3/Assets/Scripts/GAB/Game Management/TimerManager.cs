using UnityEngine;

public class TimerManager : MonoSingleton<TimerManager>
{
    [SerializeField] private int gameDuration;
    private int currentDuration;
    private float timer;

    public void StartGameLoop()
    {
        currentDuration = gameDuration;
        TimerCanvasManager.instance.SetTimerOnDisplay(currentDuration);
    }

    public void UpdateGameLoop()
    {
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer -= 1;
            currentDuration -= 1;

            if (currentDuration <= -1) GameManager.onGameEnds?.Invoke(false);
            else TimerCanvasManager.instance.SetTimerOnDisplay(currentDuration);
        }
    }
}