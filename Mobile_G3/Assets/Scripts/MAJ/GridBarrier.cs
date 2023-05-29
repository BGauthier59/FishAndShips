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
        if(isClosedOnStart) ToggleServerRpc();
    }
    
    public void Refresh()
    {
        barrier.position = Vector3.Lerp(barrier.position, isClosed.Value ? closedPos : openPos, 5 * Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleServerRpc()
    {
        isClosed.Value = !isClosed.Value;
    }
}