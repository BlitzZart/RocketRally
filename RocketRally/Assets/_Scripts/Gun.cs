using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private bool m_ready = true;
    private float m_cooldown = 0.2f;

    public Rocket rocketPrefab;
    public Transform muzzle;

    public void Fire()
    {
        if (!m_ready)
        {
            return;
        }
        m_ready = false;
        StopCoroutine(Cooldown());
        StartCoroutine(Cooldown());

        Instantiate(rocketPrefab, muzzle.position, muzzle.rotation).Fire(transform.root);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(m_cooldown);
        m_ready = true;
    }
}
