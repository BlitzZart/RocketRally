using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float gravity = -9.81f;
    public float mass;

    private float m_massBase = 10;

    //private int m_numberOfSpawnPoints = 5;
    //private Vector3[] m_spawnPoints;

    private SphereCollider m_sphereCollider;

    private void Awake()
    {
        m_sphereCollider = GetComponentInChildren<SphereCollider>();
    }

    private void Start()
    {
        //mass = m_massBase * transform.localScale.x;
        // DO ONLY ON SERVER SetSpawnPoints();
    }

    private void Remesh()
    {

    }

    // TODO: would need updating if planets move
    // [unused pregen pos]
    //public void SetSpawnPoints()
    //{
    //    m_spawnPoints = new Vector3[m_numberOfSpawnPoints];
    //    SphereCollider col = GetComponentInChildren<SphereCollider>();

    //    for (int i = 0; i < m_spawnPoints.Length; i++)
    //    {
    //        GameObject sphere = new GameObject("SpawnPoint_" + i);

    //        // planet pos + radius + 3m above ground
    //        Vector3 positionOnSurface = transform.position + 
    //            Random.onUnitSphere * col.transform.lossyScale.x * col.radius;
    //        Vector3 aboveSurface = (positionOnSurface - col.transform.position).normalized * 3.0f;

    //        sphere.transform.position = positionOnSurface + aboveSurface;
    //        sphere.transform.SetParent(transform);
    //    }
    //}

    public Vector3 GetRandomSpawnPoint()
    {
        // uses pregen pos
        //if (m_spawnPoints == null)
        //{
        //    SetSpawnPoints();
        //}

        Vector3 positionOnSurface = transform.position +
            Random.onUnitSphere * m_sphereCollider.transform.lossyScale.x * m_sphereCollider.radius;
        Vector3 aboveSurface = (positionOnSurface - m_sphereCollider.transform.position).normalized * 3.0f;

        return positionOnSurface + aboveSurface;
    }
}
