using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// 暂停菜单：ESC 暂停/恢复，S 设置，R 重启，Q 退出。
    /// </summary>
    public sealed class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Text pauseTitle;
        [SerializeField] private Text pauseHint;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode quitKey = KeyCode.Q;
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private bool paused;
        private bool settingsOpen;

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (masterSlider != null)
            {
                masterSlider.value = AudioManager.GetMasterVolume();
                masterSlider.onValueChanged.AddListener(v => AudioManager.SetMasterVolume(v));
            }
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.GetMusicVolume();
                musicSlider.onValueChanged.AddListener(v => AudioManager.SetMusicVolume(v));
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.GetSfxVolume();
                sfxSlider.onValueChanged.AddListener(v => AudioManager.SetSfxVolume(v));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (settingsOpen) { CloseSettings(); return; }
                if (paused) Resume();
                else Pause();
            }

            if (paused && !settingsOpen)
            {
                if (Input.GetKeyDown(KeyCode.S))
                    OpenSettings();
                else if (Input.GetKeyDown(restartKey))
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
            settingsOpen = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            settingsOpen = true;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
            // 刷新滑块值
            if (masterSlider != null) masterSlider.value = AudioManager.GetMasterVolume();
            if (musicSlider != null) musicSlider.value = AudioManager.GetMusicVolume();
            if (sfxSlider != null) sfxSlider.value = AudioManager.GetSfxVolume();
        }

        public void CloseSettings()
        {
            settingsOpen = false;
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            LevelManager.ReloadCurrentLevel();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelManager.ReturnToMenu();
        }
    }
}
