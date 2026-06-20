using System.Collections.Generic;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 商店系统：关卡间使用军票购买永久升级和消耗品。
    /// 构建"消耗-追求"闭环，为玩家提供长期追求目标。
    /// </summary>
    public static class ShopManager
    {
        private const string KeyReviveCount = "Save_ReviveCount";
        private const string KeyWeaponBoost = "Save_WeaponBoost_";

        /// <summary>
        /// 商店可购买项定义。
        /// </summary>
        public readonly struct ShopItem
        {
            public readonly string id;
            public readonly string displayName;
            public readonly int price;
            public readonly ShopItemType type;

            public ShopItem(string id, string displayName, int price, ShopItemType type)
            {
                this.id = id;
                this.displayName = displayName;
                this.price = price;
                this.type = type;
            }
        }

        public enum ShopItemType
        {
            Revive,         // 复活次数（消耗品）
            WeaponBoost,    // 永久武器伤害加成
            MaxHealthUp,    // 永久最大血量提升
        }

        // 商店商品目录
        private static readonly List<ShopItem> catalog = new()
        {
            new ShopItem("revive_1", "复活信标 ×1", 300, ShopItemType.Revive),
            new ShopItem("revive_3", "复活信标 ×3", 800, ShopItemType.Revive),
            new ShopItem("weapon_boost", "永久武器强化 +10%", 500, ShopItemType.WeaponBoost),
            new ShopItem("max_hp", "最大生命 +1", 600, ShopItemType.MaxHealthUp),
        };

        public static IReadOnlyList<ShopItem> Catalog => catalog;

        /// <summary>
        /// 当前最大血量提升次数（每次+1）。
        /// </summary>
        public static int MaxHealthUpCount
        {
            get => PlayerPrefs.GetInt(KeyReviveCount + "_maxhp", 0);
            private set => PlayerPrefs.SetInt(KeyReviveCount + "_maxhp", Mathf.Max(0, value));
        }

        /// <summary>
        /// 当前持有的复活次数。
        /// </summary>
        public static int ReviveCount
        {
            get => PlayerPrefs.GetInt(KeyReviveCount, 0);
            private set => PlayerPrefs.SetInt(KeyReviveCount, Mathf.Max(0, value));
        }

        /// <summary>
        /// 武器伤害永久加成倍率（每次购买+0.1，上限0.5）。
        /// </summary>
        public static float WeaponBoostMultiplier
        {
            get => PlayerPrefs.GetFloat(KeyWeaponBoost + "all", 0f);
            private set => PlayerPrefs.SetFloat(KeyWeaponBoost + "all", Mathf.Min(0.5f, value));
        }

        /// <summary>
        /// 购买商品。返回是否成功。
        /// </summary>
        public static bool Purchase(string itemId)
        {
            var item = catalog.Find(x => x.id == itemId);
            if (item.id == null) return false;

            // 上限检查
            if (item.type == ShopItemType.WeaponBoost && WeaponBoostMultiplier >= 0.5f)
            {
                Debug.LogWarning("[Shop] 武器强化已达上限 (50%)");
                return false;
            }
            if (item.type == ShopItemType.MaxHealthUp && MaxHealthUpCount >= 5)
            {
                Debug.LogWarning("[Shop] 最大生命已提升5次，达上限");
                return false;
            }

            if (!CurrencyManager.Spend(item.price)) return false;

            switch (item.type)
            {
                case ShopItemType.Revive:
                    ReviveCount += itemId == "revive_3" ? 3 : 1;
                    break;
                case ShopItemType.WeaponBoost:
                    WeaponBoostMultiplier += 0.1f;
                    break;
                case ShopItemType.MaxHealthUp:
                    MaxHealthUpCount++;
                    // 持久化最大血量提升到 SaveSystem，确保 PlayerSquad 启动时读取
                    SaveSystem.SaveMaxHealthBonus(MaxHealthUpCount);
                    GameEvents.RaiseMaxHealthUpgraded();
                    break;
            }
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>
        /// 消耗一次复活信标。返回是否成功。
        /// </summary>
        public static bool ConsumeRevive()
        {
            if (ReviveCount <= 0) return false;
            ReviveCount--;
            PlayerPrefs.Save();
            return true;
        }

        public static void Reset()
        {
            ReviveCount = 0;
            WeaponBoostMultiplier = 0f;
            MaxHealthUpCount = 0;
            PlayerPrefs.Save();
        }
    }
}
