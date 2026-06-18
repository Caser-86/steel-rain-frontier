using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Text scoreText;

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
            if (panel != null) panel.SetActive(true);
            ScoreManager.CheckHighScore();
            if (scoreText != null)
                scoreText.text = $"Score: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";

            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(LevelManager.CurrentLevel < LevelManager.TotalLevels - 1);

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
