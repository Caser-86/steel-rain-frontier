using UnityEngine;

namespace SteelRain.Weapons
{
    [CreateAssetMenu(menuName = "Steel Rain/Weapon Form")]
    public sealed class WeaponFormDefinition : ScriptableObject
    {
        public string id = "auto";
        public string displayName = "Auto";
        public ProjectilePattern pattern = ProjectilePattern.Single;
        public int damage = 1;
        public int ammoCost = 1;
        public float fireRate = 9f;
        public float projectileSpeed = 18f;
        public int projectileCount = 1;
        public float spreadAngle = 0f;
        public int pierceCount = 0;
        public float explosionRadius = 0f;
        public float levelOneDamageMultiplier = 1.3f;
        public float levelTwoDamageMultiplier = 1.7f;
        public float levelThreeDamageMultiplier = 2f;
        public float levelOneFireRateMultiplier = 1.1f;
        public float levelTwoFireRateMultiplier = 1.25f;
        public float levelThreeFireRateMultiplier = 1.4f;
    }
}
