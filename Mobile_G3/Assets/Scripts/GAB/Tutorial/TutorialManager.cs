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

    public async UniTask DisplayTutorial(TutorialSO data, int index)
    {
        for (int i = 0; i < tutorials.Length; i++)
        {
            if(i == data.tutorials[index]) tutorials[i].SetActive(true);
            else tutorials[i].SetActive(false);
        }
        
        displayTutorialAnim.gameObject.SetActive(true);
        displayTutorialAnim.Play(displayTutorialClip.name);

        await UniTask.Delay((int) (500 * displayTutorialClip.length));
    }

    public async UniTask DisableTutorial()
    {
        displayTutorialAnim.Play(disableTutorialClip.name);
        await UniTask.Delay((int) (500 * disableTutorialClip.length));

        displayTutorialAnim.gameObject.SetActive(false);
    }
}
