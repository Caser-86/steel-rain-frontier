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

        private bool pendingNewGameOverwrite;

        private const string LevelSceneName = "Level01_VerticalSlice";
        public const string NewGameSceneName = "CharacterSelect";
        public const string LevelSelectSceneName = "LevelSelect";
        public const string SaveSlotsSceneName = "SaveSlots";

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
            if (SaveService.HasSave && !pendingNewGameOverwrite)
            {
                pendingNewGameOverwrite = true;
                SetStatus(FormatOverwritePrompt(SaveService.CurrentSlot));
                return;
            }

            SaveService.Delete();
            SceneManager.LoadScene(NewGameSceneName);
        }

        public static string FormatOverwritePrompt(int slot)
        {
            return $"Slot {SaveService.NormalizeSlot(slot)} has save. Press NEW GAME again to overwrite.";
        }

        public void OpenLevelSelect()
        {
            SceneManager.LoadScene(LevelSelectSceneName);
        }

        public void OpenSaveSlots()
        {
            SceneManager.LoadScene(SaveSlotsSceneName);
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
