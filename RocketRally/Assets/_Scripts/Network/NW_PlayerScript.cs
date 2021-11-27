using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NW_PlayerScript : MonoBehaviour
{
    private FPS_Controller m_fpsCtrl;
    private NetworkObject m_netObj;
    private void Awake()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        Debug.Log("Waiting for NetworkManager");
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        Debug.Log("NetworkManager initialized");

        m_netObj = GetComponent<NetworkObject>();
        m_fpsCtrl = GetComponent<FPS_Controller>();


        if (!m_netObj.IsOwner)
        {
            m_fpsCtrl.Head.enabled = false;
        }
    }
}
