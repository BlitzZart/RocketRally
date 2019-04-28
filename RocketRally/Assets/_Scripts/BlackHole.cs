using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    private class Orbit
    {
        private float m_speed;
        private Transform m_transform;

        public Orbit(Transform planet)
        {
            m_speed = Random.Range(0.1f, 0.2f);
            m_transform = new GameObject().transform;
            m_transform.name = planet.name + " orbit";
            planet.SetParent(m_transform);
        }

        public void UpdateOrbit(float dt)
        {
            m_transform.Rotate(0, m_speed * dt, 0);
        }
        
    }

    private List<Planet> m_plantes;
    private List<Orbit> m_orbits;

    private void Start()
    {
        m_orbits = new List<Orbit>();
        m_plantes = PlanetManager.Instance.Plantes;
        foreach(Planet p in m_plantes)
        {
            m_orbits.Add(new Orbit(p.transform));
        }
    }

    private void Update()
    {
        foreach(Orbit orbit in m_orbits)
        {
            orbit.UpdateOrbit(Time.deltaTime);
        }
    }
}
