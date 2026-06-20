using System;
using UnityEngine;

namespace SteelRain.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action<string, int> AmmoChanged;
        public static event Action<string> WeaponFormChanged;
        public static event Action<int> WeaponLevelChanged;
        public static event Action<string> PlayerCharacterChanged;
        public static event Action<Vector2> PlayerDamaged;
        public static event Action CheckpointReached;
        public static event Action PlayerDied;
        public static event Action BossDefeated;
        public static event Action<int> CurrencyChanged;
        public static event Action MaxHealthUpgraded;

        public static void RaisePlayerHealthChanged(int current, int max) =>
            PlayerHealthChanged?.Invoke(current, max);

        public static void RaiseAmmoChanged(string weaponName, int ammo) =>
            AmmoChanged?.Invoke(weaponName, ammo);

        public static void RaiseWeaponFormChanged(string formName) =>
            WeaponFormChanged?.Invoke(formName);

        public static void RaiseWeaponLevelChanged(int level) =>
            WeaponLevelChanged?.Invoke(level);

        public static void RaisePlayerCharacterChanged(string displayName) =>
            PlayerCharacterChanged?.Invoke(displayName);

        public static void RaisePlayerDamaged(Vector2 sourceDirection) =>
            PlayerDamaged?.Invoke(sourceDirection);

        public static void RaiseCheckpointReached() =>
            CheckpointReached?.Invoke();

        public static void RaisePlayerDied() =>
            PlayerDied?.Invoke();

        public static void RaiseBossDefeated() =>
            BossDefeated?.Invoke();

        public static void RaiseCurrencyChanged(int balance) =>
            CurrencyChanged?.Invoke(balance);

        public static void RaiseMaxHealthUpgraded() =>
            MaxHealthUpgraded?.Invoke();
    }
}
