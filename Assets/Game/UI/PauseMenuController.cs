using SteelRain.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.UI
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CheckpointManager checkpoints;

        private bool paused;

        private void Awake()
        {
            SetPaused(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                SetPaused(!paused);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        public void RestartCheckpoint()
        {
            if (checkpoints != null)
                checkpoints.RestoreCheckpoint(checkpoints.CurrentSpawn);

            SetPaused(false);
        }

        public void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }

        private void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = paused ? 0f : 1f;

            if (panel != null)
                panel.SetActive(paused);
        }
    }
}
