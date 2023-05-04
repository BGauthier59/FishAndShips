using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConvertIP : MonoBehaviour
{
    public TextMeshProUGUI textIP;
    public TextMeshProUGUI textCodeIP;
    public TextMeshProUGUI textReturnIP;

    public string ipAdress; 
    private  string codeIpAdress;

    
    
    // Start is called before the first frame update
    
    [ContextMenu("TestIP")]
    void TestIP()
    {
        textIP.text = "IP : " + ipAdress;
        codeIpAdress = StringUtils.NumberToLetterIP(ipAdress);
        textCodeIP.text = "Code : " + codeIpAdress;
        textReturnIP.text = "Code IP Return : " + StringUtils.LetterToNumberIP(codeIpAdress);
    }
}
