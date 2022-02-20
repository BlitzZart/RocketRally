using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AutoGravity : MonoBehaviour
{
    private Transform m_personalGravityObject; // could be player or target
    private float m_initialDistanceToGrabityObject = 0;
    private Vector3 m_initialPosition = Vector3.zero;

    private List<Planet> m_plantes;
    private Rigidbody m_body;
    private Vector3 m_gravity;

    [SerializeField] private float m_effectMultiplier = 5.0f;
    private bool m_homeActive = false;
    private Planet m_homePlanet;

    private bool m_isNetRocket = false;

    private void Start()
    {
        m_body = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (m_personalGravityObject != null)
        {
            // start applying gravity only when rocket traveled a little
            // precisely: when the rocket traveled a third of the distance of its birth the target during birth
            if (Vector3.Distance(m_initialPosition, transform.position) >=
                m_initialDistanceToGrabityObject * 0.333f)
            {
                m_gravity = (m_personalGravityObject.position - transform.position) * m_effectMultiplier;
            }
        }
        else
        {
            if (m_plantes == null)
            {
                // this is the case for rockets, which are spawned on clients by the server
                return;
            }
            foreach (Planet p in m_plantes)
            {
                if (!m_homeActive && p == m_homePlanet)
                {
                    continue;
                }
                m_gravity += (p.transform.position - transform.position) /
                    Vector3.Distance(p.transform.position, transform.position) *
                    p.mass * m_effectMultiplier;
            }
        }

        m_body.AddForce(m_gravity);
        m_gravity = Vector3.zero;
    }

    private IEnumerator ActivateDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_homeActive = true;
    }

    public void Activate(Planet homePlanet = null, float delayGravityActivasion = 1.0f)
    {
        if (m_personalGravityObject)
        {
            m_initialDistanceToGrabityObject = Vector3.Distance(transform.position, m_personalGravityObject.position);
            m_initialPosition = transform.position;
            m_plantes = new List<Planet>();
        }
        else
        {
            m_plantes = PlanetManager.Instance.Plantes;
            // if we know the planet, the player is currently standing on
            // we can delay its effect specifically
            if (homePlanet != null && delayGravityActivasion > 0)
            {
                m_homePlanet = homePlanet;
                StartCoroutine(ActivateDelayed(delayGravityActivasion));
            }
            else
            {
                //m_homeActive = true;
                StartCoroutine(ActivateDelayed(delayGravityActivasion));
            }
        }
    }

    public void SetTarget(Transform target)
    {
        m_personalGravityObject = target;
    }
}
