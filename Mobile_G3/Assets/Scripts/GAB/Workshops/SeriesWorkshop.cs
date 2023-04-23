using Unity.Netcode;
using UnityEngine;

public class SeriesWorkshop : Workshop
{
    public MiniGame[] nextMiniGames;

    [Tooltip("Index -1 is associatedMiniGame. Otherwise follows list indices")]
    public NetworkVariable<int> currentMiniGameIndex = new(-1);

    public MiniGame GetCurrentMiniGame()
    {
        return currentMiniGameIndex.Value == -1 ? associatedMiniGame : nextMiniGames[currentMiniGameIndex.Value];
    }

    public override void Deactivate(bool victory)
    {
        associatedMiniGame.AssociatedWorkshopGetDeactivated();

        if (!victory)
        {
            SetOccupiedServerRpc(false);
            return;
        }

        SetMiniGameIndexServerRpc(currentMiniGameIndex.Value + 1);
        if (currentMiniGameIndex.Value == nextMiniGames.Length)
        {
            SetMiniGameIndexServerRpc(-1);
            SetActiveServerRpc(false);
            SetOccupiedServerRpc(false);
            return;
        }

        WorkshopManager.instance.StartWorkshopInteraction(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMiniGameIndexServerRpc(int index)
    {
        currentMiniGameIndex.Value = index;
    }
}