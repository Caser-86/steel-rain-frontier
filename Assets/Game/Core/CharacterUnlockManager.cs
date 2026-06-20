using System;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 角色解锁系统：通过成就/进度解锁新角色，为玩家提供长期追求目标。
    /// 初始仅 Aila 可用，其他角色需达成条件解锁。
    /// </summary>
    public static class CharacterUnlockManager
    {
        private const string KeyPrefix = "Unlock_";

        public static event Action<string> CharacterUnlocked;

        static CharacterUnlockManager()
        {
            // 订阅成就解锁事件，自动检查角色解锁条件
            AchievementManager.AchievementUnlocked += _ => CheckUnlocks();
        }

        /// <summary>
        /// 角色解锁条件定义。
        /// </summary>
        public static bool IsUnlocked(string characterId)
        {
            // Aila 始终可用
            if (characterId == "aila") return true;
            return PlayerPrefs.GetInt(KeyPrefix + characterId, 0) == 1;
        }

        /// <summary>
        /// 解锁指定角色。
        /// </summary>
        public static void Unlock(string characterId)
        {
            if (IsUnlocked(characterId)) return;
            PlayerPrefs.SetInt(KeyPrefix + characterId, 1);
            PlayerPrefs.Save();
            Debug.Log($"[CharacterUnlock] Unlocked: {characterId}");
            CharacterUnlocked?.Invoke(characterId);
        }

        /// <summary>
        /// 检查并执行所有解锁条件（在成就解锁、关卡完成时调用）。
        /// </summary>
        public static void CheckUnlocks()
        {
            // Bruno：完成关卡1解锁
            if (AchievementManager.IsUnlocked(AchievementManager.AchievementId.Level1Complete))
                Unlock("bruno");

            // Mara：累计击杀100敌人解锁
            if (AchievementManager.GetStat(AchievementManager.StatId.TotalKills) >= 100)
                Unlock("mara");

            // Niko：通关整个游戏解锁
            if (AchievementManager.IsUnlocked(AchievementManager.AchievementId.GameComplete))
                Unlock("niko");
        }

        /// <summary>
        /// 重置所有解锁状态（用于测试）。
        /// </summary>
        public static void Reset()
        {
            PlayerPrefs.DeleteKey(KeyPrefix + "bruno");
            PlayerPrefs.DeleteKey(KeyPrefix + "mara");
            PlayerPrefs.DeleteKey(KeyPrefix + "niko");
            PlayerPrefs.Save();
        }
    }
}
