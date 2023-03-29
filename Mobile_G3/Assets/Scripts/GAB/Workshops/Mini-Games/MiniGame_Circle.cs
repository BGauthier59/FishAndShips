using TMPro;
using UnityEngine;

public class MiniGame_Circle : MiniGame
{
    [SerializeField] private CircularSwipeSetupData data;
    [SerializeField] private float duration;
    private float timer;
    [SerializeField] private TMP_Text timerText;

    private void OnValidate()
    {
        data.availableZone.localScale = new Vector3(data.minMaxMagnitude.y * 2, data.minMaxMagnitude.y * 2, 1);
        data.unavailableCenterZone.localScale = new Vector3(data.minMaxMagnitude.x * 2, data.minMaxMagnitude.x * 2, 1);
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        WorkshopManager.instance.circularSwipeManager.Enable(data);
        timerText.text = duration.ToString("F0");
    }

    public override void ExecuteMiniGame()
    {
        if (timer >= duration)
        {
            ExitMiniGame(false);
        }
        else
        {
            timer += Time.deltaTime;

            if (WorkshopManager.instance.circularSwipeManager.CalculateCircularSwipe())
            {
                ExitMiniGame(true);
            }
        }

        timerText.text = (duration - timer).ToString("F0");
    }

    public override void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.circularSwipeManager.Disable();
        timer = 0;
        base.ExitMiniGame(victory);
    }
}