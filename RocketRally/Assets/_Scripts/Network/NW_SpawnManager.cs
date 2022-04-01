using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NW_SpawnManager : MonoBehaviour
{
    NetworkSpawnManager spawnManager;
    private void Start()
    {
        spawnManager = NetworkManager.Singleton.SpawnManager;

    }
}
