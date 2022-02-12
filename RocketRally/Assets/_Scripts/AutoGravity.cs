using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGravity : MonoBehaviour
{
    // TODO: maybe only use the closes planet or
    // weight it so it comes close
    // this would make rockets more predictable!!!

    private List<Planet> m_plantes;
    private Rigidbody m_body;
    private Vector3 m_gravity;

    [SerializeField] private float m_effectMultiplier = 5.0f;
    private bool m_active = false;
    private bool m_homeActive = false;
    private Planet m_homePlanet;

    private void Start()
    {
        m_body = GetComponent<Rigidbody>();
        m_plantes = PlanetManager.Instance.Plantes;       
    }

    private void FixedUpdate()
    {
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

        m_body.AddForce(m_gravity);

        m_gravity = Vector3.zero;
    }

    private IEnumerator ActivateDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_homeActive = true;
    }

    public void Activate(Planet homePlanet = null, float delayHomePlanet = 0.5f)
    {
        if (homePlanet != null && delayHomePlanet > 0)
        {
            m_homePlanet = homePlanet;
            StartCoroutine(ActivateDelayed(delayHomePlanet));
        }
        else
        {
            m_homeActive = true;
        }
        m_active = true;
    }
}
