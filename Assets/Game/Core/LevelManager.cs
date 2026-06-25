using SteelRain.Pickups;
using UnityEngine;

namespace SteelRain.Core
{
    public static class LevelManager
    {
        private static readonly string[] levels =
        {
            "Level01_VerticalSlice",
            "Level02_Factory",
            "Level03_Warzone",
            "Level04_Bunker",
            "Level05_Citadel"
        };

        private static int currentLevelIndex = 0;
        private static bool inEndlessMode = false;

        public static int CurrentLevel => currentLevelIndex;
        public static int TotalLevels => levels.Length;
        public static string CurrentLevelName => levels[currentLevelIndex];
        public static bool InEndlessMode => inEndlessMode;

        private static void LoadScene(string sceneName)
        {
            WeaponPickup.ClearRegistry();
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene(sceneName);
        }

        public static void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Length) return;
            currentLevelIndex = index;
            inEndlessMode = false;
            LoadScene(levels[index]);
        }

        public static void LoadNextLevel()
        {
            var nextIndex = currentLevelIndex + 1;
            if (nextIndex >= levels.Length)
            {
                currentLevelIndex = 0;
                inEndlessMode = false;
                ScoreManager.Reset();
                SaveSystem.ClearSquadSave();
                LoadScene("MainMenu");
                return;
            }
            SaveSystem.ClearSquadSave();
            LoadLevel(nextIndex);
        }

        public static void ReloadCurrentLevel()
        {
            SaveSystem.ClearSquadSave();
            if (inEndlessMode)
            {
                LoadScene("EndlessMode");
                return;
            }
            LoadLevel(currentLevelIndex);
        }

        public static void ReturnToMenu()
        {
            currentLevelIndex = 0;
            inEndlessMode = false;
            LoadScene("MainMenu");
        }

        public static void LoadEndlessMode()
        {
            currentLevelIndex = 0;
            inEndlessMode = true;
            LoadScene("EndlessMode");
        }
    }
}
