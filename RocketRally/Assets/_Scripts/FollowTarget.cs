using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private Transform m_target;
    public void Follow(Transform target)
    {
        m_target = target;
    }

    private void Update()
    {
        if (m_target == null)
        {
            return;
        }
        transform.position = m_target.position;
    }
}
