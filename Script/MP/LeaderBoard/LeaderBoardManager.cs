using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class LeaderBoardManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static LeaderBoardManager Instance { get; private set; }

    public delegate void PlayerStatsUpdated(string playerName, PlayerStats stats);
    public event PlayerStatsUpdated OnPlayerAdded;
    public event PlayerStatsUpdated OnPlayerRemoved;
    public event System.Action OnLeaderboardUpdated;

    [SerializeField] public List<PlayerEntry> leaderboardList = new List<PlayerEntry>();
    private Dictionary<string, PlayerStats> leaderboard = new Dictionary<string, PlayerStats>();

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RequestLeaderboard", RpcTarget.MasterClient);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateLeaderboard(newPlayer.NickName, 0, 0, 0);
            OnPlayerAdded?.Invoke(newPlayer.NickName, new PlayerStats { kills = 0, deaths = 0, score = 0 });

            photonView.RPC("UpdatePlayerStats", RpcTarget.Others, newPlayer.NickName, 0, 0, 0);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RemovePlayer(otherPlayer.NickName);
            OnPlayerRemoved?.Invoke(otherPlayer.NickName, new PlayerStats());
        }
    }

    [PunRPC]
    public void RequestLeaderboard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncLeaderboard", RpcTarget.Others);
        }
    }

    [PunRPC]
    public void SyncLeaderboard()
    {
        foreach (var entry in leaderboard)
        {
            photonView.RPC("UpdatePlayerStats", RpcTarget.Others, entry.Key, entry.Value.kills, entry.Value.deaths, entry.Value.score);
        }
    }

    [PunRPC]
    public void UpdatePlayerStats(string playerName, int kills, int deaths, int score)
    {
        UpdateLeaderboard(playerName, kills, deaths, score);
    }

    public void UpdateLeaderboard(string playerName, int kills, int deaths, int score)
    {
        if (!leaderboard.ContainsKey(playerName))
        {
            leaderboard.Add(playerName, new PlayerStats { kills = kills, deaths = deaths, score = score });
        }
        else
        {
            PlayerStats playerStats = leaderboard[playerName];
            playerStats.kills += kills;
            playerStats.deaths += deaths;
            playerStats.score += score;
            leaderboard[playerName] = playerStats;
        }

        UpdateLeaderboardList();
        OnLeaderboardUpdated?.Invoke();
    }

    public void RemovePlayer(string playerName)
    {
        if (leaderboard.ContainsKey(playerName))
        {
            leaderboard.Remove(playerName);
            UpdateLeaderboardList();
        }
    }

    private void UpdateLeaderboardList()
    {
        leaderboardList.Clear();
        foreach (var entry in leaderboard)
        {
            leaderboardList.Add(new PlayerEntry { playerName = entry.Key, stats = entry.Value });
        }
        Debug.Log($"Leaderboard updated: {leaderboardList.Count} players.");
    }

    public List<PlayerEntry> GetLeaderboardList()
    {
        return leaderboardList;
    }

    public PlayerStats GetPlayerStats(string playerName)
    {
        if (leaderboard.ContainsKey(playerName))
        {
            return leaderboard[playerName];
        }
        return new PlayerStats();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
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
            int count = (int)stream.ReceiveNext();
            for (int i = 0; i < count; i++)
            {
                string playerName = (string)stream.ReceiveNext();
                int kills = (int)stream.ReceiveNext();
                int deaths = (int)stream.ReceiveNext();
                int score = (int)stream.ReceiveNext();

                if (leaderboard.ContainsKey(playerName))
                {
                    leaderboard[playerName] = new PlayerStats { kills = kills, deaths = deaths, score = score };
                }
                else
                {
                    leaderboard.Add(playerName, new PlayerStats { kills = kills, deaths = deaths, score = score });
                }
            }
            UpdateLeaderboardList();
            OnLeaderboardUpdated?.Invoke();
        }
    }
}
