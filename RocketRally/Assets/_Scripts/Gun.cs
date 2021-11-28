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


    public void Fire()
    {
        if (!m_ready)
        {
            return;
        }
        m_ready = false;
        StopCoroutine(Cooldown());
        StartCoroutine(Cooldown());

        //Rocket r = Instantiate(rocketPrefab, muzzle.position, muzzle.rotation);
        //r.Fire(m_netPlayer.FpsCtrl.RigidBody.velocity, 0);

        NW_PlayerScript.Instance.FireNetworkedRocket(
            muzzle.position,
            muzzle.rotation.eulerAngles,
            NW_PlayerScript.Instance.FpsCtrl.RigidBody.velocity,
            NetworkManager.Singleton.LocalClientId);

        //r.Fire(m_owner);
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(m_cooldown);
        m_ready = true;
    }
}
