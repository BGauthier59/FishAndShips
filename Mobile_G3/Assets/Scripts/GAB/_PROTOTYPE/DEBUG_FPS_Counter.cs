using TMPro;
using UnityEngine;

public class DEBUG_FPS_Counter : MonoBehaviour
{
    public TMP_Text text;
    void Update()
    {
        text.text = $"FPS {(1 / Time.deltaTime):F0}" ;
    }
}
