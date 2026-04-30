using UnityEngine;

namespace SteelRain.Weapons
{
    [CreateAssetMenu(menuName = "Steel Rain/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string id = "assault_rifle";
        public string displayName = "Assault Rifle";
        public int startingAmmo = 90;
        public bool baseAmmoInfinite = true;
        public string levelOneLabel = "Firepower Boost";
        public string levelTwoLabel = "Evolved Form";
        public string levelThreeLabel = "Tactical Awakening";
        public WeaponFormDefinition[] forms;
        public Projectile projectilePrefab;
    }
}
