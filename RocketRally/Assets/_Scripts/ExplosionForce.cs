using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    private float m_power = 10.0f;
    private bool m_triggeredDisable = false;


    private void Start()
    {
        if (m_triggeredDisable)
        {
            return;
        }
        m_triggeredDisable = true;
        StartCoroutine(Disable());
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 dir = other.transform.position - transform.position;
        other.GetComponent<Rigidbody>().AddForce((dir * m_power) / Mathf.Clamp(dir.magnitude, 1, m_power), ForceMode.Impulse);
    }

    private IEnumerator Disable()
    {
        yield return new WaitForSeconds(0.1f);
        print("DISABLE");
        gameObject.SetActive(false);
    }

}
