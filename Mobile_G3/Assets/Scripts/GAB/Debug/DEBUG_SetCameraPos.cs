using System;
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
}
