using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 狙击手敌人：远距离高伤害，攻击前有激光瞄准预警。
    /// 行为：占据高位→瞄准（激光线可见）→射击→换位→重复。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public sealed class SniperEnemy : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] private float detectRange = 18f;
        [SerializeField] private float fireRange = 15f;
        [SerializeField] private float aimTime = 1.2f;
        [SerializeField] private float cooldown = 3.5f;
        [SerializeField] private int damage = 3;
        [SerializeField] private int scoreValue = 250;

        [Header("Projectile")]
        [SerializeField] private float projectileSpeed = 22f;
        [SerializeField] private Color laserColor = new(1f, 0.2f, 0.2f, 0.5f);

        [Header("Movement")]
        [SerializeField] private float repositionRange = 3f;
        [SerializeField] private float repositionSpeed = 2.5f;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;

        private enum State { Idle, Aiming, Cooldown, Repositioning }
        private State state = State.Idle;
        private Rigidbody2D body;
        private Health health;
        private Transform target;
        private Vector3 anchor;
        private float nextActionTime;
        private bool dead;

        // Laser line
        private LineRenderer laserLine;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
            health = GetComponent<Health>();
            health.Died += OnDied;
            anchor = transform.position;

            SetupLaserLine();
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        private void SetupLaserLine()
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = laserColor;
            laserLine.endColor = laserColor;
            laserLine.positionCount = 2;
            laserLine.enabled = false;
        }

        private void Update()
        {
            if (dead || target == null) return;

            var delta = target.position - transform.position;
            var dist = Mathf.Abs(delta.x);

            // 翻转朝向
            if (spriteRenderer != null)
                spriteRenderer.flipX = delta.x < 0;

            switch (state)
            {
                case State.Idle:
                    if (dist < detectRange && Time.time >= nextActionTime)
                        state = State.Aiming;
                    break;

                case State.Aiming:
                    UpdateAiming();
                    break;

                case State.Cooldown:
                    if (Time.time >= nextActionTime)
                        state = State.Repositioning;
                    break;

                case State.Repositioning:
                    UpdateRepositioning();
                    break;
            }
        }

        private void UpdateAiming()
        {
            // 显示激光瞄准线
            laserLine.enabled = true;
            var from = firePoint != null ? firePoint.position : transform.position;
            laserLine.SetPosition(0, from);
            laserLine.SetPosition(1, target.position);

            // 缓慢脉冲效果
            var pulse = Mathf.PingPong(Time.time * 4f, 1f);
            laserLine.startColor = new Color(1f, 0.2f, 0.2f, 0.3f + pulse * 0.5f);

            if (Time.time >= nextActionTime)
            {
                FireSniperShot();
                laserLine.enabled = false;
                nextActionTime = Time.time + cooldown;
                state = State.Cooldown;
            }
        }

        private void FireSniperShot()
        {
            var from = firePoint != null ? firePoint.position : transform.position;
            var dir = ((Vector2)(target.position - from)).normalized;

            CombatVFX.SpawnHitFlash(from, 0.3f);
            AudioManager.Play("sfx_hit", 0.8f);

            // 直接对目标造成伤害（命中率高）
            var targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
                targetHealth.ApplyDamage(new DamageInfo(damage, Team.Enemy, dir));

            CombatVFX.SpawnHitFlash(target.position, 0.2f);
        }

        private void UpdateRepositioning()
        {
            // 移动到新的狙击位置
            var newTarget = anchor.x + Random.Range(-repositionRange, repositionRange);
            var dir = Mathf.Sign(newTarget - transform.position.x);
            body.linearVelocity = new Vector2(dir * repositionSpeed, body.linearVelocity.y);

            if (Mathf.Abs(transform.position.x - newTarget) < 0.3f)
            {
                body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                nextActionTime = Time.time + 0.5f;
                state = State.Idle;
            }
        }

        private void OnDied()
        {
            dead = true;
            if (laserLine != null) laserLine.enabled = false;
            ScoreManager.AddKill(scoreValue);
            UI.AchievementTracker.OnEnemyKilled(scoreValue);

            var loot = GetComponent<Pickups.LootDrop>();
            if (loot != null) loot.SpawnLoot(transform.position);

            ExplosionEffect.Spawn(transform.position, 0.5f);
            AudioManager.Play("sfx_explosion", 0.5f);
            Destroy(gameObject, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, fireRange);
        }
    }
}
