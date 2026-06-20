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
        MortarMarker,
        RapidCharge,      // 快速冲锋近战
        SniperShot,       // 远程狙击
        HeavyMachineGun   // 重型机枪
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

        [Header("Advanced")]
        [Tooltip("子弹散射角度，0为单发")]
        public float projectileSpread = 0f;
        [Tooltip("一次发射的子弹数")]
        public int projectileCount = 1;
        [Tooltip("敌人子弹颜色")]
        public Color projectileColor = new Color(1f, 0.5f, 0.3f, 1f);
        [Tooltip("敌人子弹大小")]
        public float projectileScale = 1f;
    }
}
