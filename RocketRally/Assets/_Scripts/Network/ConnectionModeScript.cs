using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;


//#if ENABLE_RELAY_SERVICE
//using System;
//using Unity.Services.Core;
//using Unity.Services.Authentication;
//#endif

/// <summary>
/// Used in tandem with the ConnectModeButtons prefab asset in test project
/// </summary>
public class ConnectionModeScript : MonoBehaviour
{
    private string UNSET_STRING = "#^*unset_string";

    public static string PlayerName = "#^*unset_string";
    public string PlayerUID = string.Empty;

    [SerializeField]
    private Camera m_menuCamera;

    [SerializeField]
    private GameObject m_ConnectionModeButtons;

    [SerializeField]
    private GameObject m_AuthenticationButtons;

    [SerializeField]
    private GameObject m_JoinCodeInput;

    [SerializeField]
    private int m_MaxConnections = 10;

    //private CommandLineProcessor m_CommandLineProcessor;

    [HideInInspector]
    public string RelayJoinCode { get; set; }

    public delegate void OnNotifyConnectionEventDelegateHandler();

    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventServer;
    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventHost;
    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventClient;

    private IEnumerator WaitForNetworkManager()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            try
            {
                if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
                {
                    m_ConnectionModeButtons.SetActive(false);
                    //m_CommandLineProcessor.ProcessCommandLine();
                    break;
                }
            }
            catch { }
        }
        yield return null;
    }

    /// <summary>
    /// Check whether we are even using UnityTransport and
    /// if so whether it is using the RelayUnityTransport
    /// </summary>
    private bool HasRelaySupport()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (unityTransport != null && unityTransport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            return true;
        }
        return false;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //If we have a NetworkManager instance and we are not listening and m_ConnectionModeButtons is not null then show the connection mode buttons
        if (m_ConnectionModeButtons && m_AuthenticationButtons)
        {

            if (HasRelaySupport())
            {
                m_JoinCodeInput.SetActive(true);
                //If Start() is called on the first frame update, it's not likely that the AuthenticationService is going to be instantiated yet
                //Moved old logic for this out to OnServicesInitialized
                m_ConnectionModeButtons.SetActive(false);
                m_AuthenticationButtons.SetActive(true);
            }
            else
            {

                m_JoinCodeInput.SetActive(false);
                m_AuthenticationButtons.SetActive(false);
                m_ConnectionModeButtons.SetActive(NetworkManager.Singleton && !NetworkManager.Singleton.IsListening);
            }
        }

        if (PlayerName != UNSET_STRING)
        {
            TMP_InputField n = GetComponentInChildren<TMP_InputField>();

            if (n != null)
            {
                n.text = PlayerName;
            }
        }
    }

    private void OnServicesInitialized()
    {
        if (HasRelaySupport())
        {
            m_JoinCodeInput.SetActive(true);
            //m_ConnectionModeButtons.SetActive(false || AuthenticationService.Instance.IsSignedIn);
            //m_AuthenticationButtons.SetActive(NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && !AuthenticationService.Instance.IsSignedIn);
        }
    }

    /// <summary>
    /// Handles starting netcode in server mode
    /// </summary>
    public void OnStartServerButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (HasRelaySupport())
            {
                //StartCoroutine(StartRelayServer(StartServer));
            }
            else
            {
                Debug.Log("OnStartServerButton");
                StartServer();
            }
        }

        // TODO: only if headless?
        //Destroy(m_menuCamera);
    }

    // if setIp is false - 127.0.0.1 will be used
    public void StartServer(bool setIp, string ip)
    {
        StartCoroutine(StartServerWhenReady(setIp, ip));
    }

    public void SetIp(string ip)
    {
        StartCoroutine(SetIpWhenReady(ip));
    }

    private IEnumerator SetIpWhenReady(string ip)
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        UnityTransport ut = FindObjectOfType<UnityTransport>();
        if (ut)
        {
            ut.ConnectionData.Address = ip;
        }
    }

    private IEnumerator StartServerWhenReady(bool setIp, string ip)
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        if (setIp)
        {
            UnityTransport ut = FindObjectOfType<UnityTransport>();
            if (ut)
            {
                ut.ConnectionData.Address = ip;
            }
        }

        StartServer();
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        OnNotifyConnectionEventServer?.Invoke();
        m_ConnectionModeButtons.SetActive(false);
        print("Server Started");
    }

    /// <summary>
    /// Handles starting netcode in host mode
    /// </summary>
    public void OnStartHostButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (HasRelaySupport())
            {
                //StartCoroutine(StartRelayServer(StartHost));
            }
            else
            {
                StartHost();
            }
        }
        Destroy(m_menuCamera);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        OnNotifyConnectionEventHost?.Invoke();
        m_ConnectionModeButtons.SetActive(false);
    }

    /// <summary>
    /// Handles starting netcode in client mode
    /// </summary>
    public void OnStartClientButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (HasRelaySupport())
            {
                StartCoroutine(StartRelayClient());
            }
            else
            {
                StartClient();
            }
        }

        Destroy(m_menuCamera);
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        OnNotifyConnectionEventClient?.Invoke();

        TMP_InputField n = GetComponentInChildren<TMP_InputField>();

        if (n != null)
        {
            if (n.text == string.Empty)
            {
                PlayerName = "Pal-" + Random.Range(1000, 9999);
            }
            else
            {
                PlayerName = n.text;
            }
        }

        PlayerUID = SystemInfo.deviceUniqueIdentifier;

        m_ConnectionModeButtons.SetActive(false);
    }


    /// <summary>
    /// Coroutine that kicks off Relay SDK calls to join a Relay Server instance with a join code
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartRelayClient()
    {
#if ENABLE_RELAY_SERVICE
        m_ConnectionModeButtons.SetActive(false);

        //assumes that RelayJoinCodeInput populated RelayJoinCode prior to this
        var clientRelayUtilityTask = RelayUtility.JoinRelayServerFromJoinCode(RelayJoinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var (ipv4address, port, allocationIdBytes, connectionData, hostConnectionData, key) = clientRelayUtilityTask.Result;

        //When connecting as a client to a relay server, connectionData and hostConnectionData are different.
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(ipv4address, port, allocationIdBytes, key, connectionData, hostConnectionData);

        NetworkManager.Singleton.StartClient();
        OnNotifyConnectionEventClient?.Invoke();
#else
        yield return null;
#endif
    }

    // Will be used for Relay support when it becomes available.
    // TODO: Remove this comment once relay support is available.
#if ENABLE_RELAY_SERVICE
    /// <summary>
    /// Handles authenticating UnityServices, needed for Relay
    /// </summary>
    public async void OnSignIn()
    {
        await UnityServices.InitializeAsync();
        OnServicesInitialized();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"Logging in with PlayerID {AuthenticationService.Instance.PlayerId}");

        if (AuthenticationService.Instance.IsSignedIn)
        {
            m_ConnectionModeButtons.SetActive(true);
            m_AuthenticationButtons.SetActive(false);
        }
    }
#endif

    public void Reset()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            m_ConnectionModeButtons.SetActive(true);
        }
    }
}
