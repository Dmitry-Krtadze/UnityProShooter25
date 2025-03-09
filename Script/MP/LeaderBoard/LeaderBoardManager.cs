using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class LeaderBoardManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static LeaderBoardManager Instance { get; private set; }

    [System.Serializable]
    public struct PlayerStats
    {
        public int kills;
        public int deaths;
        public int score;
    }

    [System.Serializable]
    public class PlayerEntry
    {
        public string playerName;
        public PlayerStats stats;
    }

    public List<PlayerEntry> leaderboardList = new List<PlayerEntry>();
    private Dictionary<string, PlayerStats> leaderboard = new Dictionary<string, PlayerStats>();

    public delegate void PlayerStatsUpdated(string playerName, PlayerStats stats);
    public event PlayerStatsUpdated OnPlayerAdded;
    public event PlayerStatsUpdated OnPlayerRemoved;
    public event System.Action OnLeaderboardUpdated;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем объект при смене сцен
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Новый мастер пересылает актуальный лидерборд всем игрокам
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master switched. Syncing leaderboard...");
            SyncLeaderboard();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Мастер добавляет нового игрока в лидерборд и отправляет данные всем
        if (PhotonNetwork.IsMasterClient)
        {
            if (!leaderboard.ContainsKey(newPlayer.NickName))
            {
                UpdateLeaderboard(newPlayer.NickName, 0, 0, 0);
            }
            SyncLeaderboard();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Удаляем игрока при выходе из комнаты
        if (PhotonNetwork.IsMasterClient)
        {
            RemovePlayer(otherPlayer.NickName);
            SyncLeaderboard();
        }
    }

    [PunRPC]
    public void RequestLeaderboardFromMaster()
    {
        // Отправляем лидерборд игроку, запросившему его
        if (PhotonNetwork.IsMasterClient)
        {
            SyncLeaderboard();
        }
    }

    [PunRPC]
    public void SyncLeaderboard()
    {
        // Отправляем всем игрокам текущие данные
        foreach (var entry in leaderboard)
        {
            photonView.RPC("UpdatePlayerStats", RpcTarget.Others, entry.Key, entry.Value.kills, entry.Value.deaths, entry.Value.score);
        }
        OnLeaderboardUpdated?.Invoke();
    }

    [PunRPC]
    public void UpdatePlayerStats(string playerName, int kills, int deaths, int score)
    {
        // Обновляем данные игрока локально
        UpdateLeaderboard(playerName, kills, deaths, score);
    }

    public void UpdateLeaderboard(string playerName, int kills, int deaths, int score)
    {
        // Обновляем или добавляем игрока в лидерборд
        if (!leaderboard.ContainsKey(playerName))
        {
            leaderboard.Add(playerName, new PlayerStats { kills = kills, deaths = deaths, score = score });
            OnPlayerAdded?.Invoke(playerName, leaderboard[playerName]);
        }
        else
        {
            leaderboard[playerName] = new PlayerStats { kills = kills, deaths = deaths, score = score };
        }

        UpdateLeaderboardList();
        OnLeaderboardUpdated?.Invoke();
    }

    public void RemovePlayer(string playerName)
    {
        // Удаляем игрока из лидерборда
        if (leaderboard.ContainsKey(playerName))
        {
            leaderboard.Remove(playerName);
            leaderboardList.RemoveAll(p => p.playerName == playerName);
            OnPlayerRemoved?.Invoke(playerName, new PlayerStats());
            OnLeaderboardUpdated?.Invoke();
        }
    }

    private void UpdateLeaderboardList()
    {
        // Синхронизация списка для UI
        leaderboardList.Clear();
        foreach (var entry in leaderboard)
        {
            leaderboardList.Add(new PlayerEntry { playerName = entry.Key, stats = entry.Value });
        }
    }

    public List<PlayerEntry> GetLeaderboardList()
    {
        return leaderboardList;
    }

    public PlayerStats GetPlayerStats(string playerName)
    {
        return leaderboard.ContainsKey(playerName) ? leaderboard[playerName] : new PlayerStats();
    }

    public bool HasPlayer(string playerName)
    {
        return leaderboard.ContainsKey(playerName);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Сериализация данных для наблюдаемых компонентов Photon
        if (stream.IsWriting)
        {
            stream.SendNext(leaderboard.Count);
            foreach (var entry in leaderboard)
            {
                stream.SendNext(entry.Key);
                stream.SendNext(entry.Value.kills);
                stream.SendNext(entry.Value.deaths);
                stream.SendNext(entry.Value.score);
            }
        }
        else
        {
            leaderboard.Clear();
            int count = (int)stream.ReceiveNext();
            for (int i = 0; i < count; i++)
            {
                string playerName = (string)stream.ReceiveNext();
                int kills = (int)stream.ReceiveNext();
                int deaths = (int)stream.ReceiveNext();
                int score = (int)stream.ReceiveNext();
                leaderboard.Add(playerName, new PlayerStats { kills = kills, deaths = deaths, score = score });
            }
            UpdateLeaderboardList();
            OnLeaderboardUpdated?.Invoke();
        }
    }

    [PunRPC]
    public void RPC_AddScore(string playerName, int scoreToAdd)
    {
        // Вызывается клиентом и обрабатывается мастером для добавления очков
        if (PhotonNetwork.IsMasterClient)
        {
            var currentStats = GetPlayerStats(playerName);
            int newScore = currentStats.score + scoreToAdd;
            UpdateLeaderboard(playerName, currentStats.kills, currentStats.deaths, newScore);
            SyncLeaderboard();
        }
    }

    [PunRPC]
    public void RPC_AddKill(string playerName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var currentStats = GetPlayerStats(playerName);
            int newScore = currentStats.score + 30; // Добавляем 30 очков за убийство
            UpdateLeaderboard(playerName, currentStats.kills + 1, currentStats.deaths, newScore);
            SyncLeaderboard();
        }
    }

    [PunRPC]
    public void RPC_AddDeath(string playerName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var currentStats = GetPlayerStats(playerName);
            int newScore = currentStats.score - 10; // Отнимаем 10 очков за смерть
            UpdateLeaderboard(playerName, currentStats.kills, currentStats.deaths + 1, newScore);
            SyncLeaderboard();
        }
    }
}
