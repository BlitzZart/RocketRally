using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> ServerRestartInS = new NetworkVariable<int>();

    public Dictionary<string, ulong> UniqueIdNetworkIdMap = new Dictionary<string, ulong>();
    public List<PlayerScoreData> RankingList = new List<PlayerScoreData>();


    private bool m_autoRestartCountdownIsRunning = false;
    private int m_autoRestartIntervalInS = 361;
    private int m_autoRestartCurrentTimeInS = 0;

    [Serializable]
    public class PlayerScoreData : IComparable<PlayerScoreData>
    {
        public string uniqueId;
        public string name;
        public int deaths;
        public int kills;
        public int score;

        public PlayerScoreData(string uniqueId, string name, int deaths, int kills, int score)
        {
            this.uniqueId = uniqueId;
            this.name = name;
            this.deaths = deaths;
            this.kills = kills;
            this.score = score;
        }

        public int CompareTo(PlayerScoreData other)
        {
            return other.score.CompareTo(score);
        }
    }
    private Dictionary<string, PlayerScoreData> m_playerScoreDict;

    public Action<string, string> ScoreChanged;
    public Action<string> NameChanged;

    private void Start()
    {
        Application.targetFrameRate = 100;

        m_playerScoreDict = new Dictionary<string, PlayerScoreData>();

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
        if (NetworkManager.Singleton.IsServer)
        {
            Application.targetFrameRate = 30;
        }

        print("Target FPS = " + Application.targetFrameRate);
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

            UpdateAutoRestart();
        }
        else
        {
            StartCoroutine(WaitForOwnershipAndAddInitialize());
        }
    }

    private IEnumerator WaitForOwnershipAndAddInitialize()
    {
        NetworkObject no = GetComponent<NetworkObject>();

        ulong playerId = NetworkManager.Singleton.LocalClientId;

        yield return new WaitUntil(() => no.OwnerClientId == playerId);
        print("WaitForOwnershipAndAddInitialize DONE");
        string uniqueId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_EDITOR
        uniqueId = "UnityEditorSession";
#endif

        string playerName = ConnectionModeScript.PlayerName;

        // propagate playerId/playerName mapping to server
        SendPlayerIdNameMappingServerRpc(uniqueId, playerId, playerName);

        // set unique id on server and client
        //SendUniqueIdServerRpc(SystemInfo.deviceUniqueIdentifier);
        NW_PlayerScript.Instance.UniqueId = uniqueId;

        print("> WaitForOwnershipAndAddInitialize " + uniqueId + " = " + playerId);
        UniqueIdNetworkIdMap[uniqueId] = playerId;

        // update player name if uniqueId already exists
        // update name
        if (m_playerScoreDict.ContainsKey(uniqueId))
        {
            m_playerScoreDict[uniqueId].name = playerName;
            //SendPlayerIdNameMappingServerRpc(uniqueId, playerId, playerName);
        }
    }

    private void OnClientDisconnected(ulong obj)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NW_PlayerScript.Instance.PlayerKilled -= OnPlayerKilled;
            if (NetworkManager.ConnectedClients.ContainsKey(obj))
            {
                print("HAS client " + obj);
                //NetworkManager.DisconnectClient(obj);
            }

            RestartServerIfAllPlayersLeft();
        }
        if(IsClient)
        {
            Cursor.lockState = CursorLockMode.None;
            NW_PlayerScript.Instance.RestartClient();
        }
    }

    private void UpdateAutoRestart()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 &&
            !m_autoRestartCountdownIsRunning)
        {
            m_autoRestartCountdownIsRunning = true;
            StartCoroutine(AutoRestartServerAfterTime());
        }
    }

    private IEnumerator AutoRestartServerAfterTime()
    {
        int timeLeft = m_autoRestartIntervalInS;
        ServerRestartInS.Value = (timeLeft);
        print("Restart in " + (timeLeft));
        while (timeLeft > 0)
        {
            timeLeft = m_autoRestartIntervalInS - m_autoRestartCurrentTimeInS;

            bool updateVar = false;
            if (timeLeft <= 10)
            {
                updateVar = true;
            }
            else if (timeLeft <= 30)
            {
                updateVar = timeLeft % 10 == 0;
            }
            else if (timeLeft <= 90)
            {
                updateVar = timeLeft % 30 == 0;
            }
            else
            {
                updateVar = timeLeft % 300 == 0;
            }

            if (updateVar)
            {
                ServerRestartInS.Value = (timeLeft);
                print("Restart in " + (timeLeft));
            }

            m_autoRestartCurrentTimeInS += 1;
            yield return new WaitForSeconds(1.0f);
        }

        //StopAllServerRoutines();
        RestartServer();
    }

    private void RestartServerIfAllPlayersLeft()
    {
        if (NetworkManager.ConnectedClients.Count < 1)
        {
            print("Restart Server because last client just left.");

            //StopAllServerRoutines();
            RestartServer();
        }
    }

    private void OnPlayerKilled(ulong victimId, ulong killerId)
    {
        print("OnPlayerKilled " + victimId + " by " + killerId);
        PlayerKilled(victimId, killerId);
    }
    private void AddEntriesIfNotExists(string uniqueId, ulong playerId, string playerName)
    {
        //if (!m_playerScoreDict.ContainsKey(uniqueId))
        //{
        if (playerName == string.Empty)
        {
            playerName = " Rocketeer #" + playerId;
        }

        //print("Added: " + playerId + " = " + playerName);

        if (!m_playerScoreDict.ContainsKey(uniqueId))
        {
            m_playerScoreDict.Add(uniqueId, new PlayerScoreData(uniqueId, playerName, 0, 0, 0));
        }
        else
        {
            m_playerScoreDict[uniqueId].name = playerName;
            NameChanged?.Invoke(uniqueId);
        }


        // propagate - so ui can add it
        ScoreChanged?.Invoke(string.Empty, uniqueId);
        //}
    }

    private void SendAllScoreData()
    {
        // wait for a while or until we know that everythin is up to date

        foreach (KeyValuePair<string, PlayerScoreData> psd in m_playerScoreDict)
        {
            if (UniqueIdNetworkIdMap.ContainsKey(psd.Key))
            {
                ulong playerId = UniqueIdNetworkIdMap[psd.Key];
                print("SendAllScoreData");
                AllScoreClientRpc(psd.Key, playerId, psd.Value.name, psd.Value.deaths, psd.Value.kills, psd.Value.score);
            }
        }
    }

    public PlayerScoreData GetPlayerData(string uniqueId)
    {
        if (uniqueId == "")
        {
            return null;
        }

        //ulong playerId = UniqueIdNetworkIdMap[uniqueId];

        return m_playerScoreDict[uniqueId];
    }

    public void PlayerKilled(ulong victimId, ulong killerId)
    {
        //AddEntriesIfNotExists(victimId);
        //AddEntriesIfNotExists(killerId);

        string victimUID = GetUniqueIdForNetworkId(victimId);
        string killerUID = GetUniqueIdForNetworkId(killerId);

        m_playerScoreDict[victimUID].deaths += 1;

        if (victimId == killerId)
        {
            m_playerScoreDict[killerUID].score -= 1;
        }
        else
        {
            m_playerScoreDict[killerUID].kills += 1;
            m_playerScoreDict[killerUID].score += 1;
        }

        //foreach (KeyValuePair<ulong, PlayerScoreData> psd in m_playerScoreDict)
        //{
        //    print(">>> " + psd.Key + " " + psd.Value.name + " " + psd.Value.kills + " " + psd.Value.death + " " + psd.Value.score);
        //}

        if (NetworkManager.Singleton.IsServer)
        {
            PlayerScoreData v = m_playerScoreDict[victimUID];
            PlayerScoreData k = m_playerScoreDict[killerUID];
            ScoreChangedClientRpc(victimUID, killerUID, v.deaths, v.kills, k.deaths, k.kills, v.score, k.score);
        }
    }

    public void StopAllRoutines()
    {
        StopAllCoroutines();
    }

    public void RestartServer()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            RestartClientRpc();
        }

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);

        StopAllRoutines();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #region networking

    [ClientRpc]
    public void RestartClientRpc()
    {
        print("RestartClientRpc");

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.StopAllRoutines();
        }

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ServerRpc]
    private void SendUniqueIdServerRpc(string uniqueId)
    {
        NW_PlayerScript.Instance.UniqueId = uniqueId;
    }

    [ServerRpc]
    private void SendPlayerIdNameMappingServerRpc(string uniqueId, ulong playerId, string playerName)
    {
        NW_PlayerScript.Instance.UniqueId = uniqueId;
        UniqueIdNetworkIdMap[uniqueId] = playerId;

        AddEntriesIfNotExists(uniqueId, playerId, playerName);

        print("> SendPlayerIdNameMappingServerRpc " + uniqueId + " = " + playerId + " name = " + playerName);
        // propagate to all clients
        SendPlayerIdNameMappingClientRpc(uniqueId, playerId, playerName);
    }

    [ClientRpc]
    private void SendPlayerIdNameMappingClientRpc(string uniqueId, ulong playerId, string playerName)
    {
        AddEntriesIfNotExists(uniqueId, playerId, playerName);

        //print("> SendPlayerIdNameMappingClientRpc " + uniqueId + " = " + playerId + " name " + playerName);
        UniqueIdNetworkIdMap[uniqueId] = playerId;
        NW_PlayerScript.Instance.UniqueId = uniqueId;
    }

    [ClientRpc]
    private void AllScoreClientRpc(string uniqueId, ulong playerId, string playerName, int deaths, int kills, int score)
    {
        if (uniqueId == NW_PlayerScript.Instance.UniqueId)
        {
            playerName = ConnectionModeScript.PlayerName;
        }

        print("AllScoreClientRpc " + playerName);

        AddEntriesIfNotExists(uniqueId, playerId, playerName);
        PlayerScoreData p = m_playerScoreDict[uniqueId];
        p.name = playerName;
        p.deaths = deaths;
        p.kills = kills;
        p.score = score;
    }
    [ClientRpc]
    private void ScoreChangedClientRpc(string victimId, string killerId,
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

        if (!RankingList.Contains(k))
        {
            RankingList.Add(k);
        }
        if (!RankingList.Contains(v))
        {
            RankingList.Add(v);
        }

        RankingList.Sort();

        // update UI
        ScoreChanged?.Invoke(victimId, killerId);
    }
    #endregion

    public string GetUniqueIdForNetworkId(ulong playerId)
    {
        print("GetUniqueIdForNetworkId " + playerId);
        return UniqueIdNetworkIdMap.FirstOrDefault(x => x.Value == playerId).Key;
    }

    public ulong GetNetwrokIdForUniqueId(string uniqueId)
    {
        print("GetNetwrokIdForUniqueId " + uniqueId);
        return UniqueIdNetworkIdMap[uniqueId];
    }
}
