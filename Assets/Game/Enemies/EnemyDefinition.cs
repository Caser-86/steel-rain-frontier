using UnityEngine;

namespace SteelRain.Enemies
{
    [CreateAssetMenu(menuName = "Steel Rain/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string id = "rifle_soldier";
        public string displayName = "Rifle Soldier";
        public int maxHealth = 3;
        public float moveSpeed = 2.5f;
        public float detectRange = 9f;
        public float attackRange = 6f;
        public float attackCooldown = 1.4f;
        public EnemyAttackPattern attackPattern = EnemyAttackPattern.RifleBurst;
        public EnemyProjectile projectilePrefab;
        public int projectileDamage = 1;
        public float projectileSpeed = 9f;
    }
}
