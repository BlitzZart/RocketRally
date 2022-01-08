using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Rocket : NetworkBehaviour
{
    private Rigidbody m_body;
    private float m_force = 75.0f;
    private float m_maxLifetime = 3.0f;
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
    [SerializeField] private NetworkVariable<ulong> m_ownerId = new NetworkVariable<ulong>();
    private bool m_deactivatedNetRocket = false;
    private bool m_useNetRocket = false;

    private void Awake()
    {
        m_trail = Instantiate(m_trailPrefab, transform.position, Quaternion.identity).transform;

        // take care of stray rockets
        //if (!NetworkManager.Singleton.IsServer)
        //{
        //    StartCoroutine(DestroyOldRocket());
        //}
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
        if (!m_useNetRocket)
        {
            if (!m_deactivatedNetRocket &&
                !m_localInstantiated &&
                !NetworkManager.Singleton.IsServer)
            {
                // we swawned this rockert
                if (m_ownerId.Value == NW_PlayerScript.Instance.OwnerClientId)
                {
                    m_deactivatedNetRocket = true;

                    Renderer r = GetComponent<Renderer>();
                    if (r)
                    {
                        r.enabled = false;
                    }

                    Collider c = GetComponent<Collider>();
                    if (c)
                    {
                        c.enabled = false;
                    }

                    Destroy(m_trail.gameObject);
                }
                else
                {
                    // someone else shot this rocket
                    m_useNetRocket = true;
                }
            }
            else if (m_deactivatedNetRocket)
            {
                return;
            }
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

        //m_ownerId.SetDirty(true);

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

    private IEnumerator DestroyOldRocket()
    {
        yield return new WaitForSeconds(m_maxLifetime + 1.0f);

        StartCoroutine(AutoDetonate());
        if (m_trail)
        {
            Destroy(m_trail.gameObject);
        }

        Destroy(gameObject);
    }

    private IEnumerator AutoDetonate()
    {
        yield return new WaitForSeconds(m_maxLifetime);

        StopAllCoroutines();
        Detonate(transform.position);
    }
}
