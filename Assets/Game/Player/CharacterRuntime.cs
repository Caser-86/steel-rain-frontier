using UnityEngine;

namespace SteelRain.Player
{
    public sealed class CharacterRuntime
    {
        public CharacterDefinition Definition { get; }
        public int CurrentHealth { get; set; }

        public CharacterRuntime(CharacterDefinition definition)
        {
            Definition = definition;
            CurrentHealth = definition.maxHealth;
        }
    }
}
