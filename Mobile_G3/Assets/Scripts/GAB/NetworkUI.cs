using UnityEngine;


public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        ConnectionManager.instance.ConnectAsHost();
    }

    public void StartClient()
    {
        ConnectionManager.instance.ConnectAsClient();
    }

    public void OnSetIp(string ip)
    {
        ConnectionManager.instance.SetIP(ip);
    }
}