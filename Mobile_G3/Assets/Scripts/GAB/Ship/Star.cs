using UnityEngine;

public class Star : MonoBehaviour
{
    [SerializeField] private byte index;
    [SerializeField] private bool taken;

    public void TakeStar()
    {
        taken = true;
    }

    public bool IsTaken()
    {
        return taken;
    }

    public byte GetIndex()
    {
        return index;
    }
}
