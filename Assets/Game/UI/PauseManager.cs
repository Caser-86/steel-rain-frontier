using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SteelRain.UI
{
    /// <summary>
    /// 暂停菜单：ESC 暂停/恢复，Q 退出游戏。
    /// </summary>
    public sealed class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Text pauseTitle;
        [SerializeField] private Text pauseHint;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode quitKey = KeyCode.Q;
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private bool paused;

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (paused) Resume();
                else Pause();
            }

            if (paused)
            {
                if (Input.GetKeyDown(restartKey))
                    Restart();
                else if (Input.GetKeyDown(quitKey))
                    ReturnToMenu();
            }
        }

        public void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneFader.FadeToScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneFader.FadeToScene("MainMenu");
        }
    }
}
