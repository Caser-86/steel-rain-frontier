using SteelRain.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Text statusLabel;

        private const string LevelSceneName = "Level01_VerticalSlice";

        private void Start()
        {
            if (continueButton != null)
                continueButton.interactable = SaveService.HasSave;

            SetStatus(SaveService.HasSave ? "Save found. Continue ready." : "No save yet. Start new mission.");
        }

        public void ContinueGame()
        {
            var save = SaveService.Load();
            if (save == null)
            {
                SetStatus("No save file found.");
                return;
            }

            SaveService.QueuePendingLoad(save);
            SceneManager.LoadScene(LevelSceneName);
        }

        public void NewGame()
        {
            SaveService.Delete();
            SceneManager.LoadScene(LevelSceneName);
        }

        public void OpenSettings()
        {
            SetStatus("Settings coming later. Current build uses keyboard controls.");
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
                statusLabel.text = text;
        }
    }
}
