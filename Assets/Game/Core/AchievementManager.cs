using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 成就系统：跟踪玩家成就解锁状态和游戏统计。
    /// 成就通过 PlayerPrefs 持久化保存。
    /// </summary>
    public static class AchievementManager
    {
        private const string KeyPrefix = "Achievement_";
        private const string KeyStatsPrefix = "Stats_";

        // 成就定义
        public enum AchievementId
        {
            FirstBlood,          // 首次击杀
            Kill10,              // 击杀10个敌人
            Kill50,              // 击杀50个敌人
            Kill100,             // 击杀100个敌人
            FirstBoss,           // 首次击败Boss
            NoDeathComplete,     // 无死亡完成关卡
            PacifistRun,         // 和平主义者（不击杀任何敌人完成关卡）
            Combo5,              // 5连击
            Combo10,             // 10连击
            Combo20,             // 20连击
            AllCharactersUsed,   // 使用所有4个角色
            WeaponMaster,        // 所有武器升级到满级
            Level1Complete,      // 完成关卡1
            Level2Complete,      // 完成关卡2
            GameComplete,        // 完成整个游戏
            Speedrun,            // 快速通关（10分钟内）
            Survivor,            // 生存者（血量低于1时击败Boss）
            SharpShooter,        // 神射手（连击达到20且不中断）
            Veteran              // 老兵（累计游戏时间1小时）
        }

        // 统计数据
        public enum StatId
        {
            TotalKills,
            TotalDeaths,
            TotalScore,
            TotalPlayTime,
            TotalBossKills,
            TotalCheckpoints,
            MaxCombo,
            LevelsCompleted,
            GamesPlayed
        }

        public static event Action<AchievementId> AchievementUnlocked;

        // 成就名称
        private static readonly Dictionary<AchievementId, string> achievementNames = new()
        {
            { AchievementId.FirstBlood, "First Blood" },
            { AchievementId.Kill10, "Slayer" },
            { AchievementId.Kill50, "Exterminator" },
            { AchievementId.Kill100, "One Man Army" },
            { AchievementId.FirstBoss, "Boss Slayer" },
            { AchievementId.NoDeathComplete, "Flawless Victory" },
            { AchievementId.PacifistRun, "Pacifist" },
            { AchievementId.Combo5, "Combo Starter" },
            { AchievementId.Combo10, "Combo Master" },
            { AchievementId.Combo20, "Combo Legend" },
            { AchievementId.AllCharactersUsed, "Squad Leader" },
            { AchievementId.WeaponMaster, "Weapon Master" },
            { AchievementId.Level1Complete, "Beach Victor" },
            { AchievementId.Level2Complete, "Factory Victor" },
            { AchievementId.GameComplete, "War Hero" },
            { AchievementId.Speedrun, "Speed Demon" },
            { AchievementId.Survivor, "Survivor" },
            { AchievementId.SharpShooter, "Sharp Shooter" },
            { AchievementId.Veteran, "Veteran" }
        };

        // 成就描述
        private static readonly Dictionary<AchievementId, string> achievementDescriptions = new()
        {
            { AchievementId.FirstBlood, "Defeat your first enemy" },
            { AchievementId.Kill10, "Defeat 10 enemies" },
            { AchievementId.Kill50, "Defeat 50 enemies" },
            { AchievementId.Kill100, "Defeat 100 enemies" },
            { AchievementId.FirstBoss, "Defeat your first boss" },
            { AchievementId.NoDeathComplete, "Complete a level without dying" },
            { AchievementId.PacifistRun, "Complete a level without killing anyone" },
            { AchievementId.Combo5, "Achieve a 5x combo" },
            { AchievementId.Combo10, "Achieve a 10x combo" },
            { AchievementId.Combo20, "Achieve a 20x combo" },
            { AchievementId.AllCharactersUsed, "Use all 4 characters in one level" },
            { AchievementId.WeaponMaster, "Max out all weapon upgrades" },
            { AchievementId.Level1Complete, "Complete Level 1: Beach" },
            { AchievementId.Level2Complete, "Complete Level 2: Factory" },
            { AchievementId.GameComplete, "Complete the entire game" },
            { AchievementId.Speedrun, "Complete the game in under 10 minutes" },
            { AchievementId.Survivor, "Defeat a boss with less than 1 HP" },
            { AchievementId.SharpShooter, "Reach 20x combo without breaking it" },
            { AchievementId.Veteran, "Play for 1 hour total" }
        };

        /// <summary>
        /// 检查成就是否已解锁。
        /// </summary>
        public static bool IsUnlocked(AchievementId id)
        {
            return PlayerPrefs.GetInt(KeyPrefix + id, 0) == 1;
        }

        /// <summary>
        /// 解锁成就。如果已解锁则返回false。
        /// 解锁后自动奖励军票，构建成就-经济闭环。
        /// </summary>
        public static bool Unlock(AchievementId id)
        {
            if (IsUnlocked(id)) return false;

            PlayerPrefs.SetInt(KeyPrefix + id, 1);
            PlayerPrefs.Save();

            // 成就解锁奖励军票
            var reward = GetAchievementReward(id);
            if (reward > 0)
                CurrencyManager.Add(reward);

            Debug.Log($"[Achievement] Unlocked: {GetAchievementName(id)} - {GetAchievementDescription(id)} (+{reward}军票)");
            AchievementUnlocked?.Invoke(id);
            return true;
        }

        /// <summary>
        /// 获取成就奖励军票数。稀有成就奖励更高。
        /// </summary>
        public static int GetAchievementReward(AchievementId id)
        {
            return id switch
            {
                AchievementId.FirstBlood => 50,
                AchievementId.Kill10 => 100,
                AchievementId.Kill50 => 300,
                AchievementId.Kill100 => 500,
                AchievementId.FirstBoss => 500,
                AchievementId.NoDeathComplete => 800,
                AchievementId.PacifistRun => 1000,
                AchievementId.Combo5 => 100,
                AchievementId.Combo10 => 300,
                AchievementId.Combo20 => 500,
                AchievementId.AllCharactersUsed => 200,
                AchievementId.WeaponMaster => 400,
                AchievementId.Level1Complete => 500,
                AchievementId.Level2Complete => 800,
                AchievementId.GameComplete => 2000,
                AchievementId.Speedrun => 1500,
                AchievementId.Survivor => 600,
                AchievementId.SharpShooter => 400,
                AchievementId.Veteran => 300,
                _ => 0
            };
        }

        /// <summary>
        /// 获取成就名称。
        /// </summary>
        public static string GetAchievementName(AchievementId id)
        {
            return achievementNames.TryGetValue(id, out var name) ? name : id.ToString();
        }

        /// <summary>
        /// 获取成就描述。
        /// </summary>
        public static string GetAchievementDescription(AchievementId id)
        {
            return achievementDescriptions.TryGetValue(id, out var desc) ? desc : "";
        }

        /// <summary>
        /// 获取所有成就ID。
        /// </summary>
        public static IEnumerable<AchievementId> GetAllAchievements()
        {
            return Enum.GetValues(typeof(AchievementId)) as AchievementId[];
        }

        /// <summary>
        /// 获取已解锁成就数量。
        /// </summary>
        public static int GetUnlockedCount()
        {
            int count = 0;
            foreach (AchievementId id in Enum.GetValues(typeof(AchievementId)))
            {
                if (IsUnlocked(id)) count++;
            }
            return count;
        }

        /// <summary>
        /// 获取成就总数。
        /// </summary>
        public static int GetTotalCount()
        {
            return Enum.GetValues(typeof(AchievementId)).Length;
        }

        // ===== 统计系统 =====

        /// <summary>
        /// 增加统计值。
        /// </summary>
        public static void AddStat(StatId stat, int amount = 1)
        {
            var current = GetStat(stat);
            SetStat(stat, current + amount);
        }

        /// <summary>
        /// 获取统计值。
        /// </summary>
        public static int GetStat(StatId stat)
        {
            return PlayerPrefs.GetInt(KeyStatsPrefix + stat, 0);
        }

        /// <summary>
        /// 设置统计值。
        /// </summary>
        public static void SetStat(StatId stat, int value)
        {
            PlayerPrefs.SetInt(KeyStatsPrefix + stat, value);
            PlayerPrefs.Save();
            CheckStatAchievements(stat, value);
        }

        /// <summary>
        /// 增加浮点统计值（用于游戏时间等）。
        /// 注意：此方法不调用PlayerPrefs.Save()，需要外部定期保存。
        /// </summary>
        public static void AddFloatStat(StatId stat, float amount)
        {
            var current = GetFloatStat(stat);
            SetFloatStatNoSave(stat, current + amount);
        }

        /// <summary>
        /// 获取浮点统计值。
        /// </summary>
        public static float GetFloatStat(StatId stat)
        {
            return PlayerPrefs.GetFloat(KeyStatsPrefix + stat, 0f);
        }

        /// <summary>
        /// 设置浮点统计值（不保存，用于频繁更新）。
        /// </summary>
        public static void SetFloatStatNoSave(StatId stat, float value)
        {
            PlayerPrefs.SetFloat(KeyStatsPrefix + stat, value);
        }

        /// <summary>
        /// 设置浮点统计值。
        /// </summary>
        public static void SetFloatStat(StatId stat, float value)
        {
            PlayerPrefs.SetFloat(KeyStatsPrefix + stat, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 保存所有未保存的统计到PlayerPrefs。
        /// </summary>
        public static void SaveAll()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 检查统计相关成就。
        /// </summary>
        private static void CheckStatAchievements(StatId stat, int value)
        {
            switch (stat)
            {
                case StatId.TotalKills:
                    if (value >= 1) Unlock(AchievementId.FirstBlood);
                    if (value >= 10) Unlock(AchievementId.Kill10);
                    if (value >= 50) Unlock(AchievementId.Kill50);
                    if (value >= 100) Unlock(AchievementId.Kill100);
                    break;

                case StatId.TotalBossKills:
                    if (value >= 1) Unlock(AchievementId.FirstBoss);
                    break;

                case StatId.MaxCombo:
                    if (value >= 5) Unlock(AchievementId.Combo5);
                    if (value >= 10) Unlock(AchievementId.Combo10);
                    if (value >= 20) Unlock(AchievementId.Combo20);
                    break;

                case StatId.LevelsCompleted:
                    if (value >= 1) Unlock(AchievementId.Level1Complete);
                    if (value >= 2) Unlock(AchievementId.Level2Complete);
                    if (value >= 2) Unlock(AchievementId.GameComplete);
                    break;

                case StatId.TotalPlayTime:
                    if (value >= 3600) Unlock(AchievementId.Veteran); // 1小时
                    break;
            }
        }

        /// <summary>
        /// 重置所有成就和统计（用于测试）。
        /// </summary>
        public static void ResetAll()
        {
            foreach (AchievementId id in Enum.GetValues(typeof(AchievementId)))
                PlayerPrefs.DeleteKey(KeyPrefix + id);
            foreach (StatId stat in Enum.GetValues(typeof(StatId)))
                PlayerPrefs.DeleteKey(KeyStatsPrefix + stat);
            PlayerPrefs.Save();
        }
    }
}
