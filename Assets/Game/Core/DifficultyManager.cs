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

        /// <summary>
        /// Easy 难度自动回血间隔（秒）。返回 0 表示不自动回血。
        /// 为新手玩家提供容错，避免卡关挫败。
        /// </summary>
        public static float GetAutoHealInterval()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => 5f,
                Difficulty.Normal => 0f,
                Difficulty.Hard => 0f,
                _ => 0f
            };
        }

        /// <summary>
        /// Hard 难度下 Boss 是否启用额外阶段（第四阶段）。
        /// 为硬核玩家提供机制层面的挑战，而非单纯数值堆叠。
        /// </summary>
        public static bool HasBossExtraPhase()
        {
            return currentDifficulty == Difficulty.Hard;
        }

        /// <summary>
        /// Hard 难度下敌人攻击 cooldown 缩减倍率（越小越激进）。
        /// </summary>
        public static float GetEnemyAttackCooldownMultiplier()
        {
            return currentDifficulty switch
            {
                Difficulty.Easy => 1.3f,
                Difficulty.Normal => 1f,
                Difficulty.Hard => 0.7f,
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
