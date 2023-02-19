using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircularSwipeManager : MonoBehaviour
{
    [SerializeField] private Transform centralPoint;

    private Vector2 startTouch, currentTouch;
    private Vector2 startVector, currentVector, lastVector;
    private bool isDraging;

    private float currentAngle;
    private int circleCount;

    [UsedImplicitly]
    public void OnTapOnScreen(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isDraging = true;
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startVector = startTouch - (Vector2) centralPoint.position;
            lastVector = startVector;

            // Todo - Should verify if magnitude if big enough

            startVector.Normalize();
        }
        else if (ctx.canceled)
        {
            isDraging = false;
            startTouch = currentTouch = startVector = currentVector = lastVector = Vector2.zero;
            currentAngle = 0;
        }
    }

    private void Update()
    {
        Debug.DrawRay(centralPoint.position, startVector, Color.green);
        if (isDraging)
        {
            currentTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentVector = currentTouch - (Vector2) centralPoint.position;

            // Todo - Should verify if magnitude if big enough

            currentVector.Normalize();

            var angleGap = Vector2.Angle(lastVector, currentVector);
            
            var last2float3 = new float3(lastVector.x, lastVector.y, 0);
            var current2float3 = new float3(currentVector.x, currentVector.y, 0);
            
            var cross = math.cross(last2float3, current2float3);

            if (cross.z > 0)
            {
                currentAngle += angleGap;
            }
            else
            {
                currentAngle -= angleGap;
            }

            if (currentAngle > 360)
            {
                circleCount++;
                currentAngle -= 360;
            }
            else if(currentAngle < 0 && circleCount != 0)
            {
                circleCount--;
                currentAngle += 360;
            }
            
            lastVector = currentVector;
        }

        Debug.DrawRay(centralPoint.position, currentVector, Color.yellow);
    }

    /*    
    Comment coder le swipe circulaire ?

    -Définir un point central du cercle

    -Quand on touche, récupérer les position de l’écran

    -Vecteur Point Central / Position du toucher, Vecteur Point Central / Position current (mouse Position)

    -Calculer l’angle entre ces deux vecteurs normalisés, et ajouter l’écart à une variable pour incrémenter.
        
    */
}