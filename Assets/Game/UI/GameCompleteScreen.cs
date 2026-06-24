using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 游戏通关画面（合金弹头简化版）。
    /// 通关后显示总分和游戏时间，无评级、无NGP。
    /// </summary>
    public sealed class GameCompleteScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text statsText;
        [SerializeField] private Button menuButton;

        private bool shown;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (menuButton != null)
                menuButton.onClick.AddListener(ReturnToMenu);
        }

        public void TryShow()
        {
            if (shown) return;

            var levelsCompleted = AchievementManager.GetFloatStat(AchievementManager.StatId.LevelsCompleted);
            if (levelsCompleted < LevelManager.TotalLevels) return;

            shown = true;
            if (panel != null) panel.SetActive(true);

            // 通关后解锁无尽模式
            SaveSystem.UnlockEndlessMode();

            var totalPlayTime = AchievementManager.GetFloatStat(AchievementManager.StatId.TotalPlayTime);
            var totalScore = ScoreManager.HighScore;

            if (titleText != null)
                titleText.text = "CAMPAIGN COMPLETE!";

            if (statsText != null)
            {
                var hours = (int)(totalPlayTime / 3600f);
                var mins = (int)((totalPlayTime % 3600f) / 60f);
                statsText.text =
                    $"High Score: {totalScore}\n" +
                    $"Total Play Time: {hours}h {mins}m\n" +
                    $"Enemies Defeated: {AchievementManager.GetFloatStat(AchievementManager.StatId.TotalKills)}";
            }

            AudioManager.Play("sfx_victory", 1f);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelManager.ReturnToMenu();
        }
    }
}
