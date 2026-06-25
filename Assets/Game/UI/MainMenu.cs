using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button endlessButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button characterSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Text difficultyText;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject menuContent;

        private GameObject cachedPanel;
        private GameObject cachedMenu;
        private AchievementPanel achievementPanel;
        private CharacterSelectScreen characterSelectScreen;

        private void Start()
        {
            // 检查EventSystem
            var es = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();

            // 如果没有EventSystem，自动创建
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
            if (continueButton != null)
            {
                // 有存档时才显示继续游戏按钮
                var hasSave = SaveSystem.HasSquadSave();
                continueButton.gameObject.SetActive(hasSave);
                continueButton.onClick.AddListener(ContinueGame);
            }
            if (newGameButton != null)
                newGameButton.onClick.AddListener(NewGame);
            if (endlessButton != null)
            {
                // 通关后才解锁无尽模式
                var unlocked = SaveSystem.IsEndlessUnlocked() ||
                               AchievementManager.IsUnlocked(AchievementManager.AchievementId.GameComplete);
                endlessButton.gameObject.SetActive(unlocked);
                endlessButton.onClick.AddListener(StartEndlessMode);

                // 显示最高波次记录
                if (unlocked)
                {
                    var bestWave = SaveSystem.GetEndlessBestWave();
                    var text = endlessButton.GetComponentInChildren<Text>();
                    if (text != null && bestWave > 0)
                        text.text = $"Endless Mode (Best: Wave {bestWave})";
                }
            }
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ToggleSettings);
            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(ToggleAchievements);
            if (characterSelectButton != null)
                characterSelectButton.onClick.AddListener(ToggleCharacterSelect);
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

        private void Update()
        {
            // 设置面板打开时，ESC关闭设置面板而不是退出游戏
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (cachedPanel != null && cachedPanel.activeSelf)
                {
                    ToggleSettings();
                    return;
                }
                // 只在主菜单（非设置面板）时才退出游戏
                if (cachedMenu == null || cachedMenu.activeSelf)
                {
                    QuitGame();
                }
                return;
            }

            // 键盘快捷键：空格或回车开始游戏（仅在主菜单显示时）
            if (cachedMenu != null && cachedMenu.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    StartGame();
                }
            }
        }

        private void SetDifficulty(Difficulty diff)
        {
            DifficultyManager.SetDifficulty(diff);
            UpdateDifficultyDisplay();
        }

        private void UpdateDifficultyDisplay()
        {
            if (difficultyText != null)
                difficultyText.text = DifficultyManager.GetDifficultyName();
        }

        private void StartGame()
        {
            Time.timeScale = 1f;
            SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void ContinueGame()
        {
            Time.timeScale = 1f;
            // 继续到上一次检查点所在的关卡
            var levelIndex = SaveSystem.LoadLevelIndex();
            if (levelIndex >= 0 && levelIndex < LevelManager.TotalLevels)
                LevelManager.LoadLevel(levelIndex);
            else
                SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void NewGame()
        {
            SaveSystem.ClearAll();
            ScoreManager.Reset();
            TempBuffState.Reset();
            Time.timeScale = 1f;
            SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void StartEndlessMode()
        {
            ScoreManager.Reset();
            Time.timeScale = 1f;
            LevelManager.LoadEndlessMode();
        }

        private void ToggleSettings()
        {
            if (cachedPanel == null) return;
            var show = !cachedPanel.activeSelf;
            cachedPanel.SetActive(show);
            if (cachedMenu != null)
                cachedMenu.SetActive(!show);
        }

        private void ToggleAchievements()
        {
            if (achievementPanel == null)
                achievementPanel = FindFirstObjectByType<AchievementPanel>();
            if (achievementPanel == null)
            {
                var go = new GameObject("AchievementPanel");
                go.transform.SetParent(transform);
                achievementPanel = go.AddComponent<AchievementPanel>();
            }
            if (achievementPanel.gameObject.activeSelf)
            {
                achievementPanel.Hide();
                if (cachedMenu != null)
                    cachedMenu.SetActive(true);
            }
            else
            {
                if (cachedMenu != null)
                    cachedMenu.SetActive(false);
                achievementPanel.Show();
            }
        }

        private void ToggleCharacterSelect()
        {
            if (characterSelectScreen == null)
                characterSelectScreen = FindFirstObjectByType<CharacterSelectScreen>();
            if (characterSelectScreen == null)
            {
                var go = new GameObject("CharacterSelectScreen");
                go.transform.SetParent(transform);
                characterSelectScreen = go.AddComponent<CharacterSelectScreen>();
            }
            if (cachedMenu != null)
                cachedMenu.SetActive(false);
            characterSelectScreen.Show();
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
