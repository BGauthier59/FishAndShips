using System;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Level", menuName = "Create Level")]
public class LevelSO : ScriptableObject
{
    public int levelIndex;
    public string sceneName;
}
