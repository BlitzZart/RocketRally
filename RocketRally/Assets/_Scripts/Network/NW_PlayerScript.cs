using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NW_PlayerScript : NetworkBehaviour
{
    // <killedPlayer, killedBy>
    public Action<ulong, ulong> PlayerKilled;

    private static NW_PlayerScript m_instance;
    public static NW_PlayerScript Instance {
        get {
            if (m_instance == null)
            {
                Debug.LogWarning("NW_PlayerScript is not set yet!");
            }
            return m_instance;
        }
    }

    public bool Initialized { get => m_initialized; }
    private bool m_initialized;

    private Detonator m_detonator;
    public FPS_Controller FpsCtrl { get => m_fpsCtrl; }

    public bool IsLocal { get { return m_netObj.IsLocalPlayer; } }

    private FPS_Controller m_fpsCtrl;
    private NetworkObject m_netObj;
    private Gun m_gun;

    private Health m_health;
    public Health Health { get => m_health; }

    #region Unity Callbacks
    private void Awake()
    {
        StartCoroutine(Initialize());
        m_health = GetComponent<Health>();

        m_health.PlayerDied += OnPlayerDied;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_health.PlayerDied -= OnPlayerDied;
    }
    #endregion

    #region public
    public void SetFpsController(FPS_Controller fpsCtrl)
    {
        NetworkObject netObject = fpsCtrl.GetComponent<NetworkObject>();
        m_netObj = netObject;

        if (NetworkManager.Singleton.IsServer || netObject.IsLocalPlayer)
        {
            m_fpsCtrl = fpsCtrl;
            m_gun = m_fpsCtrl.GetComponentInChildren<Gun>();
        }
        
        m_initialized = true;

        if (!netObject.IsLocalPlayer)
        {
            Destroy(fpsCtrl);

            return;
        }


        m_instance = this;

        UI_PlayerHud hud = FindObjectOfType<UI_PlayerHud>();
        hud.PlayerReady(m_fpsCtrl);

        if (!m_netObj.IsOwner)
        {
            // disable all player cameras but own
            //m_fpsCtrl.Head.enabled = false;
            //    //Destroy(m_fpsCtrl.Head);
        }
    }
    private void OnPlayerDied(ulong dmgDealerId)
    {
        print("Player Died 4 real");

        if (NetworkManager.Singleton.IsServer)
        {
            KillPlayerClientRpc(dmgDealerId);
        }

        PlayerKilled?.Invoke(m_netObj.OwnerClientId, dmgDealerId);
    }
    public void FireNetworkedRocket(int rocketType, Vector3 pos, Vector3 rot, float initVelocity, ulong ownerId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (NetworkManager.Singleton.IsServer)
        {
            FireClientRpc(rocketType, pos, rot, initVelocity, ownerId);
        }
        else if (m_netObj.IsLocalPlayer)
        {
            FireServerRpc(rocketType, pos, rot, initVelocity, ownerId);
        }
    }
    public void Detonate(ulong ownerId, Vector3 pos, float maxRange, float maxDamage)
    {
        print("Detonate");
        if (NetworkManager.Singleton.IsServer)
        {
            DetonateClientRpc(ownerId, pos, maxRange, maxDamage);
            // with zero damage and zero range to visualize on server
            // pointless on headless sever!
            // TODO: remove in final version
            m_detonator.SpawnDetonation(ownerId, pos, maxRange, maxDamage);
        }
        else if (m_netObj.IsLocalPlayer)
        {
            DetonateServerRpc(ownerId, pos, maxRange, maxDamage);
        }
    }
    #endregion

    #region Network Functions
    // restart handling ---------------------------------------
    public void RestartClient()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ServerRestart();
        }

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RestartServerAndClient()
    {
        print("Client: Restart Server");

        StartCoroutine(RestartServerAndClientSequence());
    }
    private IEnumerator RestartServerAndClientSequence()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ServerRestart();
        }
        RestartServerRpc();
        yield return null;
        yield return null;

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    [ServerRpc]
    public void RestartServerRpc()
    {
        print("Server: Restart Server");
        
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ServerRestart();
        }

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // --------------------------------------------------------

    [ServerRpc]
    public void RevivePlayerServerRpc()
    {
        print("REVIVE");
        m_health.Revive();
    }
    [ClientRpc]
    public void KillPlayerClientRpc(ulong dmgDealerId)
    {
        print(NetworkManager.Singleton.LocalClientId + " was KILLED by " + dmgDealerId);

        // only kill self
        if (m_fpsCtrl)
        {
            m_fpsCtrl.KillPlayer(dmgDealerId);
        }
    }

    [ClientRpc]
    public void FireClientRpc(int rocketType, Vector3 pos, Vector3 rot, float initVelocity, ulong ownerNetId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        FireRocket(rocketType, pos, rot, initVelocity, ownerNetId); 
        //Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        //r.Fire(initVelocity, ownerNetId);
        //NetworkObject no = r.GetComponent<NetworkObject>();
        //no.Spawn(true);
    }
    [ServerRpc]
    public void FireServerRpc(int rocketType, Vector3 pos, Vector3 rot, float initVelocity, ulong ownerNetId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        FireRocket(rocketType, pos, rot, initVelocity, ownerNetId);
        //Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        //r.Fire(initVelocity, ownerNetId);
        //NetworkObject no = r.GetComponent<NetworkObject>();
        //no.Spawn(true);
    }

    private void FireRocket(int rocketType, Vector3 pos, Vector3 rot, float initVelocity, ulong ownerNetId)
    {
        Rocket r;
        if (rocketType == (int)Gun.RocketType.Gravity)
        {
            r = Instantiate(m_gun.m_gravityRocketPrefab, pos, Quaternion.Euler(rot));
        }
        else
        {
            r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        }

        r.Fire(initVelocity, ownerNetId);
        NetworkObject no = r.GetComponent<NetworkObject>();
        no.Spawn(true);
    }

    [ClientRpc]
    public void DetonateClientRpc(ulong ownerId, Vector3 pos, float maxRange, float maxDamage)
    {
        m_detonator.SpawnDetonation(ownerId, pos, maxRange, maxDamage);
    }
    [ServerRpc]
    public void DetonateServerRpc(ulong ownerId, Vector3 pos, float maxRange, float maxDamage)
    {
        m_detonator.SpawnDetonation(ownerId, pos, maxRange, maxDamage);
    }
    #endregion

    #region private
    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        NetworkObject netObject = GetComponent<NetworkObject>();
        m_netObj = netObject;
        if (NetworkManager.Singleton.IsServer || netObject.IsLocalPlayer)
        {
            m_gun = GetComponentInChildren<Gun>();

            m_instance = this;
            Debug.Log("NetworkManager initialized");
        }
        m_detonator = FindObjectOfType<Detonator>();

        m_initialized = true;
    }
    #endregion
}
