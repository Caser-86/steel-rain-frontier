using UnityEngine;

namespace SteelRain.Weapons
{
    [CreateAssetMenu(menuName = "Steel Rain/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "assault_rifle";
        public string displayName = "Assault Rifle";
        public int startingAmmo = 90;

        [Header("Forms")]
        public WeaponFormDefinition[] forms;
        public Projectile projectilePrefab;

        [Header("Ammo")]
        public bool baseAmmoInfinite = true;
    }
}
