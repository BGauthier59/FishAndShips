using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public class TutorialFireSoftEvent : RandomEvent
{
    [SerializeField] private int2[] spawnTiles, otherSpawnTiles;

    public override bool CheckConditions()
    {
        return true;
    }

    public override void StartEvent()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Host is managing event.");
            return;
        }
        
        GenerateFireWorkshops();
    }

    protected override void EndEvent()
    {
        
    }
    
    private void GenerateFireWorkshops()
    {
        for (int i = 0; i < TutorialEventManager.instance.playerNb; i++)
        {
            Tile targetedTile = GridManager.instance.GetTile(spawnTiles[i].x, spawnTiles[i].y);
            if (targetedTile.GetEntity() != null)
                targetedTile = GridManager.instance.GetTile(otherSpawnTiles[i].x, otherSpawnTiles[i].y);

            int2 coord = targetedTile.GetTilePos();
            GenerateFireWorkshopsClientRpc(coord.x, coord.y, i);
        }
    }

    [ClientRpc]
    private void GenerateFireWorkshopsClientRpc(int x, int y, int index)
    {
        Workshop workshop = TutorialEventManager.instance.GetFireWorkshop(index);
        workshop.SetPosition(x, y);
        if (NetworkManager.Singleton.IsHost)workshop.ActivateServerRpc();
    }
}
