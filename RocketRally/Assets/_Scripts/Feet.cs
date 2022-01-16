using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feet : MonoBehaviour
{
    public Action HitGround;
    private bool m_onGround = false;
    public bool OnGround { get { return m_onGround; } }

    private void OnTriggerEnter(Collider other)
    {
        m_onGround = true;
        
        HitGround?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        m_onGround = false;
    }


}
