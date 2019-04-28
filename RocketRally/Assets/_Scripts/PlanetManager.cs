﻿using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : Util.Singleton<PlanetManager>
{
    private List<Planet> m_plantes;
    public List<Planet> Plantes {
        get {
            return m_plantes;
        }
    }

    private void Awake()
    {
        Planet[] ps = FindObjectsOfType<Planet>();
        m_plantes = new List<Planet>(ps);
    }
}