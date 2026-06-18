using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Core
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public static class DifficultyManager
    {
        private static Difficulty currentDifficulty = Difficulty.Normal;
        private const string KeyDifficulty = "Settings_Difficulty";

        public static Difficulty Current => currentDifficulty;

        static DifficultyManager()
        {
            currentDifficulty = (Difficulty)PlayerPrefs.GetInt(KeyDifficulty, 1);
        }

        public static void SetDifficulty(Difficulty diff)
        {
            currentDifficulty = diff;
            PlayerPrefs.SetInt(KeyDifficulty, (int)diff);
            PlayerPrefs.Save();
        }

        public static float GetHealthMultiplier()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => 0.7f,
                Difficulty.Normal => 1f,
                Difficulty.Hard => 1.4f,
                _ => 1f
            };
        }

        public static float GetDamageMultiplier()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => 0.6f,
                Difficulty.Normal => 1f,
                Difficulty.Hard => 1.5f,
                _ => 1f
            };
        }

        public static float GetEnemySpeedMultiplier()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => 0.8f,
                Difficulty.Normal => 1f,
                Difficulty.Hard => 1.2f,
                _ => 1f
            };
        }

        public static string GetDifficultyName()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => "Easy",
                Difficulty.Normal => "Normal",
                Difficulty.Hard => "Hard",
                _ => "Normal"
            };
        }
    }
}
