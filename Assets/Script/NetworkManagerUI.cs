using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;

public class RelayNetworkManagerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private InputField joinCodeInput;
    [SerializeField] private Text statusText;
    [SerializeField] private Text joinCodeText;
    [SerializeField] private GameObject uiRoot;

    private const int MAX_CONNECTIONS = 4;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHostWithRelay);
        clientButton.onClick.AddListener(StartClientWithRelay);
    }

    private async void Start()
    {
        statusText.text = "Инициализация Unity Services...";

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            statusText.text = "Готово";
        }
        catch (Exception e)
        {
            statusText.text = "Ошибка инициализации Unity Services";
            Debug.LogException(e);
        }
    }

    // ===================== HOST =====================
    private async void StartHostWithRelay()
    {
        try
        {
            statusText.text = "Создаём Relay Allocation...";

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);

            // Настраиваем транспорт
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayEndpoint = allocation.ServerEndpoints[0];

            transport.SetRelayServerData(
                relayEndpoint.Host,
                (ushort)relayEndpoint.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                null, // hostConnectionData у хоста нет
                true // DTLS
            );

            // Генерируем Join Code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = $"Join Code: {joinCode}";
            statusText.text = "Хост запущен! Поделись Join Code с другом.";

            NetworkManager.Singleton.StartHost();
            uiRoot.SetActive(false);
        }
        catch (Exception e)
        {
            statusText.text = "Ошибка хоста: " + e.Message;
            Debug.LogException(e);
        }
    }

    // ===================== CLIENT =====================
    private async void StartClientWithRelay()
    {
        string joinCode = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(joinCode))
        {
            statusText.text = "Введите Join Code!";
            return;
        }

        try
        {
            statusText.text = "Подключаемся к Relay...";

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayEndpoint = allocation.ServerEndpoints[0];

            transport.SetRelayServerData(
                relayEndpoint.Host,
                (ushort)relayEndpoint.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                true // DTLS
            );

            NetworkManager.Singleton.StartClient();
            statusText.text = "Подключились к хосту!";
            uiRoot.SetActive(false);
        }
        catch (RelayServiceException ex)
        {
            if (ex.Message.Contains("Not Found"))
                statusText.text = "Join Code не найден или истёк. Попросите хоста создать новый!";
            else
                statusText.text = "Ошибка подключения: " + ex.Message;

            Debug.LogException(ex);
        }
        catch (Exception e)
        {
            statusText.text = "Ошибка подключения: " + e.Message;
            Debug.LogException(e);
        }
    }
}