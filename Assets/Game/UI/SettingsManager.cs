using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class SettingsManager : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject panel;

        private float masterVolume = 1f;
        private float musicVolume = 0.7f;
        private float sfxVolume = 1f;
        private bool isFullscreen = true;

        private void Awake()
        {
            LoadSettings();
            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);
            if (backButton != null)
                backButton.onClick.AddListener(() => gameObject.SetActive(false));

            // 实时预览：滑块变化时立即应用音量，无需点确认
            if (masterSlider != null)
                masterSlider.onValueChanged.AddListener(v => { AudioManager.SetMasterVolume(v); });
            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(v => { AudioManager.SetMusicVolume(v); });
            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(v => { AudioManager.SetSfxVolume(v); });
        }

        private void OnEnable()
        {
            LoadSettings();
            UpdateUI();
        }

        private void LoadSettings()
        {
            masterVolume = SaveSystem.LoadMasterVolume();
            musicVolume = SaveSystem.LoadMusicVolume();
            sfxVolume = SaveSystem.LoadSfxVolume();
            isFullscreen = SaveSystem.LoadFullscreen();
        }

        private void UpdateUI()
        {
            if (masterSlider != null) masterSlider.value = masterVolume;
            if (musicSlider != null) musicSlider.value = musicVolume;
            if (sfxSlider != null) sfxSlider.value = sfxVolume;
            if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        }

        public void ApplySettings()
        {
            if (masterSlider != null) masterVolume = masterSlider.value;
            if (musicSlider != null) musicVolume = musicSlider.value;
            if (sfxSlider != null) sfxVolume = sfxSlider.value;
            if (fullscreenToggle != null) isFullscreen = fullscreenToggle.isOn;

            AudioManager.SetMasterVolume(masterVolume);
            AudioManager.SetMusicVolume(musicVolume);
            AudioManager.SetSfxVolume(sfxVolume);

            Screen.fullScreen = isFullscreen;
            if (isFullscreen)
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

            SaveSystem.SaveVolume(masterVolume, musicVolume, sfxVolume);
            SaveSystem.SaveDisplaySettings(isFullscreen, Screen.width, Screen.height);
        }
    }
}
