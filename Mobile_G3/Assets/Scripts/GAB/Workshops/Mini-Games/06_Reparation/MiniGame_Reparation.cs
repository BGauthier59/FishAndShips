using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGame_Reparation : MiniGame
{
    [SerializeField] private ReparationDragAndDropManagerData data;

    [SerializeField] private Texture2D[] holeTextures;

    private Texture2D currentTexture;
    [SerializeField] private MeshRenderer renderer;
    
    public RenderTexture renderTexture;
    public TMP_Text percentageTexture;
    public float precisionColor;
    [SerializeField] [Range(0, 100)] private float minimumRateToRepair;
    private static readonly int TexHole = Shader.PropertyToID("_Tex_Hole");

    
    public override async void StartMiniGame()
    {
        base.StartMiniGame();
        SetupHole();
        WorkshopManager.instance.reparationDragAndDrop.OnSetPlank = CheckPlanks;

        await Task.Delay(WorkshopManager.instance.GetIndicatorAnimationLength());

        WorkshopManager.instance.reparationDragAndDrop.Enable(data);
        StartExecutingMiniGame();
    }

    private void SetupHole()
    {
        currentTexture = holeTextures[Random.Range(0, holeTextures.Length)];
        renderer.material.SetTexture(TexHole, currentTexture);
    }

    private void CheckPlanks()
    {
        // Todo - Implement feedbacks
        
        if (CompareTexturesAndGetPercentage() > minimumRateToRepair) WallIsFixed();
    }

    public override void ExecuteMiniGame()
    {
        WorkshopManager.instance.reparationDragAndDrop.CalculateCurrentPlankPosition();
    }

    public override void Reset()
    {
    }

    private async void WallIsFixed()
    {
        WorkshopManager.instance.reparationDragAndDrop.Disable();
        StopExecutingMiniGame();
        WorkshopManager.instance.SetVictoryIndicator();
        await Task.Delay(WorkshopManager.instance.GetVictoryAnimationLength());
        ExitMiniGame(true);
    }

    protected override async void ExitMiniGame(bool victory)
    {
        base.ExitMiniGame(victory);
    }

    public override void OnLeaveMiniGame()
    {
        WorkshopManager.instance.reparationDragAndDrop.Disable();
        StopExecutingMiniGame();
        ExitMiniGame(false);
    }

    private float CompareTexturesAndGetPercentage()
    {
        // Récupérer les pixels des deux textures
        Color[] pixelsTexture2D = currentTexture.GetPixels();
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
                if (pixelRenderTexture.r >= precisionColor && pixelRenderTexture.g >= precisionColor &&
                    pixelRenderTexture.b >= precisionColor)
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

        percentageTexture.text = "Percentage of White" + percentage.ToString("F2");

        // Retourner le pourcentage calculé
        return percentage;
    }
}