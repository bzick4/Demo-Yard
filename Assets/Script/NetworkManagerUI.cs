using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    // [SerializeField] private Button _startHostButton;
    // [SerializeField] private Button _startClientButton;
    // [SerializeField] private Button _startServerButton;
    [SerializeField] private GameObject _StartMenuUI;

    // private void Awake()
    // {
    //     _startHostButton.onClick.AddListener(()   => { NetworkManager.Singleton.StartHost();   CloseMenu(); });
    //     _startClientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); CloseMenu(); });
    //     _startServerButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); CloseMenu(); });
    // }

    private void CloseMenu()
    {
        _StartMenuUI.SetActive(false);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        CloseMenu();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        CloseMenu();
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        CloseMenu();
    }
}
