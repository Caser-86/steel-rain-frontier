using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private Projectile enemyProjectilePrefab;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;

        private Health health;
        private Rigidbody2D body;
        private float nextAttackTime;
        private Vector2 originalPosition;

        public EnemyDefinition Definition => definition;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (firePoint == null)
                firePoint = transform;
            originalPosition = transform.position;
            health.Died += OnDeath;
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= OnDeath;
        }

        private void OnDeath()
        {
            if (definition != null)
            {
                ScoreManager.AddKill(definition.scoreValue);
                // 触发成就系统
                UI.AchievementTracker.OnEnemyKilled(definition.scoreValue);
            }
            ExplosionEffect.Spawn(transform.position, 0.5f);
            AudioManager.Play("sfx_explosion", 0.4f);
            Destroy(gameObject);
        }

        public void Initialize(EnemyDefinition def, Transform playerTarget)
        {
            if (health == null) health = GetComponent<Health>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (firePoint == null) firePoint = transform;

            definition = def;
            target = playerTarget;
            // 应用难度生命倍率
            var scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(def.maxHealth * DifficultyManager.GetHealthMultiplier()));
            health.Initialize(scaledMaxHealth, Team.Enemy);
            if (spriteRenderer != null)
                spriteRenderer.color = def.spriteColor;
        }

        private void FixedUpdate()
        {
            if (target == null || definition == null)
                return;

            var delta = target.position - transform.position;
            var distance = Mathf.Abs(delta.x);
            var slowMultiplier = CharacterSkill.TimeRiftActive ? 0.4f : 1f;

            if (distance > definition.detectRange)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if (distance > definition.attackRange)
            {
                var direction = Mathf.Sign(delta.x);
                // 应用难度速度倍率
                var speed = definition.moveSpeed * slowMultiplier * DifficultyManager.GetEnemySpeedMultiplier();
                body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);
                if (spriteRenderer != null)
                    spriteRenderer.flipX = direction < 0;
                return;
            }

            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            if (spriteRenderer != null)
                spriteRenderer.flipX = delta.x < 0;

            TryAttack();
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
                return;

            // 难度影响攻击冷却：Hard难度敌人攻击更频繁，Easy更迟钝
            nextAttackTime = Time.time + definition.attackCooldown * DifficultyManager.GetEnemyAttackCooldownMultiplier();

            switch (definition.attackPattern)
            {
                case EnemyAttackPattern.RifleBurst:
                case EnemyAttackPattern.DroneDive:
                    FireAtTarget();
                    break;
                case EnemyAttackPattern.HeavyMachineGun:
                    // 重机枪：3 连发，每发间隔 0.1 秒
                    StartCoroutine(BurstFire(3, 0.1f));
                    break;
                case EnemyAttackPattern.SniperShot:
                    // 狙击：单发高速大子弹，预警闪光
                    SkillVFX.SpawnWarningZone(firePoint.position, 0.5f, 0.4f);
                    StartCoroutine(SniperFire());
                    break;
                case EnemyAttackPattern.RapidCharge:
                    // 快速冲锋：不射击，直接冲撞（移动逻辑已处理）
                    break;
            }
        }

        private System.Collections.IEnumerator BurstFire(int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                FireAtTarget();
                yield return new WaitForSeconds(interval);
            }
        }

        private System.Collections.IEnumerator SniperFire()
        {
            yield return new WaitForSeconds(0.4f);
            if (enemyProjectilePrefab == null || target == null) yield break;

            var toTarget = target.position - firePoint.position;
            var dir = new Vector2(Mathf.Sign(toTarget.x), 0f);
            var projectile = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);

            // 狙击子弹：红色大子弹，高速
            var sr = projectile.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1f, 0.2f, 0.2f, 1f);
                projectile.transform.localScale = Vector3.one * 2f;
            }

            var speed = definition.projectileSpeed * 1.8f * (CharacterSkill.TimeRiftActive ? 0.4f : 1f);
            var dmg = Mathf.RoundToInt(definition.rangedDamage * 2 * DifficultyManager.GetDamageMultiplier());
            projectile.LaunchWithDamage(dir, speed, dmg, 0, Team.Enemy);
            AudioManager.Play("sfx_enemy_shoot", 0.7f);
        }

        private void FireAtTarget()
        {
            if (enemyProjectilePrefab == null || target == null) return;

            // 子弹水平飞行（不追踪玩家 Y 轴），让蹲下/趴下/跳跃的玩家能躲过子弹。
            var toTarget = target.position - firePoint.position;
            var baseDir = new Vector2(Mathf.Sign(toTarget.x), 0f);

            var count = Mathf.Max(1, definition.projectileCount);
            var spread = definition.projectileSpread;
            var startAngle = -spread * 0.5f;
            var step = count == 1 ? 0f : spread / (count - 1);

            for (int i = 0; i < count; i++)
            {
                var dir = Quaternion.Euler(0, 0, startAngle + step * i) * baseDir;
                var projectile = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);

                // 应用敌人专属子弹颜色和大小
                var sr = projectile.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = definition.projectileColor;
                    projectile.transform.localScale = Vector3.one * definition.projectileScale;
                }

                var speed = definition.projectileSpeed * (CharacterSkill.TimeRiftActive ? 0.4f : 1f);
                var dmg = Mathf.RoundToInt(definition.rangedDamage * DifficultyManager.GetDamageMultiplier());
                projectile.LaunchWithDamage(dir, speed, dmg, 0, Team.Enemy);
            }
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

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
