using SteelRain.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Text difficultyText;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject menuContent;

        private GameObject cachedPanel;
        private GameObject cachedMenu;

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
            if (newGameButton != null)
                newGameButton.onClick.AddListener(NewGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ToggleSettings);
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            if (easyButton != null)
                easyButton.onClick.AddListener(() => SetDifficulty(Difficulty.Easy));
            if (normalButton != null)
                normalButton.onClick.AddListener(() => SetDifficulty(Difficulty.Normal));
            if (hardButton != null)
                hardButton.onClick.AddListener(() => SetDifficulty(Difficulty.Hard));

            cachedPanel = settingsPanel;
            if (cachedPanel == null) cachedPanel = transform.Find("SettingsPanel")?.gameObject;
            if (cachedPanel != null) cachedPanel.SetActive(false);

            cachedMenu = menuContent;
            if (cachedMenu == null) cachedMenu = transform.Find("MenuContent")?.gameObject;

            UpdateDifficultyDisplay();
        }

        private void SetDifficulty(Difficulty diff)
        {
            DifficultyManager.SetDifficulty(diff);
            UpdateDifficultyDisplay();
        }

        private void UpdateDifficultyDisplay()
        {
            if (difficultyText != null)
                difficultyText.text = $"Difficulty: {DifficultyManager.GetDifficultyName()}";
        }

        private void StartGame()
        {
            Time.timeScale = 1f;
            SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void NewGame()
        {
            SaveSystem.ClearAll();
            ScoreManager.Reset();
            Time.timeScale = 1f;
            SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void ToggleSettings()
        {
            if (cachedPanel == null) return;
            var show = !cachedPanel.activeSelf;
            cachedPanel.SetActive(show);
            if (cachedMenu != null)
                cachedMenu.SetActive(!show);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
