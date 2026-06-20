using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
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
        [SerializeField] private Button reviveButton;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text reviveText;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (retryButton != null)
                retryButton.onClick.AddListener(Retry);
            if (menuButton != null)
                menuButton.onClick.AddListener(ReturnToMenu);
            if (reviveButton != null)
                reviveButton.onClick.AddListener(UseRevive);
        }

        private bool shown;

        public bool IsShown => panel != null && panel.activeSelf;

        public void Show()
        {
            if (shown) return;
            shown = true;
            if (panel != null) panel.SetActive(true);
            ScoreManager.Save();
            ScoreManager.CheckHighScore();
            if (scoreText != null)
                scoreText.text = $"Score: {ScoreManager.Score}\nHigh Score: {ScoreManager.HighScore}";

            // 显示复活信标按钮（仅在有信标时）
            UpdateReviveButton();

            AudioManager.Play("sfx_gameover", 0.7f);
            Time.timeScale = 0f;
        }

        private void UpdateReviveButton()
        {
            if (reviveButton == null) return;
            var hasRevive = ShopManager.ReviveCount > 0;
            reviveButton.gameObject.SetActive(hasRevive);
            if (reviveText != null)
                reviveText.text = $"使用复活信标 ({ShopManager.ReviveCount})";
        }

        private void UseRevive()
        {
            if (!ShopManager.ConsumeRevive()) return;

            // 复活：恢复时间，隐藏面板，复活所有角色
            Time.timeScale = 1f;
            shown = false;
            if (panel != null) panel.SetActive(false);

            // 找到 PlayerSquad 并复活所有角色
            var squad = FindFirstObjectByType<PlayerSquad>();
            if (squad != null) squad.ReviveAll();

            AudioManager.Play("sfx_upgrade", 0.8f);
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
