using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private float m_simpleRocketCosts = 20;
    private float m_gravityRocketCosts = 10;

    public enum RocketType
    {
        Simple = 0, 
        Gravity = 1,
        None = 2
    }

    [SerializeField] private RocketType _rocketType = RocketType.Simple;

    private bool m_ready = true;
    private float m_cooldown = 0.2f;

    public Rocket m_gravityRocketPrefab;
    public Rocket m_stdRocketPrefab;

    public Transform muzzle;
    public FPS_Controller m_fpsController;

    public Action<float> PowerChanged;
    private float m_power = 100.0f;
    private float m_powerUpSpeed = 10;

    public float Power { get => m_power; }

    private void OnEnable()
    {
        m_fpsController = transform.root.GetComponent<FPS_Controller>();
    }

    public void NextRocketType()
    {
        _rocketType = (RocketType)(((int)_rocketType + 1) % (int)RocketType.None);
    }

    public void Fire()
    {
        if (!m_ready ||
            m_fpsController.IsDead)
        {
            return;
        }
        m_ready = false;
        StopCoroutine(Cooldown());
        StartCoroutine(Cooldown());

        Rocket r;
        if (_rocketType == RocketType.Gravity)
        {
            if (m_power < m_gravityRocketCosts)
            {
                return;
            }
            r = Instantiate(m_gravityRocketPrefab, muzzle.position, muzzle.rotation);
            m_power -= m_gravityRocketCosts;
            PowerChanged?.Invoke(m_power);
        }
        else
        {
            if (m_power < m_simpleRocketCosts)
            {
                return;
            }
            r = Instantiate(m_stdRocketPrefab, muzzle.position, muzzle.rotation);
            m_power -= m_simpleRocketCosts;
            PowerChanged?.Invoke(m_power);
        }

        Vector3 vel = NW_PlayerScript.Instance.FpsCtrl.RigidBody.velocity;

        //print("VEL " + vel.magnitude);
        // only use start vel of player if the player moves forward
        float dot = Vector3.Dot(vel.normalized, transform.forward);
        Debug.DrawRay(transform.position, transform.forward * 5, Color.magenta, 2.0f);
        Debug.DrawRay(transform.position, transform.up, Color.magenta, 2.0f);
        Debug.DrawRay(transform.position, vel.normalized * 2, Color.red, 2.0f);

        //print("DOT = " + dot);
        if (dot < 0.1f)
        {
            //print("DOT < " + dot);
            vel = -vel;
        }
        r.Fire(vel.magnitude, NetworkManager.Singleton.LocalClientId, true);

        NW_PlayerScript.Instance.FireNetworkedRocket(
            (int)_rocketType,
            muzzle.position,
            muzzle.rotation.eulerAngles,
            vel.magnitude,
            NetworkManager.Singleton.LocalClientId);

        //r.Fire(m_owner);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(m_cooldown);
        m_ready = true;
    }

    private void Update()
    {
        if (m_power < 100.0f)
        {
            m_power = Mathf.Clamp(m_power + Time.deltaTime * m_powerUpSpeed, 0.0f, 100.0f);
            PowerChanged?.Invoke(m_power);
        }
    }
}
