using UnityEngine;

public class SeriesWorkshop : Workshop
{
    public MiniGame[] nextMiniGames;
    [Tooltip("Index -1 is associatedMiniGame. Otherwise follows list indices")] public int currentMiniGameIndex = -1;

    public MiniGame GetCurrentMiniGame()
    {
        return currentMiniGameIndex == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndex];
    }

    public override void Deactivate(bool victory)
    {
        if (!victory) return;
        currentMiniGameIndex++;
        if (currentMiniGameIndex == nextMiniGames.Length)
        {
            currentMiniGameIndex = -1;
            SetActiveServerRpc(false);
            SetOccupiedServerRpc(false);
            return;
        }
        WorkshopManager.instance.StartWorkshopInteraction(this);
    }
}
