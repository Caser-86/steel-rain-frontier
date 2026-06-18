using SteelRain.Core;
using UnityEngine;

namespace SteelRain.UI
{
    /// <summary>
    /// 成就跟踪器：监听游戏事件，自动触发成就解锁和统计更新。
    /// 应挂载到游戏场景中的持久对象上。
    /// </summary>
    public sealed class AchievementTracker : MonoBehaviour
    {
        private int charactersUsedMask;
        private bool levelStartedWithDeath;
        private float levelStartTime;
        private float gameStartTime;
        private bool hasKilledThisLevel;

        private void OnEnable()
        {
            GameEvents.PlayerDied += OnPlayerDied;
            GameEvents.BossDefeated += OnBossDefeated;
            GameEvents.CheckpointReached += OnCheckpointReached;
            GameEvents.PlayerCharacterChanged += OnCharacterChanged;
            ScoreManager.ComboChanged += OnComboChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDied -= OnPlayerDied;
            GameEvents.BossDefeated -= OnBossDefeated;
            GameEvents.CheckpointReached -= OnCheckpointReached;
            GameEvents.PlayerCharacterChanged -= OnCharacterChanged;
            ScoreManager.ComboChanged -= OnComboChanged;
        }

        private void Start()
        {
            levelStartTime = Time.time;
            if (LevelManager.CurrentLevel == 0)
            {
                gameStartTime = Time.time;
                AchievementManager.AddStat(AchievementManager.StatId.GamesPlayed);
            }
            hasKilledThisLevel = false;
            charactersUsedMask = 0;
        }

        private void Update()
        {
            // 更新游戏时间统计
            AchievementManager.AddFloatStat(AchievementManager.StatId.TotalPlayTime, Time.deltaTime);

            // 检查老兵成就（1小时）
            var playTime = AchievementManager.GetFloatStat(AchievementManager.StatId.TotalPlayTime);
            if (playTime >= 3600f)
            {
                AchievementManager.Unlock(AchievementManager.AchievementId.Veteran);
            }
        }

        /// <summary>
        /// 当玩家击杀敌人时调用（由EnemyController触发）。
        /// </summary>
        public static void OnEnemyKilled(int scoreValue)
        {
            AchievementManager.AddStat(AchievementManager.StatId.TotalKills);
            AchievementManager.AddStat(AchievementManager.StatId.TotalScore, scoreValue);

            // 标记当前关卡有击杀
            var tracker = FindObjectOfType<AchievementTracker>();
            if (tracker != null) tracker.hasKilledThisLevel = true;
        }

        private void OnPlayerDied()
        {
            AchievementManager.AddStat(AchievementManager.StatId.TotalDeaths);
            levelStartedWithDeath = true;
        }

        private void OnBossDefeated()
        {
            AchievementManager.AddStat(AchievementManager.StatId.TotalBossKills);

            // 检查生存者成就（血量低于1时击败Boss）
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.TryGetComponent(out Health health))
            {
                if (health.Current <= 1)
                {
                    AchievementManager.Unlock(AchievementManager.AchievementId.Survivor);
                }
            }
        }

        private void OnCheckpointReached()
        {
            AchievementManager.AddStat(AchievementManager.StatId.TotalCheckpoints);
        }

        private void OnCharacterChanged(string displayName)
        {
            // 跟踪使用的角色（使用位掩码）
            int charIndex = displayName switch
            {
                "Aila" => 0,
                "Bruno" => 1,
                "Mara" => 2,
                "Niko" => 3,
                _ => -1
            };

            if (charIndex >= 0)
            {
                charactersUsedMask |= (1 << charIndex);
                // 检查是否使用了所有4个角色
                if (charactersUsedMask == 0b1111)
                {
                    AchievementManager.Unlock(AchievementManager.AchievementId.AllCharactersUsed);
                }
            }
        }

        private void OnComboChanged(int combo)
        {
            if (combo > AchievementManager.GetStat(AchievementManager.StatId.MaxCombo))
            {
                AchievementManager.SetStat(AchievementManager.StatId.MaxCombo, combo);
            }
        }

        /// <summary>
        /// 当关卡完成时调用。
        /// </summary>
        public void OnLevelComplete()
        {
            AchievementManager.AddStat(AchievementManager.StatId.LevelsCompleted);

            // 检查无死亡成就
            if (!levelStartedWithDeath)
            {
                AchievementManager.Unlock(AchievementManager.AchievementId.NoDeathComplete);
            }

            // 检查和平主义者成就
            if (!hasKilledThisLevel)
            {
                AchievementManager.Unlock(AchievementManager.AchievementId.PacifistRun);
            }

            // 检查快速通关成就
            var levelDuration = Time.time - gameStartTime;
            if (LevelManager.CurrentLevel >= LevelManager.TotalLevels - 1 && levelDuration < 600f)
            {
                AchievementManager.Unlock(AchievementManager.AchievementId.Speedrun);
            }
        }

        private void OnDestroy()
        {
            // 保存当前分数到统计
            AchievementManager.AddStat(AchievementManager.StatId.TotalScore, ScoreManager.Score);
        }
    }
}
