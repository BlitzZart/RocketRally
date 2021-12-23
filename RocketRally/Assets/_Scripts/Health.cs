using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float m_startHp = 100;

    [SerializeField]
    private float m_maxHp = 100;

    [SerializeField]
    private float m_currentHp;

    public float CurrentHp { get => m_currentHp; set => m_currentHp = value; }

    public Action<float> OnHealthChanged;

    private void Start()
    {
        m_currentHp = m_startHp;

        Detonator.Detonated += (pos, dmg, rng) =>
        {
            // clamt dmg multiplicator to 1
            float dist = Mathf.Max(1.0f, Vector3.Distance(transform.position, pos));

            if (dist < rng)
            {
                float dmgWheight = (rng / dist) / rng;
                //print("range: " + rng + " dist: " + dist + " % " + dmgWheight);
                Damage(dmg * dmgWheight);
            }
        };
    }

    public void Heal(float hp)
    {
        m_currentHp = Mathf.Min(m_currentHp + hp, m_maxHp);
        HealthChanged();
    }

    public void Damage(float hp)
    {
        print("DMG " + hp);
        m_currentHp = Mathf.Max(m_currentHp - hp, 0);
        HealthChanged();
    }

    private void HealthChanged()
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(m_currentHp);
        }

    }
}
