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
        if(isClosedOnStart) Toggle();
    }
    
    public void Refresh()
    {
        barrier.position = Vector3.Lerp(barrier.position, isClosed.Value ? closedPos : openPos, 5 * Time.deltaTime);
    }

    public void Toggle() // Host only
    {
        isClosed.Value = !isClosed.Value;
    }
}