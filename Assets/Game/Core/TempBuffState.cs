using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 全局临时状态（合金弹头简化版）。
    /// 供拾取物和敌人AI共享。
    /// 包括：护盾、速度增益。
    /// </summary>
    public static class TempBuffState
    {
        // 护盾（ShieldPickup 激活，Health 检查）
        public static bool ShieldActive { get; set; }
        public static float ShieldTimer { get; set; }

        // 速度增益（SpeedBoostPickup 激活）
        public static bool SpeedBoostActive { get; set; }
        public static float SpeedBoostTimer { get; set; }
        public static float SpeedBoostMultiplier { get; set; } = 1f;

        /// <summary>
        /// 每帧由 PlayerController 调用，递减计时器。
        /// </summary>
        public static void Tick()
        {
            if (ShieldActive && ShieldTimer > 0f)
            {
                ShieldTimer -= Time.deltaTime;
                if (ShieldTimer <= 0f)
                {
                    ShieldTimer = 0f;
                    ShieldActive = false;
                }
            }

            if (SpeedBoostActive && SpeedBoostTimer > 0f)
            {
                SpeedBoostTimer -= Time.deltaTime;
                if (SpeedBoostTimer <= 0f)
                {
                    SpeedBoostTimer = 0f;
                    SpeedBoostActive = false;
                    SpeedBoostMultiplier = 1f;
                }
            }
        }

        public static void Reset()
        {
            ShieldActive = false;
            ShieldTimer = 0f;
            SpeedBoostActive = false;
            SpeedBoostTimer = 0f;
            SpeedBoostMultiplier = 1f;
        }
    }
}
