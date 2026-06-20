using System;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 货币（军票）管理系统：击杀掉落、商店消费、跨关卡持久化。
    /// 构建"体验-反馈-消耗-追求"闭环中的"消耗-追求"环节。
    /// </summary>
    public static class CurrencyManager
    {
        private const string KeyCurrency = "Save_Currency";

        public static event Action<int> CurrencyChanged;

        public static int Balance
        {
            get => PlayerPrefs.GetInt(KeyCurrency, 0);
            private set
            {
                var clamped = Mathf.Max(0, value);
                PlayerPrefs.SetInt(KeyCurrency, clamped);
                PlayerPrefs.Save();
                CurrencyChanged?.Invoke(clamped);
                GameEvents.RaiseCurrencyChanged(clamped);
            }
        }

        /// <summary>
        /// 增加军票（击杀敌人、成就奖励等）。
        /// </summary>
        public static void Add(int amount)
        {
            if (amount <= 0) return;
            Balance = Balance + amount;
        }

        /// <summary>
        /// 消费军票（商店购买）。返回是否购买成功。
        /// </summary>
        public static bool Spend(int amount)
        {
            if (amount <= 0 || Balance < amount) return false;
            Balance = Balance - amount;
            return true;
        }

        /// <summary>
        /// 根据敌人分数价值计算掉落军票数。
        /// 普通敌人=10，精英=50，Boss=500（向上取整到10的倍数）。
        /// </summary>
        public static int CalculateDrop(int scoreValue)
        {
            if (scoreValue <= 0) return 0;
            // Boss（分数>=500）掉落500军票
            if (scoreValue >= 500) return 500;
            // 精英（分数>=100）掉落50军票
            if (scoreValue >= 100) return 50;
            // 普通敌人掉落10军票
            return 10;
        }

        public static void Reset()
        {
            Balance = 0;
        }
    }
}
