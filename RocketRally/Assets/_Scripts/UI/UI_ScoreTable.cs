using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: only client
public class UI_ScoreTable : MonoBehaviour
{
    [SerializeField] private UI_ScoreEntry m_entryPrefab;
    [SerializeField] private Color m_colorOthers, m_colorSelf;
    private bool m_colorsSet = false;


    private Dictionary<string, UI_ScoreEntry> m_entries;

    private GameManager m_gameManager;

    private void Start()
    {
        m_entries = new Dictionary<string, UI_ScoreEntry>();

        m_gameManager = FindObjectOfType<GameManager>();
        m_gameManager.ScoreChanged += OnScoreChanged;
        m_gameManager.NameChanged += OnNameChanged;
    }
    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.ScoreChanged -= OnScoreChanged;
            m_gameManager.NameChanged -= OnNameChanged;
        }
    }

    private void UpdateRanking()
    {
        for (int i = 0; i < m_gameManager.RankingList.Count; i++)
        {
            string uid = m_gameManager.RankingList[i].uniqueId;

            if (m_entries.ContainsKey(uid))
            {
                m_entries[uid].transform.SetSiblingIndex(i);
            }
        }
    }

    private void OnNameChanged(string uniqueId)
    {
        UI_ScoreEntry entry = m_entries[uniqueId];
        entry.UpdateName(m_gameManager.GetPlayerData(uniqueId).name);
    }

    private void OnScoreChanged(string victimId, string killerId)
    {
        //print("OnScoreChanged victim: " + victimId + " killer: " + killerId);

        GameManager.PlayerScoreData victimData = m_gameManager.GetPlayerData(victimId);
        GameManager.PlayerScoreData killerData = m_gameManager.GetPlayerData(killerId);

        // victimData is null if OnScoreChanged was fired because new player joined
        if (victimData != null &&
            !m_entries.ContainsKey(victimId))
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

        if (victimData != null)
        {
            UI_ScoreEntry vE = m_entries[victimId];
            vE.UpdateScore(victimData.kills, victimData.deaths, victimData.score);
        }

        UI_ScoreEntry kE = m_entries[killerId];
        kE.UpdateScore(killerData.kills, killerData.deaths, killerData.score);

        UpdateRanking();
        if (!m_colorsSet)
        {
            m_colorsSet = true;
            StartCoroutine(SetColorsWhenPlayerUidIsKnown());
        }
    }


    private IEnumerator SetColorsWhenPlayerUidIsKnown()
    {
        yield return new WaitUntil(() => NW_PlayerScript.Instance != null);
        yield return new WaitUntil(() => NW_PlayerScript.Instance.UniqueId != string.Empty);

        foreach (var e in m_entries)
        {
            if (e.Key == NW_PlayerScript.Instance.UniqueId)
            {
                e.Value.SetColor(m_colorSelf);
            }
        }
    }

    private void SetColor(UI_ScoreEntry entry, string uid)
    {
        if (IsSelf(uid))
        {
            entry.SetColor(m_colorSelf);
        }
        else
        {
            entry.SetColor(m_colorOthers);
        }
    }

    private bool IsSelf(string uid)
    {
        return uid == NW_PlayerScript.Instance.UniqueId;
    }
}
