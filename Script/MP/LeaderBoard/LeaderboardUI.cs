using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public Text leaderboardText;  // Ссылка на текстовый UI элемент для отображения лидера
    private LeaderBoardManager leaderBoardManager;

    private void Start()
    {

        leaderBoardManager = GameObject.FindGameObjectWithTag("LeaderBoardManager").GetComponent<LeaderBoardManager>();

        // Подписываемся на событие обновления лидерборда
        leaderBoardManager.OnLeaderboardUpdated += UpdateLeaderboardUI;


    }

    private void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать утечек памяти
        leaderBoardManager.OnLeaderboardUpdated -= UpdateLeaderboardUI;
    }

    // Метод для обновления UI лидерборда
    void UpdateLeaderboardUI()
    {
        // Очищаем текущий текст
        leaderboardText.text = "";

        // Получаем список всех игроков
        var leaderboard = leaderBoardManager.GetLeaderboardList();

        // Добавляем всех игроков в текст
        foreach (var player in leaderboard)
        {
            string playerName = player.playerName;
            LeaderBoardManager.PlayerStats stats = player.stats;
            leaderboardText.text += $"{playerName} - Kills: {stats.kills} Deaths: {stats.deaths} Score: {stats.score}\n";
        }
    }
}
