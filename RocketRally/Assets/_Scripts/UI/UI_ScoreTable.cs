using UnityEngine;
using System.Collections.Generic;
using System;

// TODO: only client
public class UI_ScoreTable : MonoBehaviour
{
    [SerializeField] UI_ScoreEntry m_entryPrefab;

    private Dictionary<ulong, UI_ScoreEntry> m_entries;

    private GameManager m_gameManager;

    private void Start()
    {
        m_entries = new Dictionary<ulong, UI_ScoreEntry>();

        m_gameManager = FindObjectOfType<GameManager>();
        m_gameManager.ScoreChanged += OnScoreChanged;
    }
    private void OnDestroy()
    {
        m_gameManager.ScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(ulong victimId, ulong killerId)
    {
        print("OnScoreChanged victime: " + victimId + " killer: " + killerId);
        GameManager.PlayerScoreData victimData = m_gameManager.GetPlayerData(victimId);
        GameManager.PlayerScoreData killerData = m_gameManager.GetPlayerData(killerId);

        if (!m_entries.ContainsKey(victimId))
        {
            UI_ScoreEntry se = Instantiate(m_entryPrefab, transform);
            se.Initilize(victimId);
            se.UpdateName(victimData.name);
            m_entries.Add(victimId, se);
        }
        if (!m_entries.ContainsKey(killerId))
        {
            UI_ScoreEntry se = Instantiate(m_entryPrefab, transform);
            se.Initilize(killerId);
            se.UpdateName(killerData.name);
            m_entries.Add(killerId, se);
        }

        UI_ScoreEntry vE = m_entries[victimId];
        vE.UpdateScore(victimData.kills, victimData.deaths, victimData.score);

        UI_ScoreEntry kE = m_entries[killerId];
        kE.UpdateScore(killerData.kills, killerData.deaths, killerData.score);
    }
}
