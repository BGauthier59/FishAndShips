using System;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New Tutorial", menuName = "Create Tutorial")]
public class TutorialSO : ScriptableObject
{
    public Tutorial[] tutorials;
}

[Serializable]
public struct Tutorial
{
    [TextArea(4, 4)] public string text;
    public Sprite image;
}
