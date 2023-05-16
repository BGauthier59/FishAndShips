using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CinematicCanvasManager : MonoSingleton<CinematicCanvasManager>
{
    [SerializeField] private Animation cinematicAnimation;
    [SerializeField] private AnimationClip introductionCinematicClip;
    [SerializeField] private AnimationClip endCinematicClip;

    public async UniTask IntroductionCinematic()
    {
        // Todo - cinématique d'introduction
        CanvasManager.instance.DisplayCanvas(CanvasType.IntroductionCinematicCanvas);
        cinematicAnimation.Play(introductionCinematicClip.name);
        //Thread.Sleep((int)(introductionCinematicClip.length * 1000));
        await Task.Delay((int)(introductionCinematicClip.length * 1000));
        CanvasManager.instance.DisplayCanvas(CanvasType.ControlCanvas, CanvasType.TimerCanvas);
    }

    public async UniTask EndCinematic()
    {
        // Todo - cinématique de fin
        CanvasManager.instance.DisplayCanvas(CanvasType.EndCinematicCanvas);
        cinematicAnimation.Play(endCinematicClip.name);
        await Task.Delay((int)(endCinematicClip.length * 1000));
    }
}