using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private Animation animation;
    
    [SerializeField] private AnimationClip actionClip, jumpClip, idleClip;

    public void PlayActionAnimation()
    {
        animation.Play(actionClip.name);
    }

    public void PlayJumpAnimation()
    {
        animation.Play(jumpClip.name);
    }

    public void PlayIdleAnimation()
    {
        animation.Play(idleClip.name);
    }
}
