using Unity.Mathematics;
using UnityEngine;

public class FireSoftEvent : RandomEvent
{
    [SerializeField] private float sparkAnimationDuration;
    private bool isFiring;

    [SerializeField] private Transform sparkOrigin;
    [SerializeField] private Transform spark;
    [SerializeField] private float controlPoint1Height, controlPoint2Height;
    [SerializeField] private int2[] fireTargetTiles;

    public override bool CheckConditions()
    {
        if (EventsManager.instance.GetFireIndices().Length == 0) return false;
        return true;
    }

    public override void StartEvent()
    {
        
    }

    protected override void EndEvent()
    {
        
    }
}
