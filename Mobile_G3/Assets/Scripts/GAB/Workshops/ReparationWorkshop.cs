using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReparationWorkshop : Workshop, IUpdateWorkshop
{
    [SerializeField] private int damagePerSecond;
    private float timer;
    
    private void Awake()
    {
        Setup();
    }
    
    public override void SetPosition(int posX, int posY)
    {
        if (currentTile == null)
        {
            Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null, currentTile.GetFloor());

        base.SetPosition(posX, posY);
    }
    
    protected override void RemoveWorkshopFromGrid()
    {
        // Called by every client when workshop is over
        base.RemoveWorkshopFromGrid();
        if (Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            EventsManager.instance.RemoveHole();
        }
    }

    public async void Setup()
    {
        while (WorkshopManager.instance == null)
        {
            await Task.Yield();
        }

        WorkshopManager.instance.AddUpdatedWorkshop(this);
    }

    public void StartGameLoopHostOnly()
    {
        // Does nothing?
    }

    public void UpdateGameLoopHostOnly()
    {
        if (!IsActiveOnGrid()) return;

        if (timer >= 1)
        {
            timer -= 1;
            ShipManager.instance.TakeDamage(damagePerSecond);
        }
        else timer += Time.deltaTime;
    }
}
