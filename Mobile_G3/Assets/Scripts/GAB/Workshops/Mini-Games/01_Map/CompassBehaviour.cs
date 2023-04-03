using UnityEngine;

public class CompassBehaviour : MonoBehaviour
{
    [SerializeField] private Transform rotatingPart;
    private ShipManager ship;

    private void Start()
    {
        ship = ShipManager.instance;
    }

    private void Update()
    {
        rotatingPart.localEulerAngles = ship.GetShipAngle() * Vector3.up;
    }
}