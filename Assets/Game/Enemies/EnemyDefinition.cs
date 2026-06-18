using UnityEngine;

namespace SteelRain.Enemies
{
    public enum EnemyAttackPattern
    {
        RifleBurst,
        GrenadeArc,
        ShieldAdvance,
        DroneDive,
        FlamethrowerCone,
        MortarMarker
    }

    [CreateAssetMenu(menuName = "Steel Rain/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id = "rifle_soldier";
        public string displayName = "Rifle Soldier";

        [Header("Stats")]
        public int maxHealth = 3;
        public float moveSpeed = 2.5f;
        public float detectRange = 9f;
        public float attackRange = 6f;
        public float attackCooldown = 1.4f;
        public EnemyAttackPattern attackPattern = EnemyAttackPattern.RifleBurst;

        [Header("Combat")]
        public int contactDamage = 1;
        public int rangedDamage = 1;
        public float projectileSpeed = 8f;
        public int scoreValue = 100;
        public Color spriteColor = new Color(0.8f, 0.4f, 0.3f);
    }
}
