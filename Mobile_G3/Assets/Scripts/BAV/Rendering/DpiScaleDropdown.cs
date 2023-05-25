using UnityEngine;
using TMPro;

public class DpiScaleDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private int[] dpiOptions = { 200, 190, 180, 170, 160, 150, 140, 130, 120, 110, 100, 90, 80 };
    
    public void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < dpiOptions.Length)
        {
            float scaleFactor = dpiOptions[index] / Screen.dpi;
            int width = Mathf.RoundToInt(Screen.width * scaleFactor);
            int height = Mathf.RoundToInt(Screen.height * scaleFactor);
            Screen.SetResolution(width, height, Screen.fullScreen);
        }
        Debug.Log(Screen.dpi);
    }
}