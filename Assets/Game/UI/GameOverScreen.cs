using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Text scoreText;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (retryButton != null)
                retryButton.onClick.AddListener(Retry);
            if (menuButton != null)
                menuButton.onClick.AddListener(ReturnToMenu);
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            ScoreManager.CheckHighScore();
            if (scoreText != null)
                scoreText.text = $"Score: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";
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
