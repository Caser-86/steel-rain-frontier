using System.Collections.Generic;
using UnityEngine;

namespace SteelRain.Player
{
    public sealed class CharacterRuntime
    {
        private readonly Dictionary<string, int> weaponLevels = new();

        public CharacterDefinition Definition { get; }
        public int CurrentHealth { get; set; }
        public string SelectedWeaponId { get; set; } = "assault_rifle";
        public float SkillCooldownRemaining { get; set; }

        public CharacterRuntime(CharacterDefinition definition)
        {
            Definition = definition;
            CurrentHealth = definition.maxHealth;
        }

        public int GetWeaponLevel(string weaponId)
        {
            return weaponLevels.TryGetValue(weaponId, out var level) ? level : 0;
        }

        public void SetWeaponLevel(string weaponId, int level)
        {
            weaponLevels[weaponId] = Mathf.Clamp(level, 0, 3);
        }

        public void ClearCurrentWeaponUpgradeOnDeath()
        {
            SetWeaponLevel(SelectedWeaponId, 0);
        }
    }
}
