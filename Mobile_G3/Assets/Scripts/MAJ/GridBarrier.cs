using Unity.Netcode;
using UnityEngine;

public class GridBarrier : NetworkBehaviour
{
    public bool isClosedOnStart;
    public NetworkVariable<bool> isClosed = new NetworkVariable<bool>();

    public Transform barrier;
    public Vector3 closedPos, openPos;

    public void Setup()
    {
        if(isClosedOnStart) Close();
        else Open();
    }
    
    public void Refresh()
    {
        barrier.position = Vector3.Lerp(barrier.position, isClosed.Value ? closedPos : openPos, 5 * Time.deltaTime);
    }

    public void Open() // Host only
    {
        isClosed.Value = false;
    }
    
    public void Close() // Host only
    {
        isClosed.Value = true;
    }
}