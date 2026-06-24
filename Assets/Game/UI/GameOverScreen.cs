using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 游戏结束屏（合金弹头简化版）。
    /// 仅提供：重试、返回菜单。无复活信标。
    /// </summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Text scoreText;

        private bool shown;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (retryButton != null)
                retryButton.onClick.AddListener(Retry);
            if (menuButton != null)
                menuButton.onClick.AddListener(ReturnToMenu);
        }

        public bool IsShown => panel != null && panel.activeSelf;

        public void Show()
        {
            if (shown) return;
            shown = true;
            if (panel != null) panel.SetActive(true);
            ScoreManager.Save();
            ScoreManager.CheckHighScore();
            if (scoreText != null)
            {
                // 无尽模式下额外显示波次
                if (LevelManager.InEndlessMode)
                {
                    var bestWave = SaveSystem.GetEndlessBestWave();
                    scoreText.text = $"Wave: {EndlessMode.CurrentWaveStatic}\nBest Wave: {bestWave}\nScore: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";
                }
                else
                    scoreText.text = $"Score: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";
            }

            AudioManager.Play("sfx_gameover", 0.7f);
            Time.timeScale = 0f;
        }

        private void Retry()
        {
            Time.timeScale = 1f;
            LevelManager.ReloadCurrentLevel();
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelManager.ReturnToMenu();
        }
    }
}
