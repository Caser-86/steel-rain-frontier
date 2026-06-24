using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 手雷兵：抛物线投弹，压迫掩体后的玩家。
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class GrenadierAI : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float arcHeight = 4f;
        [SerializeField] private float grenadeSpeed = 8f;

        private Health health;
        private Rigidbody2D body;
        private float nextAttackTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            if (firePoint == null) firePoint = transform;
            health.Initialize(definition != null ? definition.maxHealth : 3, Team.Enemy);
            health.Died += () =>
            {
                if (definition != null)
                {
                    ScoreManager.AddKill(definition.scoreValue);
                    UI.AchievementTracker.OnEnemyKilled(definition.scoreValue);
                }
                ExplosionEffect.Spawn(transform.position, 0.5f);
                AudioManager.Play("sfx_explosion", 0.4f);
                Destroy(gameObject);
            };
        }

        private void FixedUpdate()
        {
            if (target == null || definition == null) return;

            var dx = target.position.x - transform.position.x;
            var dist = Mathf.Abs(dx);

            if (dist > definition.attackRange)
            {
                var dir = Mathf.Sign(dx);
                // 应用难度速度倍率
                var speed = definition.moveSpeed * DifficultyManager.GetEnemySpeedMultiplier();
                body.linearVelocity = new Vector2(dir * speed, body.linearVelocity.y);
            }
            else
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);

                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + definition.attackCooldown;
                    ThrowGrenade();
                }
            }
        }

        private void ThrowGrenade()
        {
            if (projectilePrefab == null || target == null) return;

            var startPos = firePoint.position;
            var targetPos = target.position;
            var dir = (targetPos - startPos).normalized;

            // 抛物线：给一个向上的初速度
            var throwDir = new Vector2(dir.x, dir.y + arcHeight / 3f).normalized;
            var proj = Instantiate(projectilePrefab, startPos, Quaternion.identity);
            var dmg = Mathf.RoundToInt(definition.rangedDamage * DifficultyManager.GetDamageMultiplier());
            proj.LaunchWithDamage(throwDir, grenadeSpeed, dmg, 0, Team.Enemy);
            AudioManager.Play("sfx_enemy_shoot", 0.5f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null) return;
            if (!collision.collider.TryGetComponent(out Health other)) return;
            if (other.Team == Team.Enemy) return;
            // 根据碰撞方向计算击退方向
            var dir = (other.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Vector2.right;
            other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(definition.contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, dir));
        }

        public void Initialize(EnemyDefinition def, Transform player, Projectile projectile)
        {
            definition = def;
            target = player;
            projectilePrefab = projectile;
            if (health == null) health = GetComponent<Health>();
            // 应用难度生命倍率
            var scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(def.maxHealth * DifficultyManager.GetHealthMultiplier()));
            health.Initialize(scaledMaxHealth, Team.Enemy);
        }

        public void AssignTarget(Transform t) => target = t;
    }
}
