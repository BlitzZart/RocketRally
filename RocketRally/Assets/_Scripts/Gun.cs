using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private float m_simpleRocketCosts = 20;
    private float m_personalGravityRocketCosts = 50;
    private float m_homingRocketCosts = 70;

    public float CurrentPowerCost
    {
        get {
            switch (_rocketType)
            {
                case RocketType.Simple:
                    return m_simpleRocketCosts;
                case RocketType.PersonalGravity:
                    return m_personalGravityRocketCosts;
            }

            return 0.0f;
        }
    }

    public enum RocketType
    {
        Simple = 0,
        //GlobalGravity = 1,
        PersonalGravity = 1,
        //Homing = 3,
        None = 2 // used for count/length
    }

    [SerializeField] private RocketType _rocketType = RocketType.Simple;
    public RocketType CurrentRocketType { get => _rocketType; }

    private bool m_ready = true;
    private float m_cooldown = 0.05f;

    public Rocket m_homingRocketPrefab;
    public Rocket m_personalGravityRocketPrefab;
    public Rocket m_globalGravityRocketPrefab;
    public Rocket m_stdRocketPrefab;

    public Transform muzzle;
    public FPS_Controller m_fpsController;

    public Action<RocketType> RocketTypeChanged;
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

        RocketTypeChanged?.Invoke(_rocketType);
    }

    public void Fire(bool triggerDown = true, Transform target = null)
    {
        StopCoroutine(Cooldown());
        StartCoroutine(Cooldown());
        if (!m_ready ||
            m_fpsController.IsDead)
        {
            return;
        }
        m_ready = false;

        if (triggerDown && _rocketType == RocketType.PersonalGravity ||
            !triggerDown && _rocketType == RocketType.Simple)
        {
            return;
        }



        Rocket r;
        //if (_rocketType == RocketType.Homing)
        //{
        //    if (!GunHasEnergy())
        //    {
        //        return;
        //    }
        //    if (target == null)
        //    {
        //        print("NO TARGET!");
        //        return;
        //    }


        //    // TODO: implement me
        //    // use PID instead of gravity hack
        //    // tell server to update correctly!
        //    return;


        //    r = Instantiate(m_homingRocketPrefab, muzzle.position, muzzle.rotation);

        //    AutoGravity ag = r.GetComponent<AutoGravity>();
        //    if (ag != null)
        //    {
        //        ag.SetTarget(target);
        //    }

        //    m_power -= m_homingRocketCosts;
        //    PowerChanged?.Invoke(m_power);
        //}
        //else if (_rocketType == RocketType.GlobalGravity)
        //{
        //    if (!GunHasEnergy())
        //    {
        //        return;
        //    }
        //    r = Instantiate(m_globalGravityRocketPrefab, muzzle.position, muzzle.rotation);
        //    m_power -= m_gravityRocketCosts;
        //    PowerChanged?.Invoke(m_power);
        //}
        //else
        if (_rocketType == RocketType.PersonalGravity)
        {
            if (!GunHasEnergy())
            {
                return;
            }

            r = Instantiate(m_personalGravityRocketPrefab, muzzle.position, muzzle.rotation);

            AutoGravity ag = r.GetComponent<AutoGravity>();
            if (ag != null)
            {
                ag.SetTarget(target);
            }

            m_power -= m_personalGravityRocketCosts;
            PowerChanged?.Invoke(m_power);
        }
        else // simple rocket
        {
            if (!GunHasEnergy())
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

        NetworkObject rocketNwObj = target?.GetComponentInParent<NetworkObject>();
        if (rocketNwObj == null)
        {
            rocketNwObj = transform.root.GetComponent<NetworkObject>();
        }

        r.Fire(vel.magnitude, NetworkManager.Singleton.LocalClientId, target, true);

        NW_PlayerScript.Instance.FireNetworkedRocket(
            (int)_rocketType,
            muzzle.position,
            muzzle.rotation.eulerAngles,
            vel.magnitude,
            NetworkManager.Singleton.LocalClientId,
            rocketNwObj);
    }

    private bool GunHasEnergy()
    {
        return m_power >= CurrentPowerCost;
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
