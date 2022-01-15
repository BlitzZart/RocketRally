using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NW_PlayerScript : NetworkBehaviour
{
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

    private void OnPlayerDied()
    {
        print("Player Died 4 real");

        if (NetworkManager.Singleton.IsServer)
        {
            KillPlayerClientRpc();
        }
    }

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

    public void FireNetworkedRocket(Vector3 pos, Vector3 rot, float initVelocity, ulong ownerId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        if (NetworkManager.Singleton.IsServer)
        {
            FireClientRpc(pos, rot, initVelocity, ownerId);
        }
        else if (m_netObj.IsLocalPlayer)
        {
            FireServerRpc(pos, rot, initVelocity, ownerId);
        }
    }

    [ServerRpc]
    public void RevivePlayerServerRpc()
    {
        m_health.Revive();
    }

    [ClientRpc]
    public void KillPlayerClientRpc()
    {
        m_fpsCtrl.KillPlayer();
    }

    [ClientRpc]
    public void FireClientRpc(Vector3 pos, Vector3 rot, float initVelocity, ulong ownerId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        r.Fire(initVelocity, ownerId);
        NetworkObject no = r.GetComponent<NetworkObject>();
        no.Spawn(true);
    }

    [ServerRpc]
    public void FireServerRpc(Vector3 pos, Vector3 rot, float initVelocity, ulong ownerId)
    {
        //print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        r.Fire(initVelocity, ownerId);
        NetworkObject no = r.GetComponent<NetworkObject>();
        no.Spawn(true);
    }

    public void Detonate(Vector3 pos, float maxRange, float maxDamage)
    {
        print("Detonate");
        if (NetworkManager.Singleton.IsServer)
        {
            DetonateClientRpc(pos, maxRange, maxDamage);
            // with zero damage and zero range to visualize on server
            // pointless on headless sever!
            // TODO: remove in final version
            m_detonator.SpawnDetonation(pos, maxRange, maxDamage);
        }
        else if (m_netObj.IsLocalPlayer)
        {
            DetonateServerRpc(pos, maxRange, maxDamage);
        }
    }

    [ClientRpc]
    public void DetonateClientRpc(Vector3 pos, float maxRange, float maxDamage)
    {
        m_detonator.SpawnDetonation(pos, maxRange, maxDamage);
    }

    [ServerRpc]
    public void DetonateServerRpc(Vector3 pos, float maxRange, float maxDamage)
    {
        m_detonator.SpawnDetonation(pos, maxRange, maxDamage);
    }
}
