using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 无人机：飞行单位，俯冲攻击玩家。
    /// 不受重力影响，在空中悬浮追踪。
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DroneAI : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float hoverHeight = 4f;
        [SerializeField] private float diveCooldown = 3f;

        private Health health;
        private Rigidbody2D body;
        private float nextDiveTime;
        private float nextFireTime;
        private bool diving;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            if (firePoint == null) firePoint = transform;
            // Awake中使用基础生命值，Initialize时会应用难度倍率重新初始化
            var baseHealth = definition != null ? definition.maxHealth : 2;
            health.Initialize(baseHealth, Team.Enemy);
            health.Died += () =>
            {
                if (definition != null)
                {
                    ScoreManager.AddKill(definition.scoreValue);
                    UI.AchievementTracker.OnEnemyKilled(definition.scoreValue);
                }
                ExplosionEffect.Spawn(transform.position, 0.4f);
                AudioManager.Play("sfx_explosion", 0.3f);
                Destroy(gameObject);
            };
        }

        private void FixedUpdate()
        {
            if (target == null || definition == null) return;

            var dx = target.position.x - transform.position.x;
            var dy = target.position.y + hoverHeight - transform.position.y;
            var dist = Mathf.Abs(dx);
            var slowMultiplier = CharacterSkill.TimeRiftActive ? 0.4f : 1f;

            if (diving)
            {
                // 俯冲
                var dir = (target.position - transform.position).normalized;
                var speedMultiplier = DifficultyManager.GetEnemySpeedMultiplier() * slowMultiplier;
                body.linearVelocity = dir * definition.moveSpeed * 1.5f * speedMultiplier;

                if (dist < 1.5f)
                {
                    diving = false;
                    nextDiveTime = Time.time + diveCooldown;
                }
            }
            else
            {
                // 悬浮追踪
                var xDir = dx == 0f ? 0f : Mathf.Sign(dx);
                var speedMultiplier = DifficultyManager.GetEnemySpeedMultiplier() * slowMultiplier;
                var targetVel = new Vector2(
                    xDir * definition.moveSpeed * 0.6f * speedMultiplier,
                    dy * 2f * slowMultiplier
                );
                body.linearVelocity = targetVel;

                if (Time.time >= nextFireTime && dist < definition.attackRange)
                {
                    nextFireTime = Time.time + definition.attackCooldown;
                    FireAtTarget();
                }

                if (Time.time >= nextDiveTime && dist < definition.detectRange)
                {
                    diving = true;
                }
            }
        }

        private void FireAtTarget()
        {
            if (projectilePrefab == null || target == null) return;
            var dir = (target.position - firePoint.position).normalized;
            var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            var speed = definition.projectileSpeed * (CharacterSkill.TimeRiftActive ? 0.4f : 1f);
            var dmg = Mathf.RoundToInt(definition.rangedDamage * DifficultyManager.GetDamageMultiplier());
            proj.LaunchWithDamage(dir, speed, dmg, 0, Team.Enemy);
            AudioManager.Play("sfx_enemy_shoot", 0.4f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (diving && collision.collider.TryGetComponent(out Health other) && other.Team == Team.Player)
            {
                // 根据碰撞方向计算击退方向
                var dir = (other.transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = Vector2.up;
                other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(definition.contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, dir));
                diving = false;
                nextDiveTime = Time.time + diveCooldown;
                // 重置俯冲速度，避免无人机继续以俯冲速度移动
                body.linearVelocity = Vector2.zero;
            }
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
