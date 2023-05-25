using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private Animation animation;
    
    [SerializeField] private AnimationClip actionClip, jumpClip, idleClip;
    [SerializeField] private Sprite playerSprite;

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

    public Sprite GetSprite()
    {
        return playerSprite;
    }
}
