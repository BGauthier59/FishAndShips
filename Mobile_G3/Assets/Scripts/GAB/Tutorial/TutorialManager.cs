using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoSingleton<TutorialManager>
{
    [SerializeField] private Animation displayTutorialAnim;
    [SerializeField] private AnimationClip displayTutorialClip;
    [SerializeField] private AnimationClip disableTutorialClip;

    [SerializeField] private GameObject[] tutorials;
    [SerializeField] private GameObject waitArea;
    
    public async UniTask DisplayTutorial(TutorialSO data, int index)
    {
        waitArea.SetActive(false);
        for (int i = 0; i < tutorials.Length; i++)
        {
            if(i == data.tutorials[index]) tutorials[i].SetActive(true);
            else tutorials[i].SetActive(false);
        }
        
        displayTutorialAnim.gameObject.SetActive(true);
        displayTutorialAnim.Play(displayTutorialClip.name);

        await UniTask.Delay((int) (500 * displayTutorialClip.length));
    }

    public void DisplayWaitArea()
    {
        waitArea.SetActive(true);
    }
    
    public void DisableWaitArea()
    {
        waitArea.SetActive(false);
    }

    public async UniTask DisableTutorial()
    {
        displayTutorialAnim.Play(disableTutorialClip.name);
        await UniTask.Delay((int) (1000 * disableTutorialClip.length));

        displayTutorialAnim.gameObject.SetActive(false);
    }
}
