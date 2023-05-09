using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridFloorStair : MonoBehaviour, IGridFloor
{
    public int positionX;
    public int positionY;
    
    
    public void SetPosition(int posX, int posY)
    {
        positionX = posX;
        positionY = posY;
    }

    // Quand entity fais l'input vers cette tile
    public void OnMove(IGridEntity entity,int direction)
    {
        if (GridManager.instance.GetOppositeTile(positionX, positionY).entity == null)
        {
            entity.SetPosition(positionX, positionY);   
        }
    }

    // Quand entity a atteint cette tile
    public void OnLand(IGridEntity entity)
    {
        entity.SetPosition(positionX >= GridManager.instance.xSize ? positionX - GridManager.instance.xSize : positionX + GridManager.instance.xSize, positionY);
    }

    public void EnterStair(PlayerManager player)
    {
        player.previousPos = player.transform.position;
        player.nextPos = GridManager.instance.GetTile(player.gridPositionX.Value, player.gridPositionY.Value).transform.position +
                         (player.positionX >= GridManager.instance.xSize ? Vector3.up * 0.8f : Vector3.zero);
        var dir = player.previousPos - player.nextPos;
        if (player.IsOwner)
        {
            player.InitializeBounce(dir);
        }
        else
        {
            PlayerManager localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (player.positionX >= GridManager.instance.xSize)player.InitializeBounce(dir);
                else
                {
                    player.ChangeTileInfos();
                    player.enterScreen = true;
                    player.nextPos = GridManager.instance.GetOppositeTile(player.gridPositionX.Value, player.gridPositionY.Value).transform.position + Vector3.up * 0.8f;
                    player.previousPos = player.nextPos + Vector3.up;
                }
            }
            else
            {
                if (player.positionX < GridManager.instance.xSize) player.InitializeBounce(dir);
                else
                {
                    player.ChangeTileInfos();
                    player.enterScreen = true;
                    player.nextPos = GridManager.instance.GetOppositeTile(player.gridPositionX.Value, player.gridPositionY.Value).transform.position;
                    player.previousPos = player.nextPos - Vector3.up;
                }
            }
        }
    }
    
    public void ExitStair(PlayerManager player)
    {
        player.previousPos = player.transform.position;
        player.nextPos = GridManager.instance.GetTile(player.gridPositionX.Value, player.gridPositionY.Value).transform.position + Vector3.up * 0.4f;
        var dir = player.previousPos - player.nextPos;

        if (player.IsOwner)
        {
            player.InitializeBounce(dir);
        }
        else
        {
            PlayerManager localPlayer = ConnectionManager.instance.players[NetworkManager.Singleton.LocalClientId];
            if (localPlayer.positionX >= GridManager.instance.xSize)
            {
                if (player.positionX >= GridManager.instance.xSize)player.InitializeBounce(dir);
                else
                {
                    player.ChangeTileInfos();
                    player.exitScreen = true;
                    player.previousPos = player.transform.position;
                    player.nextPos = player.transform.position + Vector3.up;
                }
            }
            else
            {
                if (player.positionX < GridManager.instance.xSize) player.InitializeBounce(dir);
                else
                {
                    player.ChangeTileInfos();
                    player.exitScreen = true;
                    player.previousPos = player.transform.position;
                    player.nextPos = player.transform.position - Vector3.up;
                }
            }
        }
    }
}