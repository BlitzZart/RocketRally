using Unity.Netcode;
using UnityEngine;

public class Detonator : MonoBehaviour
{
    public delegate void DetonationDelegate(ulong playerI, Vector3 pos, float maxDamage, float maxRange);
    public static event DetonationDelegate Detonated;

    [SerializeField]
    private GameObject m_explosionPrefab;

    public void SpawnDetonation(ulong playerId, Vector3 pos, float maxRange, float maxDamage)
    {
        GameObject go = Instantiate(m_explosionPrefab, pos, Quaternion.identity);
        Destroy(go, go.GetComponent<ParticleSystem>().main.duration);

        //exPos = pos;
        //exMaxRange = maxRange;

        if (Detonated != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Detonated?.Invoke(playerId, pos, maxDamage, maxRange);
            }
        }
    }

    //private Vector3 exPos;
    //private float exMaxRange;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(exPos, exMaxRange);
    //}
}
