using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float gravity = -9.81f;
    public float mass;

    private float m_massBase = 10;


    private void Start()
    {
        mass = m_massBase * transform.localScale.x;
    }

}
