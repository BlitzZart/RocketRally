using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Rocket : NetworkBehaviour
{
    private static int rocketCnt = 0;

    private Rigidbody m_body;
    private float m_force = 25.0f;
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
        name = rocketCnt + name;
        m_trail.name = rocketCnt++ + m_trail.name;

        // take care of stray rockets
        //if (!NetworkManager.Singleton.IsServer)
        //{
        //    StartCoroutine(DestroyOldRocket());
        //}
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (m_trail)
        {
            Destroy(m_trail.gameObject, 2.0f);
        }
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
                if (no.OwnerClientId == m_ownerId.Value)
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

    public void Fire(float initVelocity, ulong ownerId, bool locallyInstantiated = false)
    {
        m_localInstantiated = locallyInstantiated;
        AutoGravity ag = GetComponent<AutoGravity>();
        if (ag != null)
        {
            ag.Activate();
        }

        m_body = GetComponent<Rigidbody>();
        m_body.velocity = transform.up * initVelocity;
        m_body.AddForce(transform.up * m_force, ForceMode.Impulse);

        m_ownerId.Value = ownerId;

        //m_ownerId.SetDirty(true);

        StartCoroutine(Arm());

        StartCoroutine(AutoDetonate());
    }

    private void Detonate(Vector3 pos)
    {
        StopAllCoroutines();

        if(!m_localInstantiated)
        {
            NW_PlayerScript.Instance.Detonate(m_ownerId.Value, pos, m_maxRange, m_maxDamage);
        }


        if (m_trail)
        {
            Destroy(m_trail.gameObject, 2.0f);
        }

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkObject no = GetComponent<NetworkObject>();
            no.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
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
