using System.Collections.Generic;
using SteelRain.Audio;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 武器升级胶囊拾取物，永久存在直到拾取。
    /// 死亡复活时通过 ResetAllOnRespawn 重新激活，避免死亡螺旋。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WeaponUpgradePickup : MonoBehaviour
    {
        // 当前场景内所有已注册的升级胶囊，用于复活时统一重置
        private static readonly List<WeaponUpgradePickup> registered = new();

        private bool consumed;

        private void OnEnable()
        {
            if (!registered.Contains(this))
                registered.Add(this);
            // 重新激活时清除已消耗标记
            consumed = false;
        }

        private void OnDisable()
        {
            registered.Remove(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed) return;
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;

            consumed = true;
            combat.UpgradeCurrentWeapon();
            AudioManager.Play("sfx_upgrade");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 检查点复活时调用：重新激活本关卡内所有已消耗的升级胶囊，
        /// 防止"死亡→武器变弱→更难通关→再死亡"的恶性循环。
        /// </summary>
        public static void ResetAllOnRespawn()
        {
            for (int i = registered.Count - 1; i >= 0; i--)
            {
                var pickup = registered[i];
                if (pickup == null)
                {
                    registered.RemoveAt(i);
                    continue;
                }
                pickup.consumed = false;
                if (!pickup.gameObject.activeSelf)
                    pickup.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 关卡卸载时清理静态列表引用，防止跨场景残留。
        /// </summary>
        public static void ClearRegistry()
        {
            registered.Clear();
        }
    }
}
