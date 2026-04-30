using SteelRain.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class LevelSelectController : MonoBehaviour
    {
        private const string LevelOneId = "Level01";
        private const string LevelOneSceneName = "Level01_VerticalSlice";
        private const string MainMenuSceneName = "MainMenu";

        [SerializeField] private Text statusLabel;

        private void Start()
        {
            SetStatus("Level 01 ready. Level 02 locked for later build.");
        }

        public void StartLevelOne()
        {
            var save = SaveService.Load() ?? CharacterSelectController.CreateNewGameSave(0);
            save.levelId = LevelOneId;
            SaveService.Save(save);
            SaveService.QueuePendingLoad(save);
            SceneManager.LoadScene(LevelOneSceneName);
        }

        public void ShowLevelTwoLocked()
        {
            SetStatus("Level 02 locked. Climbing stage comes later.");
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public static bool IsLevelUnlocked(string levelId, bool level01Cleared)
        {
            return levelId == LevelOneId;
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
                statusLabel.text = text;
        }
    }
}
