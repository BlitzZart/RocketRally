using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private Rigidbody m_body;
    private float m_force = 75.0f;
    private float m_maxLifetime = 20.0f;
    [SerializeField]
    private GameObject m_trailPrefab;
    private Transform m_trail;
    private bool m_isArmed = false;

    [SerializeField]
    private float m_maxDamage = 50;
    [SerializeField]
    private float m_maxRange = 10;

    // used to "sync" local and server rockets
    private bool m_localInstantiated = false;
    private NetworkVariable<ulong> m_ownerId = new NetworkVariable<ulong>();
    private bool m_deactivatedNetRocketVis = false;

    private void Awake()
    {
        m_trail = Instantiate(m_trailPrefab, transform.position, Quaternion.identity).transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //print("Rocket collided with: " + collision.gameObject.name);
        // dont detonate "own" if not armed and we hit ourselfes
        // if FpsCtrl is set, it's us
        if (!m_isArmed)
        {
            NetworkObject no = collision.gameObject.GetComponent<NetworkObject>();
            if (no)
            {
                if (no.NetworkObjectId == m_ownerId.Value)
                {
                    return;
                }
            }

        }


        Detonate(transform.position);
    }

    private void Update()
    {
        if (!m_deactivatedNetRocketVis &&
            !m_localInstantiated && 
            !NetworkManager.Singleton.IsServer)
        {
            m_deactivatedNetRocketVis = true;

            Renderer r = GetComponent<Renderer>();
            if (r)
            {
                r.enabled = false;
            }

            Destroy(m_trail.gameObject);
        }
        else if (m_deactivatedNetRocketVis)
        {
            return;
        }


        m_trail.position = transform.position;
    }

    public void Fire(Vector3 initVelocity, ulong ownerId, bool locallyInstantiated = false)
    {
        m_localInstantiated = locallyInstantiated;
        AutoGravity ag = GetComponent<AutoGravity>();
        if (ag != null)
        {
            ag.Activate();
        }

        m_body = GetComponent<Rigidbody>();
        m_body.velocity = initVelocity;
        m_body.AddForce(transform.up * m_force, ForceMode.Impulse);

        m_ownerId.Value = ownerId;

        StartCoroutine(Arm());

        StartCoroutine(AutoDetonate());
    }

    private void Detonate(Vector3 pos)
    {
        StopAllCoroutines();

        NW_PlayerScript.Instance.Detonate(pos, m_maxRange, m_maxDamage);

        if (m_trail)
        {
            Destroy(m_trail.gameObject, 2.0f);
        }

        Destroy(gameObject);
    }

    private IEnumerator Arm()
    {
        yield return 0;// new WaitForSeconds(0.02f); // arm delayed 

        m_isArmed = true;
    }

    private IEnumerator AutoDetonate()
    {
        yield return new WaitForSeconds(m_maxLifetime);
        Detonate(transform.position);
    }
}
