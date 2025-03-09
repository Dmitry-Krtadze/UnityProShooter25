using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public Text leaderboardText;  // ������ �� ��������� UI ������� ��� ����������� ������
    private LeaderBoardManager leaderBoardManager;

    private void Start()
    {

        leaderBoardManager = GameObject.FindGameObjectWithTag("LeaderBoardManager").GetComponent<LeaderBoardManager>();

        // ������������� �� ������� ���������� ����������
        leaderBoardManager.OnLeaderboardUpdated += UpdateLeaderboardUI;


    }

    private void OnDestroy()
    {
        // ������������ �� �������, ����� �������� ������ ������
        leaderBoardManager.OnLeaderboardUpdated -= UpdateLeaderboardUI;
    }

    // ����� ��� ���������� UI ����������
    void UpdateLeaderboardUI()
    {
        // ������� ������� �����
        leaderboardText.text = "";

        // �������� ������ ���� �������
        var leaderboard = leaderBoardManager.GetLeaderboardList();

        // ��������� ���� ������� � �����
        foreach (var player in leaderboard)
        {
            string playerName = player.playerName;
            LeaderBoardManager.PlayerStats stats = player.stats;
            leaderboardText.text += $"{playerName} - Kills: {stats.kills} Deaths: {stats.deaths} Score: {stats.score}\n";
        }
    }
}
