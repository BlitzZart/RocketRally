using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feet : MonoBehaviour
{
    private bool m_onGround = false;
    public bool OnGround { get => m_onGround; }

    private void OnTriggerEnter(Collider other)
    {
        m_onGround = true;
    }

    private void OnTriggerExit(Collider other)
    {
        m_onGround = false;
    }
}
