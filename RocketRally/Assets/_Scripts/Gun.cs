using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private FPS_Controller m_owner;
    private bool m_ready = true;
    private float m_cooldown = 0.2f;

    public Rocket rocketPrefab;
    public Transform muzzle;

    private void Start()
    {
        m_owner = transform.root.GetComponentInChildren<FPS_Controller>();
    }

    public void Fire()
    {
        if (!m_ready)
        {
            return;
        }
        m_ready = false;
        StopCoroutine(Cooldown());
        StartCoroutine(Cooldown());

        Rocket r = Instantiate(rocketPrefab, muzzle.position, muzzle.rotation);
        r.Fire(m_owner);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(m_cooldown);
        m_ready = true;
    }
}
