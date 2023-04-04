using UnityEngine;

public class ShipPhysics : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Obstacle")) return;
        ShipManager.instance.Collide();
    }
}
