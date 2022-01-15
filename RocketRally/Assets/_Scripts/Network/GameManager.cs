using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkList<int> m_playerScores = new NetworkList<int>();

    private void OnEnable()
    {
        m_playerScores.OnListChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        m_playerScores.OnListChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(NetworkListEvent<int> changeEvent)
    {

    }
}
