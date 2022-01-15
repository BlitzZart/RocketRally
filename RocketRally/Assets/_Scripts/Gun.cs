using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private bool m_ready = true;
    private float m_cooldown = 0.2f;


    public Rocket m_gravityRocketPrefab;
    public Rocket m_stdRocketPrefab;

    public Transform muzzle;
    public FPS_Controller m_fpsController;

    private void OnEnable()
    {
        m_fpsController = transform.root.GetComponent<FPS_Controller>();
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

        Rocket r = Instantiate(m_stdRocketPrefab, muzzle.position, muzzle.rotation);
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
}
