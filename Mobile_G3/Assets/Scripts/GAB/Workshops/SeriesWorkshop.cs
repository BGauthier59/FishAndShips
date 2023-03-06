using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeriesWorkshop : Workshop
{
    public MiniGame[] nextMiniGames;
    [Tooltip("Index -1 is associatedMiniGame. Otherwise follows list indices")] public int currentMiniGameIndex = -1;

    public override void OnCollision(IGridEntity entity)
    {
        base.OnCollision(entity);
    }

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
            isActive.Value = false;
            isOccupied.Value = false;
            return;
        }
        MiniGameManager.instance.StartWorkshopInteraction(this);
    }
}
