using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.UI;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class TurretBoss : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Projectile projectilePrefab;

        [Header("Phase 1: Single Shot")]
        [SerializeField] private float singleShotCooldown = 1.2f;
        [SerializeField] private int singleDamage = 2;
        [SerializeField] private float projectileSpeed = 10f;

        [Header("Phase 2: Spread Shot")]
        [SerializeField] private float spreadCooldown = 2f;
        [SerializeField] private int spreadCount = 5;
        [SerializeField] private float spreadAngle = 60f;

        [Header("Phase 3: Homing")]
        [SerializeField] private float homingCooldown = 3f;
        [SerializeField] private int homingCount = 3;

        [Header("Contact")]
        [SerializeField] private int contactDamage = 1;

        [Header("Score")]
        [SerializeField] private int scoreValue = 1000;

        private Health health;
        private Rigidbody2D body;
        private int currentPhase = 1;
        private float nextSingleTime;
        private float nextSpreadTime;
        private float nextHomingTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            if (firePoints == null || firePoints.Length == 0)
                firePoints = new[] { transform };
            // 应用难度生命倍率
            var scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(50 * DifficultyManager.GetHealthMultiplier()));
            health.Initialize(scaledMaxHealth, Team.Enemy);
            health.Died += OnDeath;
            health.Damaged += OnDamaged;
        }

        private void Update()
        {
            if (target == null) return;
            UpdatePhase();

            if (currentPhase >= 1 && Time.time >= nextSingleTime)
            {
                nextSingleTime = Time.time + singleShotCooldown;
                StartCoroutine(SingleShot());
            }

            if (currentPhase >= 2 && Time.time >= nextSpreadTime)
            {
                nextSpreadTime = Time.time + spreadCooldown;
                StartCoroutine(SpreadShot());
            }

            if (currentPhase >= 3 && Time.time >= nextHomingTime)
            {
                nextHomingTime = Time.time + homingCooldown;
                StartCoroutine(HomingShot());
            }
        }

        private void UpdatePhase()
        {
            var hp = (float)health.Current / health.Max;
            var newPhase = hp <= 0.3f ? 3 : hp <= 0.6f ? 2 : 1;
            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                AudioManager.Play("sfx_explosion", 0.5f);
            }
        }

        private IEnumerator SingleShot()
        {
            if (projectilePrefab == null || target == null) yield break;
            var fp = firePoints[0];
            var dir = (target.position - fp.position).normalized;
            var proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
            var dmg = Mathf.RoundToInt(singleDamage * DifficultyManager.GetDamageMultiplier());
            proj.LaunchWithDamage(dir, projectileSpeed, dmg, 0, Team.Enemy);
            AudioManager.Play("sfx_enemy_shoot", 0.5f);
        }

        private IEnumerator SpreadShot()
        {
            if (projectilePrefab == null || target == null) yield break;
            var fp = firePoints[0];
            var baseDir = (target.position - fp.position).normalized;
            var startAngle = -spreadAngle * 0.5f;
            var step = spreadCount == 1 ? 0f : spreadAngle / (spreadCount - 1);
            var dmg = Mathf.RoundToInt(singleDamage * DifficultyManager.GetDamageMultiplier());

            for (int i = 0; i < spreadCount; i++)
            {
                var dir = Quaternion.Euler(0, 0, startAngle + step * i) * baseDir;
                var proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
                proj.LaunchWithDamage(dir, projectileSpeed * 0.8f, dmg, 0, Team.Enemy);
                AudioManager.Play("sfx_enemy_shoot", 0.3f);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator HomingShot()
        {
            if (projectilePrefab == null || target == null) yield break;
            var baseDmg = Mathf.RoundToInt((singleDamage + 1) * DifficultyManager.GetDamageMultiplier());
            for (int i = 0; i < homingCount; i++)
            {
                var fp = firePoints[i % firePoints.Length];
                var dir = (target.position - fp.position).normalized;
                var proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
                proj.LaunchWithDamage(dir, projectileSpeed * 1.2f, baseDmg, 0, Team.Enemy);
                AudioManager.Play("sfx_enemy_shoot", 0.4f);
                yield return new WaitForSeconds(0.3f);
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            AudioManager.Play("sfx_boss_hit", 0.5f);
        }

        private void OnDeath()
        {
            AudioManager.Play("sfx_explosion", 1f);
            ExplosionEffect.Spawn(transform.position, 3f);
            // 补全击杀分数和成就追踪
            ScoreManager.AddKill(scoreValue);
            AchievementTracker.OnEnemyKilled(scoreValue);
            GameEvents.RaiseBossDefeated();
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent(out Health other)) return;
            if (other.Team == Team.Enemy) return;
            // 根据碰撞方向计算击退方向
            var dir = (other.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Vector2.right;
            // 应用难度伤害倍率
            var dmg = Mathf.RoundToInt(contactDamage * DifficultyManager.GetDamageMultiplier());
            other.ApplyDamage(new DamageInfo(dmg, Team.Enemy, dir));
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
            var bossBar = FindFirstObjectByType<BossHealthBar>();
            if (bossBar != null)
                bossBar.TrackBoss(health, "Turret Boss");
        }
    }
}
