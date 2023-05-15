using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task DisplayTutorial(TutorialSO data, int index)
    {
        tutorialImage.sprite = data.tutorials[index].image;
        tutorialText.text = data.tutorials[index].text;
        
        displayTutorialAnim.gameObject.SetActive(true);
        displayTutorialAnim.Play(displayTutorialClip.name);

        await Task.Delay((int) (1000 * displayTutorialClip.length));
    }

    public async Task DisableTutorial()
    {
        displayTutorialAnim.Play(disableTutorialClip.name);
        await Task.Delay((int) (1000 * disableTutorialClip.length));

        displayTutorialAnim.gameObject.SetActive(false);
    }
}
