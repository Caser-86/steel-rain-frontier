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
            Debug.Log("[MainMenu] Start called");

            // 检查EventSystem
            var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            Debug.Log("[MainMenu] EventSystem found: " + (es != null));

            // 如果没有EventSystem，自动创建
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[MainMenu] EventSystem auto-created");
            }

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
            Debug.Log("[MainMenu] Start complete. startButton=" + (startButton != null) + " newGameButton=" + (newGameButton != null));
        }

        private void Update()
        {
            // 键盘快捷键：空格或回车开始游戏
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (cachedMenu == null || cachedMenu.activeSelf)
                {
                    StartGame();
                }
            }
            // ESC退出
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
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
            Debug.Log("[MainMenu] StartGame clicked, fading to Level01_VerticalSlice");
            Time.timeScale = 1f;
            SceneFader.FadeToScene("Level01_VerticalSlice");
        }

        private void NewGame()
        {
            Debug.Log("[MainMenu] NewGame clicked");
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
