using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public class PlayerScoreData
    {
        public string name;
        public int deaths;
        public int kills;
        public int score;

        public PlayerScoreData(string name, int deaths, int kills, int score)
        {
            this.name = name;
            this.deaths = deaths;
            this.kills = kills;
            this.score = score;
        }
    }
    private Dictionary<ulong, PlayerScoreData> m_playerScoreDict;

    public Action<ulong, ulong> ScoreChanged;

    private void Start()
    {
        m_playerScoreDict = new Dictionary<ulong, PlayerScoreData>();

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    private void OnDisable()
    {
        //NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        if (NW_PlayerScript.Instance != null)
        {
            NW_PlayerScript.Instance.PlayerKilled -= OnPlayerKilled;
        }
    }

    private void OnServerStarted()
    {

    }
    private void OnClientConnected(ulong obj)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            print("ClientConnected: " + obj);
            NW_PlayerScript.Instance.PlayerKilled += OnPlayerKilled;

            NetworkObject no = GetComponent<NetworkObject>();
            no.ChangeOwnership(obj);

            SendAllScoreData();
        }
        else
        {
            StartCoroutine(WaitForOwnership());
        }
    }

    private IEnumerator WaitForOwnership()
    {
        NetworkObject no = GetComponent<NetworkObject>();

        yield return new WaitUntil(() => no.OwnerClientId == NetworkManager.Singleton.LocalClientId);
        // propagate playerId/playerName mapping to server
        SendPlayerIdNameMappingServerRpc(NetworkManager.Singleton.LocalClientId, ConnectionModeScript.PlayerName);
    }

    private void OnClientDisconnected(ulong obj)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            print("ClientDisconnected: " + obj);
            NW_PlayerScript.Instance.PlayerKilled -= OnPlayerKilled;
        }
    }

    private void OnPlayerKilled(ulong victimId, ulong killerId)
    {
        print("OnPlayerKilled " + victimId + " by " + killerId);
        PlayerKilled(victimId, killerId);
    }

    public PlayerScoreData GetPlayerData(ulong playerId)
    {
        return m_playerScoreDict[playerId];
    }

    public void PlayerKilled(ulong victimId, ulong killerId)
    {
        //AddEntriesIfNotExists(victimId);
        //AddEntriesIfNotExists(killerId);

        m_playerScoreDict[victimId].deaths += 1;

        if (victimId == killerId)
        {
            m_playerScoreDict[killerId].score -= 1;
        }
        else
        {
            m_playerScoreDict[killerId].kills += 1;
            m_playerScoreDict[killerId].score += 1;
        }


        //foreach (KeyValuePair<ulong, PlayerScoreData> psd in m_playerScoreDict)
        //{
        //    print(">>> " + psd.Key + " " + psd.Value.name + " " + psd.Value.kills + " " + psd.Value.death + " " + psd.Value.score);
        //}

        if (NetworkManager.Singleton.IsServer)
        {
            PlayerScoreData v = m_playerScoreDict[victimId];
            PlayerScoreData k = m_playerScoreDict[killerId];
            ScoreChangedClientRpc(victimId, killerId, v.deaths, v.kills, k.deaths, k.kills, v.score, k.score);
        }
    }

    private void AddEntriesIfNotExists(ulong playerId, string playerName)
    {
        if (!m_playerScoreDict.ContainsKey(playerId))
        {
            if (playerName == string.Empty)
            {
                playerName = " Rocketeer #" + playerId;
            }

            m_playerScoreDict.Add(playerId, new PlayerScoreData(playerName, 0, 0, 0));
        }
    }

    private void SendAllScoreData()
    {
        foreach (KeyValuePair<ulong, PlayerScoreData> psd in m_playerScoreDict)
        {
            AllScoreClientRpc(psd.Key, psd.Value.name, psd.Value.deaths, psd.Value.kills, psd.Value.score);
        }
    }

    #region newtorking
    [ServerRpc]
    private void SendPlayerIdNameMappingServerRpc(ulong playerId, string playerName)
    {
        AddEntriesIfNotExists(playerId, playerName);

        // propagate to all clients
        SendPlayerIdNameMappingClientRpc(playerId, playerName);
    }

    [ClientRpc]
    private void SendPlayerIdNameMappingClientRpc(ulong playerId, string playerName)
    {
        AddEntriesIfNotExists(playerId, playerName);
    }

    [ClientRpc]
    private void AllScoreClientRpc(ulong playerId, string playerName, int deaths, int kills, int score)
    {
        AddEntriesIfNotExists(playerId, playerName);
        PlayerScoreData p = m_playerScoreDict[playerId];
        p.name = playerName;
        p.deaths = deaths;
        p.kills = kills;
        p.score = score;
    }
    [ClientRpc]
    private void ScoreChangedClientRpc(ulong victimId, ulong killerId,
                                        int victimDeaths, int victimKills,
                                        int killerDeaths, int killerKills,
                                        int victimScore, int killerScore)
    {
        //AddEntriesIfNotExists(victimId);
        //AddEntriesIfNotExists(killerId);

        PlayerScoreData v = m_playerScoreDict[victimId];
        v.deaths = victimDeaths;
        v.kills = victimKills;
        v.score = victimScore;

        PlayerScoreData k = m_playerScoreDict[killerId];
        k.deaths = killerDeaths;
        k.kills = killerKills;
        k.score = killerScore;

        // update UI
        ScoreChanged?.Invoke(victimId, killerId);
    }
    #endregion
}
