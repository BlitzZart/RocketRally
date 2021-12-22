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

    private void Awake()
    {
        m_instance = this;

        StartCoroutine(Initialize());
    }

    public void SetFpsController(FPS_Controller fpsCtrl)
    {
        NetworkObject netObject = fpsCtrl.GetComponent<NetworkObject>();


        if (NetworkManager.Singleton.IsServer || netObject.IsLocalPlayer)
        {
            m_fpsCtrl = fpsCtrl;
            m_netObj = netObject;
            m_gun = m_fpsCtrl.GetComponentInChildren<Gun>();
        }
        
        m_initialized = true;

        if (!netObject.IsLocalPlayer)
        {

            return;
        }

        UI_PlayerHud hud = FindObjectOfType<UI_PlayerHud>();
        hud.PlayerReady(m_fpsCtrl);

        if (!m_netObj.IsOwner)
        {
            // disable all player cameras but own
            m_fpsCtrl.Head.enabled = false;
            //    //Destroy(m_fpsCtrl.Head);
        }
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);



        NetworkObject netObject = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton.IsServer || netObject.IsLocalPlayer)
        {
            m_gun = GetComponentInChildren<Gun>();
            m_netObj = netObject;
        }
        m_initialized = true;


        m_instance = this;
        Debug.Log("NetworkManager initialized");

        m_detonator = FindObjectOfType<Detonator>();
    }

    public void FireNetworkedRocket(Vector3 pos, Vector3 rot, Vector3 initVelocity, ulong ownerId)
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

    [ClientRpc]
    public void FireClientRpc(Vector3 pos, Vector3 rot, Vector3 initVelocity, ulong ownerId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        r.Fire(initVelocity, ownerId);
        NetworkObject no = r.GetComponent<NetworkObject>();
        no.Spawn(true);
    }

    [ServerRpc]
    public void FireServerRpc(Vector3 pos, Vector3 rot, Vector3 initVelocity, ulong ownerId)
    {
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
        Rocket r = Instantiate(m_gun.m_stdRocketPrefab, pos, Quaternion.Euler(rot));
        r.Fire(initVelocity, ownerId);
        NetworkObject no = r.GetComponent<NetworkObject>();
        no.Spawn(true);
    }

    public void Detonate(Vector3 pos, float maxRange, float maxDamage)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            DetonateClientRpc(pos, maxRange, maxDamage);
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
