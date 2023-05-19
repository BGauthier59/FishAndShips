using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NewCanonWorkshop : Workshop
{
    private float timer;
    [SerializeField] private ParticleSystem explosionFx;
    
    public override void Start()
    {
        base.Start();
        isActive.OnValueChanged = OnSetActive;
    }

    private void OnSetActive(bool _, bool current)
    {
        Debug.Log("Set active for canon");

        if (current) WorkshopManager.instance.StartBulletFillersGlow();
        else WorkshopManager.instance.EndBulletFillersGlow();
    }

    public override void SetPosition(int posX, int posY)
    {
        if (currentTile == null)
        {
            //Debug.Log("This workshop does not have any current tile, then didn't reset last tile");
        }
        else currentTile.SetTile(null, currentTile.GetFloor());

        base.SetPosition(posX, posY);
    }

    public void PlayExplosionEffect()
    {
        explosionFx.Play();
    }
}