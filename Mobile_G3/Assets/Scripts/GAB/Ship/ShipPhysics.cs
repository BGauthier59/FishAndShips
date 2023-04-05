using System;
using Unity.Netcode;
using UnityEngine;

public class ShipPhysics : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        if (!other.gameObject.CompareTag("Obstacle")) return;
        
        // Todo - find correct angle

        var reflect = Vector2.Reflect(transform.right, other.contacts[0].normal);
        var angle = Vector2.SignedAngle(Vector2.right, reflect);

        ShipManager.instance.Collide(angle);
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
