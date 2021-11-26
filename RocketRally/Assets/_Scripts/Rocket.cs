using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public GameObject explosionPrefab;

    private FPS_Controller m_owner;
    private bool m_fired = false;
    private Rigidbody m_body;
    private float m_force = 75.0f;
    private float m_maxLifetime = 20.0f;
    

    public void Fire(FPS_Controller owner)
    {
        AutoGravity ag = GetComponent<AutoGravity>();
        if (ag != null)
        {
            ag.Activate(owner.currentPlanet, 0.5f);
        }

        m_owner = owner;
        m_fired = true;
        m_body = GetComponent<Rigidbody>();

        m_body.AddForce(transform.up * m_force, ForceMode.Impulse);

        StartCoroutine(AutoDetonate());
    }



    private void OnTriggerEnter(Collider other)
    {
        // ignore your master
        if (other.transform == m_owner.transform)
        {
            return;
        }
        // print("Col. - Other: " + other.name + " | Owner:" + m_owner.name);
        Detonate();
    }

    private void Detonate()
    {
        StopAllCoroutines();
        GameObject ps = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(ps, ps.GetComponent<ParticleSystem>().main.duration);
        Destroy(gameObject);
    }

    private IEnumerator AutoDetonate()
    {
        yield return new WaitForSeconds(m_maxLifetime);
        Detonate();
    }
}
