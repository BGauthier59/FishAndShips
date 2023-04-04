using System;
using UnityEngine;

public class ShipPhysics : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Obstacle")) return;
        ShipManager.instance.Collide();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Star")) return;
        var star = other.GetComponent<Star>();
        if (star.IsTaken())
        {
            Debug.LogWarning("This star is already taken!");
            return;
        }
        star.TakeStar();
        ShipManager.instance.GetStar(star.GetIndex());
    }
}
