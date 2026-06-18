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
            health.Initialize(def.maxHealth, Team.Enemy);
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
                body.linearVelocity = new Vector2(direction * definition.moveSpeed * slowMultiplier, body.linearVelocity.y);
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

            nextAttackTime = Time.time + definition.attackCooldown;

            if (definition.attackPattern == EnemyAttackPattern.RifleBurst ||
                definition.attackPattern == EnemyAttackPattern.DroneDive)
            {
                FireAtTarget();
            }
        }

        private void FireAtTarget()
        {
            if (enemyProjectilePrefab == null || target == null) return;

            var dir = (target.position - firePoint.position).normalized;
            var projectile = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);
            var speed = definition.projectileSpeed * (CharacterSkill.TimeRiftActive ? 0.4f : 1f);
            var dmg = Mathf.RoundToInt(definition.rangedDamage * DifficultyManager.GetDamageMultiplier());
            projectile.LaunchWithDamage(dir, speed, dmg, 0, Team.Enemy);
            AudioManager.Play("sfx_enemy_shoot", 0.5f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null) return;
            if (!collision.collider.TryGetComponent(out Health other)) return;
            if (other.Team == Team.Enemy) return;

            other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(definition.contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, Vector2.right));
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
