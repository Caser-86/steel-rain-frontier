using System;

namespace SteelRain.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action<string, int> AmmoChanged;
        public static event Action<string> WeaponFormChanged;
        public static event Action CheckpointReached;
        public static event Action PlayerDied;

        public static void RaisePlayerHealthChanged(int current, int max) =>
            PlayerHealthChanged?.Invoke(current, max);

        public static void RaiseAmmoChanged(string weaponName, int ammo) =>
            AmmoChanged?.Invoke(weaponName, ammo);

        public static void RaiseWeaponFormChanged(string formName) =>
            WeaponFormChanged?.Invoke(formName);

        public static void RaiseCheckpointReached() =>
            CheckpointReached?.Invoke();

        public static void RaisePlayerDied() =>
            PlayerDied?.Invoke();
    }
}
