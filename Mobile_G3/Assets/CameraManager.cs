using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    public Transform camera;

    public void MoveCamToDeck()
    {
        camera.transform.position = new Vector3(4.5f, 10.3f, -3.22f);
    }
    
    public void MoveCamToHold()
    {
        camera.transform.position = new Vector3(4.5f, 10.3f, 21.78f);
    }
}
