using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReparationWorkshop : Workshop
{
    private float timer;
    [SerializeField] private ParticleSystem explosionFx;

    public override void SetPosition(int posX, int posY)
    {
        if (currentTile == null)
        {
            //Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null, currentTile.GetFloor());

        base.SetPosition(posX, posY);
    }
    
    protected override void RemoveWorkshopFromGrid()
    {
        // Called by every client when workshop is over
        base.RemoveWorkshopFromGrid();
    }

    public void PlayExplosionEffect()
    {
        explosionFx.Play();
    }
}
