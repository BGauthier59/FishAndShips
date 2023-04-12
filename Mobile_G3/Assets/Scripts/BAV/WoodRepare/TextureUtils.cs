using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class TextureUtils
{
    /// <param name="texture"> <paramref name="Texture we want to read"/> </param>
    public static List<Color> ReadTexturePixels(Texture2D texture)
    {
        List<Color> pixelColors = new List<Color>();
        // Loop through all pixels in the texture and store their colors in the list
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y);
                pixelColors.Add(pixelColor);
            }
        }
        return pixelColors;
    }

    /// <param name="texture"> <paramref name="Texture we want to read"/> </param>
    /// <param name="channel"> <paramref name="The channel we want to save"/> </param>
    public static List<float> readTexturePixelChannelList(Texture2D texture, int channel = 0)
    {
        List<float> pixelColors = new List<float>();
        // Loop through all pixels in the texture and store their colors in the list
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y);
                switch (channel)
                {
                    case 0:
                        pixelColors.Add(pixelColor.r);
                        break;
                    case 1:
                        pixelColors.Add(pixelColor.g);
                        break;
                    case 2:
                        pixelColors.Add(pixelColor.b);
                        break;
                    case 3:
                        pixelColors.Add(pixelColor.a);
                        break;
                    case 4:
                        pixelColors.Add(pixelColor.grayscale);
                        break;
                }

            }
        }
        return pixelColors;
    }
    
    /// <param name="texture"> <paramref name="Texture we want to read"/> </param>
    /// <param name="textureSize"> <paramref name="Size of the Texture"/> </param>
    /// <param name="channel"> <paramref name="The channel we want to save"/> </param>
    public static float[] ReadTexturePixelChannelArray(Texture2D texture, int textureSize, int channel = 0)
    {
        float[] pixelColors = new float[textureSize * textureSize];
        int index = 0;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y);
                switch (channel)
                {
                    case 0:
                        pixelColors[index] = pixelColor.r;
                        break;
                    case 1:
                        pixelColors[index] = pixelColor.g;
                        break;
                    case 2:
                        pixelColors[index] = pixelColor.b;
                        break;
                    case 3:
                        pixelColors[index] = pixelColor.a;
                        break;
                    case 4:
                        pixelColors[index] = pixelColor.grayscale;
                        break;
                }
                index++;
            }
        }
        return pixelColors;
    }
    

    //GenerateTexture From Pixel Array
    /// <param name="pixelColors"> <paramref name="Pass the Color Channel List who contain Color Value"/> </param>
    /// <param name="textureWidth"> <paramref name="Width of the texture we create"/> </param>
    /// <param name="textureHeight"> <paramref name="Height of the texture we create"/> </param>
    public static void GenerateTextureFromColorArray(List<Color> pixelColors, int textureWidth, int textureHeight)
    {
        Texture2D generatedTexture = new Texture2D(textureWidth, textureHeight);
        generatedTexture.SetPixels(pixelColors.ToArray());
        generatedTexture.Apply();
    }
    
    //Genreate Texture from Color Channel you chose
    /// <param name="channelValues"> <paramref name="Pass the Color Channel List who contain channel value"/> </param>
    /// <param name="textureWidth"> <paramref name="Width of the texture we create"/> </param>
    /// <param name="textureHeight"> <paramref name="Height of the texture we create"/> </param>
    /// <param name="channel"> <paramref name="Channel we use to contain the channelValuesList"/> </param>
    public static void GenerateTextureFromColorChannel(List<float> channelValues, int textureWidth, int textureHeight, int channel = 0)
    {
        Texture2D generatedTexture = new Texture2D(textureWidth, textureHeight);
        Color[] colors = new Color[channelValues.Count];
        for (int i = 0; i < channelValues.Count; i++)
        {
            colors[i] = new Color(channelValues[i], 0f,0f,0f);
        }
        generatedTexture.SetPixels(colors);
        generatedTexture.Apply();
    }
    
    /// <param name="pixelColors"> <paramref name="Pass the Color Channel List who contain channel value"/> </param>
    /// <param name="textureWidth"> <paramref name="Width of the texture we create"/> </param>
    /// <param name="textureHeight"> <paramref name="Height of the texture we create"/> </param>
    /// <param name="fileName"> <paramref name="The name Of the texture we save"/> </param>
    public static void GenerateTextureAndSaveAsPNG(List<Color> pixelColors, int textureWidth, int textureHeight, string fileName)
    {
        // Generate a new texture from the pixel colors list
        Texture2D generatedTexture = new Texture2D(textureWidth, textureHeight);
        generatedTexture.SetPixels(pixelColors.ToArray());
        generatedTexture.Apply();

        // Save the texture as a PNG file
        string filePath = Application.persistentDataPath + "/" + fileName;
        File.WriteAllBytes(filePath, generatedTexture.EncodeToPNG());
    }
    
    /// <param name="pixelColors"> <paramref name="Pass the Color Channel List who contain channel value"/> </param>
    /// <param name="textureWidth"> <paramref name="Width of the texture we create"/> </param>
    /// <param name="textureHeight"> <paramref name="Height of the texture we create"/> </param>
    /// <param name="fileName"> <paramref name="The name Of the texture we save"/> </param>
    /// <param name="displayImage"> <paramref name="Image UI where we display it"/> </param>
    public static void GenerateTextureAndDisplay(List<Color> pixelColors, int textureWidth, int textureHeight, string fileName, RawImage displayImage)
    {
        // Generate a new texture from the pixel colors list
        Texture2D generatedTexture = new Texture2D(textureWidth, textureHeight);
        generatedTexture.SetPixels(pixelColors.ToArray());
        generatedTexture.Apply();

        // Save the texture as a PNG file
        string filePath = Application.persistentDataPath + "/" + fileName;
        File.WriteAllBytes(filePath, generatedTexture.EncodeToPNG());

        // Display the texture in the UI RawImage component
        displayImage.texture = generatedTexture;
    }
}
