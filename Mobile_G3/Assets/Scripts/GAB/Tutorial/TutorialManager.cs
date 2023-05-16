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

    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text tutorialText;

    public async UniTask DisplayTutorial(TutorialSO data, int index)
    {
        tutorialImage.sprite = data.tutorials[index].image;
        tutorialText.text = data.tutorials[index].text;
        
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
