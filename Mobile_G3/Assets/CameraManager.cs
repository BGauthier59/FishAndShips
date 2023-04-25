using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Transform camera,holdTransform,deckTransform;

    public void MoveCamToDeck()
    {
        camera.transform.position = deckTransform.position;
    }
    
    public void MoveCamToHold()
    {
        camera.transform.position = holdTransform.position;
    }
}
