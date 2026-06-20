using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Player
{
    /// <summary>
    /// Easy 难度自动回血：每隔指定间隔恢复 1 点 HP。
    /// 仅在非满血、非死亡、非暂停时生效，为新手提供容错。
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class AutoHeal : MonoBehaviour
    {
        private Health health;
        private float timer;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            // 暂停时不计时
            if (Time.timeScale == 0f) return;

            var interval = DifficultyManager.GetAutoHealInterval();
            if (interval <= 0f) return; // 非Easy难度不自动回血
            if (health == null || health.IsDead) return;
            if (health.Current >= health.Max) return; // 满血不回

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                health.Heal(1);
            }
        }
    }
}
