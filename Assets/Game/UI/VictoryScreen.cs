using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 关卡胜利结算屏（合金弹头简化版）。
    /// 仅显示：分数、最高分、通关时间、存活人数。
    /// 无评级、无商店入口。
    /// </summary>
    public sealed class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timeText;
        [SerializeField] private GameCompleteScreen gameCompleteScreen;

        private bool shown;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (menuButton != null)
                menuButton.onClick.AddListener(ReturnToMenu);
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(NextLevel);
            GameEvents.BossDefeated += Show;
        }

        private void OnDestroy()
        {
            GameEvents.BossDefeated -= Show;
        }

        public void Show()
        {
            if (shown) return;
            // 无尽模式下不显示胜利屏（Boss 只是波次的一部分）
            if (LevelManager.InEndlessMode) return;

            shown = true;
            if (panel != null) panel.SetActive(true);
            ScoreManager.Save();
            ScoreManager.CheckHighScore();

            var playTime = Time.timeSinceLevelLoad;
            var squad = FindFirstObjectByType<PlayerSquad>();
            int aliveCount = squad != null ? squad.AliveCount : 1;
            int totalCount = squad != null ? squad.MemberCount : 1;

            if (scoreText != null)
                scoreText.text = $"Score: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";

            if (timeText != null)
            {
                var min = (int)(playTime / 60f);
                var sec = (int)(playTime % 60f);
                timeText.text = $"Time: {min:D2}:{sec:D2}  Survivors: {aliveCount}/{totalCount}";
            }

            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(LevelManager.CurrentLevel < LevelManager.TotalLevels - 1);

            // 最后一关通关→显示 GameCompleteScreen
            bool isLastLevel = LevelManager.CurrentLevel >= LevelManager.TotalLevels - 1;
            if (isLastLevel && gameCompleteScreen != null)
            {
                if (panel != null) panel.SetActive(false);
                gameCompleteScreen.TryShow();
            }

            AudioManager.Play("sfx_victory", 0.8f);
            Time.timeScale = 0f;
        }

        private void NextLevel()
        {
            Time.timeScale = 1f;
            LevelManager.LoadNextLevel();
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelManager.ReturnToMenu();
        }
    }
}
