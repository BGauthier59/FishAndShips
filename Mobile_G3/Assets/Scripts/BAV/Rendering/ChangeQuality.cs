using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeQuality : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    public void SetQualityLevelDropdown(int index)
    {
        QualitySettings.SetQualityLevel(index, false);
    }
}
