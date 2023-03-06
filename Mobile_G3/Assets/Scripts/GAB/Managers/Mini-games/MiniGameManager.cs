using UnityEngine;

public class MiniGameManager : MonoSingleton<MiniGameManager>
{
    private Workshop currentWorkshop;
    private MiniGame currentMiniGame;
    [SerializeField] private GameObject popUpMiniGame;

    [Header("TEMPORARY")]
    public CircularSwipeManager circularSwipeManager;
    public GyroscopeManager gyroscopeManager;

    public void StartWorkshopInteraction(Workshop workshop)
    {
        currentWorkshop = workshop;
        currentWorkshop.isOccupied.Value = true;
        
        var seriesWorkshop = workshop as SeriesWorkshop;
        if (seriesWorkshop)
        {
            StartMiniGame(seriesWorkshop.GetCurrentMiniGame());
        }
        else StartMiniGame(currentWorkshop.associatedMiniGame);
    }

    public void StartMiniGame(MiniGame game)
    {
        currentMiniGame = game;
        Debug.Log($"Started game {game}");
        popUpMiniGame.SetActive(true);
        currentMiniGame.StartMiniGame();
    }

    private void Update()
    {
        if (!currentMiniGame) return;
        currentMiniGame.ExecuteMiniGame();
    }

    public void ExitMiniGame(bool victory)
    {
        Debug.Log($"Exited with {victory}");
        popUpMiniGame.SetActive(false);
        currentMiniGame = null;
        EndWorkshopInteraction(victory);
    }

    private void EndWorkshopInteraction(bool victory)
    {
        if (currentWorkshop == null)
        {
            Debug.Log("There is not workshop associated with this mini-game!");
            return;
        }
        
        currentWorkshop.isOccupied.Value = false;
        currentWorkshop.Deactivate(victory);
    }
}
