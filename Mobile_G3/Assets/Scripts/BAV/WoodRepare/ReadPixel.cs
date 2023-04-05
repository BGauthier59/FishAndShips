using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadPixel : MonoBehaviour
{
    public Texture2D tex1;
    public Texture2D tex2;

    public List<Color> pixelColorTexture1;
    public List<Color> pixelColorTexture2;
    
    public List<float> pixelColorRedTexture1;
    public List<float> pixelColorRedTexture2;

    public List<float> combineTexture;

    // Start is called before the first frame update
    void Start()
    {
        pixelColorTexture1 = TextureUtils.ReadTexturePixels(tex1);
        pixelColorTexture2 = TextureUtils.ReadTexturePixels(tex2);
        
        pixelColorRedTexture1 = TextureUtils.readTexturePixelChannelList(tex1);
        pixelColorRedTexture2 = TextureUtils.readTexturePixelChannelList(tex2);
        for (int i = 0; i < pixelColorRedTexture1.Count; i++)
        {
            combineTexture.Add(pixelColorRedTexture1[i] * pixelColorRedTexture2[i]);
        }
    }
}
