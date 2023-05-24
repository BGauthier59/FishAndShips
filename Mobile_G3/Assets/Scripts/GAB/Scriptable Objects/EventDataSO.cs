using System;
using UnityEngine;

[CreateAssetMenu(order = 2, fileName = "New Event Data", menuName = "Create Event Data")]
public class EventDataSO : ScriptableObject
{
    public float timerBetweenEvents;
    public float randomTimerGapBetweenEvents;
    public uint hardEventsFrequency;
    public AnimationCurve eventActivationSpeedCurve;
    public FactorSpeed factorSpeed;
}

[Serializable]
public class FactorSpeed
{
    public float p2 = 1.33f;
    public float p3 = 1.25f;
    public float p4 = 1;
}
