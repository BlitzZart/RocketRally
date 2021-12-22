using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public delegate void OwnerIdDelegate(ulong ownerId);
    public static event OwnerIdDelegate Spawned;

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

    // used to prespawn locally and "exchange" with net spawned later
    private bool m_localInstantiated = false;
    private ulong m_ownerId;

    private void Awake()
    {
        m_trail = Instantiate(m_trailPrefab, transform.position, Quaternion.identity).transform;



        if (!m_localInstantiated)
        {
            if (Spawned != null)
            {
                Spawned(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // dont detonate "own" if not armed and we hit ourselfes
        if (!m_isArmed && collision.gameObject == NW_PlayerScript.Instance.FpsCtrl.gameObject)
        {
            return;
        }

        Detonate(transform.position);
    }

    private void Update()
    {
        m_trail.position = transform.position;
    }

    private void OnDestroy()
    {
        if (m_localInstantiated)
        {
            Spawned -= OnSpawned;
        }
    }

    public void Fire(Vector3 initVelocity, ulong ownerId, bool locallyInstantiated = false)
    {
        print("FIRE");
        //if (ownerId == NetworkManager.Singleton.LocalClientId)
        //{
        //    gameObject.SetActive(false);
        //}

        AutoGravity ag = GetComponent<AutoGravity>();
        if (ag != null)
        {
            ag.Activate();
        }

        m_body = GetComponent<Rigidbody>();
        //print("Player vel.: " + m_netPlayer.FpsCtrl.RigidBody.velocity.magnitude);
        m_body.velocity = initVelocity;
        m_body.AddForce(transform.up * m_force, ForceMode.Impulse);


        //m_trail.transform.localScale = Vector3.one;

        // don't arm if spawned locally
        // will be destroyed when net rocked was spawned and found
        m_localInstantiated = locallyInstantiated;
        if (m_localInstantiated)
        {
            Spawned += OnSpawned;
            m_ownerId = ownerId;
        }
        else
        {
            StartCoroutine(Arm());
        }

        StartCoroutine(AutoDetonate());
    }

    private void OnSpawned(ulong ownerId)
    {
        if (ownerId == m_ownerId)
        {
            Destroy(m_trail.gameObject);
            Destroy(gameObject);
        }
    }

    private void Detonate(Vector3 pos)
    {
        StopAllCoroutines();

        NW_PlayerScript.Instance.Detonate(pos, m_maxRange, m_maxDamage);

        Destroy(m_trail.gameObject, 2.0f);
        Destroy(gameObject);
    }

    private IEnumerator Arm()
    {
        yield return new WaitForSeconds(0.02f); // arm delayed

        m_isArmed = true;
    }

    private IEnumerator AutoDetonate()
    {
        yield return new WaitForSeconds(m_maxLifetime);
        Detonate(transform.position);
    }
}
