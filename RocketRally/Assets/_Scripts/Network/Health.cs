using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private float m_startHp = 100;

    [SerializeField]
    private float m_maxHp = 100;

    [SerializeField]
    private NetworkVariable<float> m_currentHp = new NetworkVariable<float>();
    private NetworkVariable<bool> m_playerIsDead = new NetworkVariable<bool>();


    public float CurrentHp { get => m_currentHp.Value; set => m_currentHp.Value = value; }

    public Action<float> PlayerHealthChanged;
    public Action<ulong> PlayerDied;



    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            m_currentHp.Value = m_startHp;
            m_playerIsDead.Value = false;
            Detonator.Detonated += OnDetonated;
        }
        else
        {
            m_currentHp.OnValueChanged += OnNwHealthChanged;
        }
    }

    private void OnDisable()
    {
        m_currentHp.OnValueChanged -= OnNwHealthChanged;
        if (NetworkManager.Singleton.IsServer)
        {
            Detonator.Detonated -= OnDetonated;
        }
    }

    private void OnNwHealthChanged(float previousValue, float newValue)
    {
        PlayerHealthChanged?.Invoke(newValue);
    }

    private void OnDetonated(ulong ownerId, Vector3 pos, float maxDamage, float maxRange)
    {
        float dist = Mathf.Max(1.0f, Vector3.Distance(transform.position, pos));

        if (dist < maxRange)
        {
            float dmgWheight = (maxRange / dist) / maxRange;
            print(">> Detonation from: " + ownerId + " me: " + NetworkManager.Singleton.LocalClientId);
            Damage(ownerId, maxDamage * dmgWheight);
        }
    }

    public void HealBy(float hp)
    {
        print("Heal by " + hp);
        m_currentHp.Value = Mathf.Min(m_currentHp.Value + hp, m_maxHp);
        HealthChanged();
    }

    public void Revive()
    {
        print("Heal to " + m_maxHp);
        m_playerIsDead.Value = false;
        m_currentHp.Value = m_maxHp;
        HealthChanged();
    }

    public void Damage(ulong dmgDealerId, float hp)
    {
        print("Damage: " + hp);
        m_currentHp.Value = Mathf.Max(m_currentHp.Value - hp, 0);
        print("Health: " + m_currentHp.Value);
        HealthChanged(dmgDealerId);
    }

    // TODO: translate ulong (net id) to int (player number) or player UID at some point
    private void HealthChanged(ulong dmgDealerId = ulong.MaxValue)
    {
        PlayerHealthChanged?.Invoke(m_currentHp.Value);

        if (m_currentHp.Value <= 0)
        {
            if (!m_playerIsDead.Value)
            {
                m_playerIsDead.Value = true;
                PlayerDied?.Invoke(dmgDealerId);
            }
        }
    }
}
