using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridEntity_InventoryFiller : MonoBehaviour, IGridEntity
{
    public InventoryObject filling;
    [SerializeField] private MeshRenderer[] glowyRenderers;
    [SerializeField] private UnityEvent pickEvent;
    [SerializeField] private Animation anim;

    public void OnCollision(IGridEntity entity, int direction)
    {
        PlayerManager player = entity as PlayerManager;
        if (player)
        {
            pickEvent?.Invoke();
            player.SetInventoryObject(filling);
            anim.Play("InventoryFillerFill");
        }
    }

    public void SetPosition(int posX, int posY)
    {
    }

    public void SetGlow(bool active)
    {
        foreach (var rd in glowyRenderers)
        {
            rd.material.SetFloat("_GLOWING", active ? 1 : 0);
        }
    }
}