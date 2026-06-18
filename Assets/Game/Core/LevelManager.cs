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
            currentLevelIndex++;
            if (currentLevelIndex >= levels.Length)
            {
                UI.SceneFader.FadeToScene("MainMenu");
                return;
            }
            LoadLevel(currentLevelIndex);
        }

        public static void ReloadCurrentLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        public static void ReturnToMenu()
        {
            currentLevelIndex = 0;
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene("MainMenu");
        }
    }
}
