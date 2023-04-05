using System;
using UnityEngine;
using UnityEngine.Serialization;

public class WoodPlate : MonoBehaviour
{
    public bool useSwipeMode = false;
    public bool hasBeenMoved = false;
    public LayerMask layerWood;
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float swipeDistanceThreshold = 50f;
    private bool isRotating;
    private bool isScaling;
    private bool isDragging;

    private Vector2 dragOffset; 
    private Vector3 lastDragPos;
    
    //Position
    private Vector2 initialTouch1Pos;
    private Vector2 initialTouch2Pos;
    private Vector3 initialScale;
    
    private Collider col;
    private Material mat;
    
    private void Start()
    {
        col = GetComponent<Collider>();
        AndroidUtils.CreateCurrentActivity();
    }

    void Update()
    {
        switch (Input.touchCount)
        {
            // Check for one finger touch
            case 1 when useSwipeMode:
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    // Check for touch began
                    case TouchPhase.Began:
                        touchStartPos = touch.position;
                        break;
                    // Check for touch ended
                    case TouchPhase.Ended:
                    {
                        touchEndPos = touch.position;

                        // Check for swipe distance threshold
                        if (Vector2.Distance(touchStartPos, touchEndPos) > swipeDistanceThreshold)
                        {
                            Vector2 swipeDirection = touchEndPos - touchStartPos;
                            // Move object in swipe direction
                            transform.position += new Vector3(swipeDirection.x, swipeDirection.y, 0) * 0.01f;
                        }
                        break;
                    }
                }
                break;
            }
            // Check for one finger touch in drag mode
            case 1 when !useSwipeMode:
            {
                Touch touch = Input.GetTouch(0);

                // Check for touch began
                if (touch.phase == TouchPhase.Began || Input.GetMouseButton(0))
                {
                    // Check if touch is inside object bounds
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 10f, layerWood) && hit.transform == transform)
                    {
                        isDragging = true;
                        lastDragPos = touch.position;
                    }
                }

                // Check for touch moved
                if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    Vector3 touchPos = touch.position;
                    Vector2 dragDelta = touchPos - lastDragPos;
                    Vector3 newPos = transform.position + new Vector3(dragDelta.x, dragDelta.y, 0) * 0.02f;
                    transform.position = newPos;
                    lastDragPos = touch.position;
                    hasBeenMoved = true;   
                }

                // Check for touch ended
                if (touch.phase == TouchPhase.Ended)
                {
                    isDragging = false;
                    if (!hasBeenMoved) {
                        col.enabled = false;
                    }
                }
                break;
            }
            // Check for two finger touch
            case 2 when isDragging:
            {
                // Check for touch began
                if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    isRotating = true;
                    //isScaling = true;
                
                    initialTouch1Pos = Input.GetTouch(0).position;
                    initialTouch2Pos = Input.GetTouch(1).position;
                    initialScale = transform.localScale;
                }

                // Check for touch ended
                if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    isRotating = false;
                    //isScaling = false;
                }

                // Rotate object
                if (isRotating)
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    Vector2 previousTouchPosition1 = touch1.position - touch1.deltaPosition;
                    Vector2 previousTouchPosition2 = touch2.position - touch2.deltaPosition;

                    Vector2 previousTouchDelta = previousTouchPosition1 - previousTouchPosition2;
                    Vector2 touchDelta = touch1.position - touch2.position;

                    float angleDelta = Vector2.Angle(previousTouchDelta, touchDelta);
                    Vector3 cross = Vector3.Cross(previousTouchDelta, touchDelta);

                    if (cross.z > 0)
                    {
                        angleDelta = -angleDelta;
                    }

                    transform.Rotate(0, 0, angleDelta);
                }

                // Scale object
                //if (isScaling)
                //{
                //    Touch touch1 = Input.GetTouch(0);
                //    Touch touch2 = Input.GetTouch(1);
                //
                //    Vector2 currentTouch1Pos = touch1.position;
                //    Vector2 currentTouch2Pos = touch2.position;
                //
                //    float touchDeltaMagnitude = (currentTouch1Pos - currentTouch2Pos).magnitude;
                //    float initialTouchDeltaMagnitude = (initialTouch1Pos - initialTouch2Pos).magnitude;
                //    float scaleFactor = touchDeltaMagnitude / initialTouchDeltaMagnitude;
                //
                //    transform.localScale = initialScale * scaleFactor;
                //}
                break;
            }
        }
    }
}
