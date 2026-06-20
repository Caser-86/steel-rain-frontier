using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Levels;
using SteelRain.Player;
using SteelRain.UI;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 小 Boss 四足侦察机甲，三阶段攻击：
    /// 阶段1：机枪横扫
    /// 阶段2：跳跃砸地+冲击波
    /// 阶段3（血量<35%）：核心暴露，转身变慢
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MiniBossWalker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Projectile enemyProjectilePrefab;

        [Header("Phase 1: Machine Gun Sweep")]
        [SerializeField] private float gunCooldown = 1.8f;
        [SerializeField] private int gunBurstCount = 5;
        [SerializeField] private float gunBurstInterval = 0.12f;
        [SerializeField] private float gunProjectileSpeed = 10f;
        [SerializeField] private int gunDamage = 1;

        [Header("Phase 2: Jump Slam")]
        [SerializeField] private float jumpCooldown = 5f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float slamRadius = 4f;
        [SerializeField] private int slamDamage = 2;

        [Header("Phase 3: Core Exposed")]
        [SerializeField] private float coreExposedAtPercent = 0.35f;
        [SerializeField] private float turnSpeedPhase3 = 0.5f;

        [Header("Contact")]
        [SerializeField] private int contactDamage = 2;

        [Header("Score")]
        [SerializeField] private int scoreValue = 500;

        private Health health;
        private Rigidbody2D body;
        private float nextGunTime;
        private float nextJumpTime;
        private int currentPhase = 1;
        private bool facingRight = true;
        private float turnDelayTimer;

        public int CurrentPhase => currentPhase;
        public bool CoreExposed => currentPhase >= 3;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (firePoint == null) firePoint = transform;
            // 应用难度生命倍率
            var scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(35 * DifficultyManager.GetHealthMultiplier()));
            health.Initialize(scaledMaxHealth, Team.Enemy);
            health.Died += OnDeath;
            health.Damaged += OnDamaged;

            // Boss出现时触发叙事警告过场
            var story = FindFirstObjectByType<StoryManager>();
            if (story != null) story.PlayBossWarning();
        }

        private void Update()
        {
            if (target == null) return;

            UpdatePhase();
            FaceTarget();

            if (currentPhase >= 1 && Time.time >= nextGunTime)
            {
                // Phase 4（Hard专属狂暴）：射速翻倍
                var cd = currentPhase >= 4 ? gunCooldown * 0.5f : gunCooldown;
                nextGunTime = Time.time + cd;
                StartCoroutine(MachineGunSweep());
            }

            if (currentPhase >= 2 && Time.time >= nextJumpTime)
            {
                var dx = Mathf.Abs(target.position.x - transform.position.x);
                // Phase 4：跳跃砸地冷却减半，追踪范围扩大
                var jumpRange = currentPhase >= 4 ? 12f : 8f;
                if (dx < jumpRange)
                {
                    var jc = currentPhase >= 4 ? jumpCooldown * 0.5f : jumpCooldown;
                    nextJumpTime = Time.time + jc;
                    StartCoroutine(JumpSlam());
                }
            }
        }

        private void UpdatePhase()
        {
            var hpPercent = (float)health.Current / health.Max;
            // Phase 4 仅在 Hard 难度触发（血量<15%），提供机制层面的额外挑战
            var newPhase = DifficultyManager.HasBossExtraPhase() && hpPercent <= 0.15f ? 4 :
                           hpPercent <= coreExposedAtPercent ? 3 :
                           hpPercent <= 0.6f ? 2 : 1;
            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                AudioManager.Play("sfx_explosion", 0.5f);
                if (spriteRenderer != null)
                {
                    if (currentPhase == 3)
                        spriteRenderer.color = new Color(1f, 0.5f, 0.3f);
                    else if (currentPhase == 4)
                        spriteRenderer.color = new Color(1f, 0.2f, 0.2f); // 狂暴红色
                }
            }
        }

        private void FaceTarget()
        {
            if (target == null) return;
            var dx = target.position.x - transform.position.x;
            var shouldFaceRight = dx > 0;
            if (shouldFaceRight != facingRight)
            {
                // 阶段3转身慢：使用累积计时器实现稳定的转身延迟
                if (currentPhase == 3)
                {
                    turnDelayTimer += Time.deltaTime;
                    if (turnDelayTimer < turnSpeedPhase3) return;
                }
                turnDelayTimer = 0f;
                facingRight = shouldFaceRight;
                if (spriteRenderer != null)
                    spriteRenderer.flipX = !facingRight;
            }
            else
            {
                turnDelayTimer = 0f;
            }
        }

        private IEnumerator MachineGunSweep()
        {
            if (enemyProjectilePrefab == null || target == null) yield break;

            var baseDir = (target.position - firePoint.position).normalized;
            var slowMultiplier = CharacterSkill.TimeRiftActive ? 0.4f : 1f;
            for (int i = 0; i < gunBurstCount; i++)
            {
                if (target == null) yield break;
                var spread = Random.Range(-0.15f, 0.15f);
                var dir = Quaternion.Euler(0, 0, spread * 30f) * baseDir;
                var proj = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.identity);
                var dmg = Mathf.RoundToInt(gunDamage * DifficultyManager.GetDamageMultiplier());
                proj.LaunchWithDamage(dir, gunProjectileSpeed * slowMultiplier, dmg, 0, Team.Enemy);
                AudioManager.Play("sfx_enemy_shoot", 0.4f);
                yield return new WaitForSeconds(gunBurstInterval);
            }
        }

        private IEnumerator JumpSlam()
        {
            var slowMultiplier = CharacterSkill.TimeRiftActive ? 0.4f : 1f;
            body.linearVelocity = new Vector2(0f, jumpForce * slowMultiplier);
            yield return new WaitForSeconds(0.6f);

            // 等待落地
            while (body.linearVelocity.y > 0.1f)
                yield return null;

            // 砸地
            AudioManager.Play("sfx_explosion", 0.8f);
            ExplosionEffect.Spawn(transform.position, 2f);
            var shake = Camera.main?.GetComponent<CameraShake>();
            if (shake != null) shake.Shake(0.4f, 0.5f);

            // 冲击波伤害
            var hits = Physics2D.OverlapCircleAll(transform.position, slamRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Health hp) && hp.Team == Team.Player)
                {
                    var dir = (hp.transform.position - transform.position).normalized;
                    hp.ApplyDamage(new DamageInfo(Mathf.RoundToInt(slamDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, dir));
                }
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            AudioManager.Play("sfx_boss_hit", 0.6f);
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
            other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, dir));
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
            var bossBar = FindFirstObjectByType<BossHealthBar>();
            if (bossBar != null)
                bossBar.TrackBoss(health, "Mini-Boss Walker");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
    }
}
