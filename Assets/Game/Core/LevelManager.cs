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

        public static void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Length) return;
            currentLevelIndex = index;
            inEndlessMode = false;
            // 切换关卡时清理上一关的武器拾取注册表
            WeaponPickup.ClearRegistry();
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene(levels[index]);
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
                WeaponPickup.ClearRegistry();
                Time.timeScale = 1f;
                UI.SceneFader.FadeToScene("MainMenu");
                return;
            }
            SaveSystem.ClearSquadSave();
            WeaponPickup.ClearRegistry();
            LoadLevel(nextIndex);
        }

        public static void ReloadCurrentLevel()
        {
            SaveSystem.ClearSquadSave();
            WeaponPickup.ClearRegistry();
            if (inEndlessMode)
            {
                Time.timeScale = 1f;
                UI.SceneFader.FadeToScene("EndlessMode");
                return;
            }
            LoadLevel(currentLevelIndex);
        }

        public static void ReturnToMenu()
        {
            currentLevelIndex = 0;
            inEndlessMode = false;
            // 返回主菜单时清理武器拾取注册表，避免静态列表残留已销毁对象的引用
            WeaponPickup.ClearRegistry();
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene("MainMenu");
        }

        public static void LoadEndlessMode()
        {
            currentLevelIndex = 0;
            inEndlessMode = true;
            WeaponPickup.ClearRegistry();
            Time.timeScale = 1f;
            UI.SceneFader.FadeToScene("EndlessMode");
        }
    }
}
