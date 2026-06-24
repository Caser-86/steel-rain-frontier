using System.Collections.Generic;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 武器拾取物（合金弹头简化版）。
    /// 玩家触碰后切换当前武器，死亡后重置为手枪。
    /// 无武器升级、无永久解锁。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private int ammo = -1; // -1 表示使用武器的 startingAmmo

        // 已拾取的武器实例注册表，用于检查点复活时重置
        private static readonly List<WeaponPickup> registry = new();
        // 标记是否被玩家拾取（区别于场景切换导致的 OnDisable）
        private bool pickedUp;

        private void OnEnable()
        {
            pickedUp = false;
            if (!registry.Contains(this))
                registry.Add(this);
        }

        private void OnDisable()
        {
            // 只有被玩家拾取才从注册表移除，场景切换时不移除
            if (pickedUp)
                registry.Remove(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;
            if (weapon == null) return;

            combat.SwapWeapon(weapon, ammo);
            AudioManager.Play("sfx_upgrade", 0.7f);
            pickedUp = true;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 关卡切换时清空注册表。
        /// </summary>
        public static void ClearRegistry()
        {
            registry.Clear();
        }

        /// <summary>
        /// 检查点复活时重新激活所有已拾取的武器拾取物。
        /// </summary>
        public static void ResetAllOnRespawn()
        {
            foreach (var pickup in registry)
            {
                if (pickup != null)
                {
                    pickup.pickedUp = false;
                    pickup.gameObject.SetActive(true);
                }
            }
        }
    }
}
