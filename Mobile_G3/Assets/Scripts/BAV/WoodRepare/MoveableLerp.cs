using UnityEngine;
using System.Collections;

public class MoveableLerp : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 1.0f;

    private void Start()
    {
        // Start the coroutine that moves the object
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        while (true)
        {
            // Move the object from point A to point B
            float t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(pointA, pointB, t);
                yield return null;
            }

            // Move the object from point B to point A
            t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(pointB, pointA, t);
                yield return null;
            }
        }
    }
}