using SteelRain.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class CharacterSelectController : MonoBehaviour
    {
        public const string LevelSceneName = "Level01_VerticalSlice";
        public const string MainMenuSceneName = "MainMenu";

        [SerializeField] private Text statusLabel;

        public void SelectAila()
        {
            StartNewGame(0);
        }

        public void SelectBruno()
        {
            StartNewGame(1);
        }

        public void SelectMara()
        {
            StartNewGame(2);
        }

        public void SelectNiko()
        {
            StartNewGame(3);
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public void StartNewGame(int selectedCharacterIndex)
        {
            var save = CreateNewGameSave(selectedCharacterIndex);
            SaveService.Save(save);
            SaveService.QueuePendingLoad(save);
            SceneManager.LoadScene(LevelSceneName);
        }

        public static SaveData CreateNewGameSave(int selectedCharacterIndex)
        {
            return new SaveData
            {
                levelId = "Level01",
                selectedCharacterIndex = Mathf.Clamp(selectedCharacterIndex, 0, 3),
                weaponId = "assault_rifle",
                weaponLevel = 0,
                level01Cleared = false
            };
        }

        private void Start()
        {
            if (statusLabel != null)
                statusLabel.text = "Choose squad lead. You can switch characters in mission.";
        }
    }
}
