using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverTester : MonoBehaviour
{
    [SerializeField] private SwipeManager controls;
    private Vector3 nextPos;
    [SerializeField] private float speed;

    private void Update()
    {
        if (controls.SwipeLeft) nextPos += Vector3.left;
        if (controls.SwipeRight) nextPos += Vector3.right;
        if (controls.SwipeUp) nextPos += Vector3.up;
        if (controls.SwipeDown) nextPos += Vector3.down;

        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);

        if (controls.Tap)
        {
            Debug.Log("Tap");
        }
    }
}
