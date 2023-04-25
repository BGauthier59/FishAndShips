using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DEBUG_SetCameraPos : MonoBehaviour
{
    public Transform camera;

    [Serializable]
    public struct PosRot
    {
        public Vector3 pos;
        public Vector3 rot;
    }

    public PosRot[] posRots;
    private int currentIndex;

    public TMP_Text posText;
    public TMP_Text rotText;

    public void OnCameraSetPositionAndRotation()
    {
        currentIndex++;
        if (currentIndex == posRots.Length) currentIndex = 0;

        camera.position = posRots[currentIndex].pos;
        camera.eulerAngles = posRots[currentIndex].rot;

        posText.text = $"Pos : {posRots[currentIndex].pos}";
        rotText.text = $"Rot : {posRots[currentIndex].rot}";
    }

    public PosRot boatUp;
    public PosRot boatDown;
    private PosRot noBoat;

    private void Start()
    {
        noBoat = new PosRot()
        {
            pos = camera.position,
            rot = camera.eulerAngles
        };
    }

    public void OnSetBoatUpCamera()
    {
        camera.DOMove(boatUp.pos, 1);
        camera.DORotate(boatUp.rot, 1);

       // camera.position = boatUp.pos;
      //  camera.eulerAngles = boatUp.rot;
    }
    
    public void OnSetBoatDownCamera()
    {
        camera.DOMove(boatDown.pos, 1);
        camera.DORotate(boatDown.rot, 1);
    }
    
    public void OnSetNoBoatCamera()
    {
        camera.DOMove(noBoat.pos, 1);
        camera.DORotate(noBoat.rot, 1);
    }
}
