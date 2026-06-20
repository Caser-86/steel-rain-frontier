using SteelRain.Pickups;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.Core
{
    public static class LevelManager
    {
        private static readonly string[] levels =
        {
            "Level01_VerticalSlice",
            "Level02_Factory"
        };

        private static int currentLevelIndex = 0;

        public static int CurrentLevel => currentLevelIndex;
        public static int TotalLevels => levels.Length;
        public static string CurrentLevelName => levels[currentLevelIndex];

        public static void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Length) return;
            currentLevelIndex = index;
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene(levels[index]);
        }

        public static void LoadNextLevel()
        {
            var nextIndex = currentLevelIndex + 1;
            if (nextIndex >= levels.Length)
            {
                currentLevelIndex = 0;
                ScoreManager.Reset();
                SaveSystem.ClearSquadSave();
                WeaponUpgradePickup.ClearRegistry();
                Time.timeScale = 1f;
                UI.SceneFader.FadeToScene("MainMenu");
                return;
            }
            SaveSystem.ClearSquadSave();
            WeaponUpgradePickup.ClearRegistry();
            LoadLevel(nextIndex);
        }

        public static void ReloadCurrentLevel()
        {
            SaveSystem.ClearSquadSave();
            WeaponUpgradePickup.ClearRegistry();
            LoadLevel(currentLevelIndex);
        }

        public static void ReturnToMenu()
        {
            currentLevelIndex = 0;
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene("MainMenu");
        }

        public static void LoadEndlessMode()
        {
            currentLevelIndex = 0;
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene("Level01_VerticalSlice");
        }
    }
}
