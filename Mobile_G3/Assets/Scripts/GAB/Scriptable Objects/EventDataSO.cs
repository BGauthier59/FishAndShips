using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 2, fileName = "New Event Data", menuName = "Create Event Data")]
public class EventDataSO : ScriptableObject
{
    public float timerBetweenEvents;
    public float randomTimerGapBetweenEvents;
    public uint hardEventsFrequency;
    public AnimationCurve eventActivationSpeedCurve;
    //public SoftEventType[] availableSoftEvents;
    //public HardEventType[] availableHardEvents;
    public uint maxWorkshopsAtTheSameTime;
}
