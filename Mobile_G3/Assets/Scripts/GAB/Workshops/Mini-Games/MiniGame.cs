using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] private string miniGameName;
    public GameObject miniGameObject;
    [SerializeField] protected Vector3 miniGameCameraPosition;
    [SerializeField] protected Vector3 miniGameCameraEulerAngles;

    public virtual void StartMiniGame()
    {
        miniGameObject.SetActive(true);
    }

    public abstract void ExecuteMiniGame();

    public virtual void ExitMiniGame(bool victory)
    {
        WorkshopManager.instance.ExitMiniGame(victory);
        miniGameObject.SetActive(false);
    }

    public (Vector3 pos, Vector3 euler) GetCameraPositionRotation()
    {
        return (miniGameCameraPosition, miniGameCameraEulerAngles);
    }
}