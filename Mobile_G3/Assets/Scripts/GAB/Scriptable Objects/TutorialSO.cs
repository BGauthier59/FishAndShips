using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 1, fileName = "New Tutorial", menuName = "Create Tutorial")]
public class TutorialSO : ScriptableObject
{
    [TextArea(4, 4)] public string text;
    public Sprite image;
}
