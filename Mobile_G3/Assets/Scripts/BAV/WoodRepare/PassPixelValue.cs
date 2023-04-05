using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassPixelValue : MonoBehaviour
{
    public Texture2D tex;
    public float[] pixelValuesToPass;
    public Material mat;
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        pixelValuesToPass = TextureUtils.ReadTexturePixelChannelArray(tex, 16);
        if (pixelValuesToPass.Length != 0)
        {
            //mat.SetFloatArray("_PixelValues", pixelValuesToPass);
        }
    }
}
