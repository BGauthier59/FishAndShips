using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class TutorialEventManager : NetworkMonoSingleton<TutorialEventManager>
{
    [SerializeField] private int tutorialStep, stepCompletion;
    public int playerNb;
    [SerializeField] private TMP_Text completionText, instructionText;
    [SerializeField] private Animation completionAnim, instructionAnim;
    [SerializeField] private GameObject[] arrows;

    [SerializeField] private ShrimpWorkshop[] shrimpWorkshops;
    [SerializeField] private Workshop[] fireWorkshops;
    [SerializeField] private ReparationWorkshop[] reparationWorkshops;
    [SerializeField] private SeriesWorkshop[] cannonWorkshops;

    private bool isRunning;

    [SerializeField] private RandomEvent[] stepEvents;

    private RandomEvent lastSoftEvent;
    private RandomEvent lastHardEvent;

    private float durationBeforeNextEvent, timer;
    private uint softEventCount, totalEventRunningCount;

    private bool checkTimer;
    [SerializeField] private GridFloorNotWalkable notWalkable;

    public void StartGameLoop()
    {
        // Récupérer les data du SO
        playerNb = GameManager.instance.players.Count;
        StartStep(0);
    }

    private void StartStep(int step)
    {
        stepCompletion = 0;
        completionText.text = "0 / " + playerNb;
        stepEvents[step].StartEvent();
        instructionAnim.Play("InstructionsTuto");
        switch (step)
        {
            case 0:
                instructionText.text = "Defeat all shrimps";
                break;
            case 1:
                instructionText.text = "Put out all fires";
                break;
            case 2:
                instructionText.text = "Repair all holes";
                break;
            case 3:
                instructionText.text = "Fire all canons";
                break;
        }
    }

    public void UpdateGameLoop()
    {
        switch (tutorialStep)
        {
            case 0:
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (i - 4 < playerNb && i - 4 >= 0)
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.one,
                            Time.deltaTime * 5);
                    else
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.zero,
                            Time.deltaTime * 5);
                }

                break;
            case 1:
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (i < playerNb)
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.one,
                            Time.deltaTime * 5);
                    else
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.zero,
                            Time.deltaTime * 5);
                }

                break;
            case 2:
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (i - 4 < playerNb && i - 4 >= 0 || i == 8 || i == 9 && playerNb > 2)
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.one,
                            Time.deltaTime * 5);
                    else
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.zero,
                            Time.deltaTime * 5);
                }

                break;
            case 3:
                for (int i = 0; i < arrows.Length; i++)
                {
                    if (i < playerNb || i == 10 || i == 11 && playerNb > 2)
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.one,
                            Time.deltaTime * 5);
                    else
                        arrows[i].transform.localScale = Vector3.Lerp(arrows[i].transform.localScale, Vector3.zero,
                            Time.deltaTime * 5);
                }

                break;
        }
    }
    
    public async void AddCompletionCount()
    {
        stepCompletion++;
        completionText.text = stepCompletion + " / " + playerNb;
        completionAnim.Play("CompletionTuto");
        if (stepCompletion >= playerNb)
        {
            // Next Step

            await Task.Delay(1000);
            tutorialStep++;
            if (tutorialStep >= 4)
            {
                // Tutorial Finished
                if (NetworkManager.Singleton.IsHost) GameManager.instance.GameEnds(true);
                return;
            }

            StartStep(tutorialStep);
        }
    }
    
    public ShrimpWorkshop GetShrimpWorkshop(int index)
    {
        return shrimpWorkshops[index];
    }

    public Workshop GetFireWorkshop(int index)
    {
        return fireWorkshops[index];
    }

    public ReparationWorkshop GetReparationWorkshop(int index)
    {
        return reparationWorkshops[index];
    }

    public SeriesWorkshop GetCannonWorkshop(int index)
    {
        return cannonWorkshops[index];
    }
    
    public GridFloorNotWalkable GetNotWalkable()
    {
        return notWalkable;
    }
}