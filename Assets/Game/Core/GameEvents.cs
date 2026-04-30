using System;

namespace SteelRain.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action<string, int> AmmoChanged;
        public static event Action<string> WeaponFormChanged;
        public static event Action<int> WeaponLevelChanged;
        public static event Action<string> PlayerCharacterChanged;
        public static event Action<string> SkillStatusChanged;
        public static event Action CheckpointReached;
        public static event Action PlayerDied;
        public static event Action SquadDefeated;
        public static event Action LevelCompleted;

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

        public static void RaiseSkillStatusChanged(string status) =>
            SkillStatusChanged?.Invoke(status);

        public static void RaiseCheckpointReached() =>
            CheckpointReached?.Invoke();

        public static void RaisePlayerDied() =>
            PlayerDied?.Invoke();

        public static void RaiseSquadDefeated() =>
            SquadDefeated?.Invoke();

        public static void RaiseLevelCompleted() =>
            LevelCompleted?.Invoke();
    }
}
