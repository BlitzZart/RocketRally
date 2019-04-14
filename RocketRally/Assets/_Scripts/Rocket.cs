﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public GameObject explosionPrefab;

    private Transform m_owner;
    private bool m_fired = false;

    public void Fire(Transform owner)
    {
        m_owner = owner;
        m_fired = true;
    }

    private void Update()
    {
        if (!m_fired)
        {
            return;
        }
        transform.Translate(Vector3.up * 20.0f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ignore your master
        if (other.transform == m_owner)
        {
            return;
        }

        GameObject ps =  Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(ps, ps.GetComponent<ParticleSystem>().main.duration);
        Destroy(gameObject);
    }
}