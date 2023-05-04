using System;
using UnityEngine;
using TMPro;

public class CompareTextures : MonoBehaviour
{
    public Texture2D texture2D;
    public RenderTexture renderTexture;
    public TextMeshPro percentageTexture;
    public float precisionColor;

    private void Update()
    {
        CompareTexturesAndGetPercentage();
    }

    public float CompareTexturesAndGetPercentage()
    {
        // Récupérer les pixels des deux textures
        Color[] pixelsTexture2D = texture2D.GetPixels();
        Texture2D texture1 = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture1.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture1.Apply();
        Color[] pixelsRenderTexture = texture1.GetPixels();

        // Initialiser les compteurs pour les pixels blancs et noirs
        int whitePixelCount = 0;
        int blackPixelCount = 0;

        // Parcourir chaque pixel des deux textures et comparer leur position
        for (int i = 0; i < pixelsTexture2D.Length; i++)
        {
            Color pixelTexture2D = pixelsTexture2D[i];
            Color pixelRenderTexture = pixelsRenderTexture[i];

            // Si le pixel de la texture 2D est noir
            if (pixelTexture2D.r == 0 && pixelTexture2D.g == 0 && pixelTexture2D.b == 0)
            {
                blackPixelCount++;

                // Si le pixel de la render texture est blanc et est au-dessus du pixel noir de la texture 2D
                //if (pixelRenderTexture.r >= precisionColor && pixelRenderTexture.g >= precisionColor && pixelRenderTexture.b >= precisionColor && i / texture2D.width < renderTexture.height - i / texture2D.width)
                if (pixelRenderTexture.r >= precisionColor && pixelRenderTexture.g >= precisionColor && pixelRenderTexture.b >= precisionColor)
                {
                    whitePixelCount++;
                }
            }
        }

        // Calculer le pourcentage de pixels blancs
        float percentage = (float)whitePixelCount / (float)blackPixelCount * 100f;

        // Si le pourcentage est supérieur à 100, le ramener à 100
        if (percentage > 100f)
        {
            percentage = 100f;
        }

        percentageTexture.text = "Percentage of White" + percentage;

        // Retourner le pourcentage calculé
        return percentage;
    }
}