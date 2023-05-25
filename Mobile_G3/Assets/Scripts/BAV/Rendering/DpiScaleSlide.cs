using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DpiScaleSlide : MonoBehaviour
{
    public Slider slider;
    public TMP_Text textGUI;

    private float minDpi = 20f; // Minimum DPI value
    private float maxDpi = 420f; // Maximum DPI value
    private float currentDPIScale; // Current DPI scale

    private void Start()
    {
        currentDPIScale = 150f;
    }

    public void OnSliderValueChanged(float value)
    {
        value = currentDPIScale;
        currentDPIScale = Mathf.Lerp(minDpi, maxDpi, value); // Interpolate between min and max DPI based on the slider value
        ApplyDPIScale();
    }

    public void ApplyDPIScale()
    {
        // Calculate the new screen resolution based on the current DPI scale
        float scaleFactor = currentDPIScale / Screen.dpi;
        int newWidth = Mathf.RoundToInt(Screen.currentResolution.width * scaleFactor);
        int newHeight = Mathf.RoundToInt(Screen.currentResolution.height * scaleFactor);

        // Change the screen resolution and apply the new settings
        Screen.SetResolution(newWidth, newHeight, Screen.fullScreenMode);

        // Update the text displaying the current DPI scale
        textGUI.text = currentDPIScale.ToString();
        Debug.Log(currentDPIScale);
    }
}