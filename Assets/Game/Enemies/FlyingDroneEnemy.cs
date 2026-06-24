using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 飞行无人机敌人：悬浮在空中，周期性俯冲攻击后返回原位。
    /// 行为模式：巡逻→发现玩家→俯冲射击→拉升→重复。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public sealed class FlyingDroneEnemy : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float hoverHeight = 4f;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float diveSpeed = 8f;
        [SerializeField] private float riseSpeed = 4f;
        [SerializeField] private float patrolRange = 6f;

        [Header("Combat")]
        [SerializeField] private float detectRange = 12f;
        [SerializeField] private float diveStopY = 0.5f;
        [SerializeField] private float diveCooldown = 3f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private int scoreValue = 120;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;

        private enum DroneState { Patrol, Dive, Rise, Return }
        private DroneState state = DroneState.Patrol;
        private Rigidbody2D body;
        private Health health;
        private Transform target;
        private Vector3 origin;
        private float patrolTarget;
        private float nextDiveTime;
        private bool dead;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0;
            health = GetComponent<Health>();

            origin = transform.position;
            patrolTarget = origin.x + patrolRange;
            health.Died += OnDied;
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        private void FixedUpdate()
        {
            if (dead || target == null) return;

            var delta = target.position - transform.position;
            var distX = Mathf.Abs(delta.x);

            switch (state)
            {
                case DroneState.Patrol:
                    UpdatePatrol(delta, distX);
                    break;
                case DroneState.Dive:
                    UpdateDive();
                    break;
                case DroneState.Rise:
                    UpdateRise();
                    break;
                case DroneState.Return:
                    UpdateReturn();
                    break;
            }

            // 翻转朝向
            if (spriteRenderer != null)
                spriteRenderer.flipX = body.linearVelocity.x < 0;
        }

        private void UpdatePatrol(Vector3 delta, float distX)
        {
            // 巡逻移动
            var dir = Mathf.Sign(patrolTarget - transform.position.x);
            body.linearVelocity = new Vector2(dir * patrolSpeed, 0);

            if (Mathf.Abs(transform.position.x - patrolTarget) < 0.5f)
                patrolTarget = Mathf.Abs(patrolTarget - origin.x) < 0.1f
                    ? origin.x + patrolRange
                    : origin.x;

            // 发现玩家且冷却结束→俯冲
            if (distX < detectRange && Time.time >= nextDiveTime)
            {
                state = DroneState.Dive;
                CombatVFX.SpawnWarningZone(transform.position, 0.3f, 0.2f);
            }
        }

        private void UpdateDive()
        {
            // 快速下降到玩家位置
            body.linearVelocity = new Vector2(
                Mathf.Sign(target.position.x - transform.position.x) * diveSpeed * 0.3f,
                -diveSpeed);

            if (transform.position.y <= diveStopY)
            {
                body.linearVelocity = Vector2.zero;
                nextDiveTime = Time.time + diveCooldown;
                state = DroneState.Rise;
            }
        }

        private void UpdateRise()
        {
            body.linearVelocity = new Vector2(0, riseSpeed);

            if (transform.position.y >= hoverHeight)
            {
                body.linearVelocity = Vector2.zero;
                state = DroneState.Return;
            }
        }

        private void UpdateReturn()
        {
            var dir = Mathf.Sign(origin.x - transform.position.x);
            body.linearVelocity = new Vector2(dir * patrolSpeed, 0);

            if (Mathf.Abs(transform.position.x - origin.x) < 0.5f)
            {
                body.linearVelocity = Vector2.zero;
                state = DroneState.Patrol;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (dead) return;
            if (!other.CompareTag("Player")) return;

            var otherHealth = other.GetComponent<Health>();
            if (otherHealth != null)
            {
                var dir = (other.transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = Vector2.up;
                otherHealth.ApplyDamage(new DamageInfo(contactDamage, Team.Enemy, dir));
            }
        }

        private void OnDied()
        {
            dead = true;
            body.gravityScale = 3f;
            ScoreManager.AddKill(scoreValue);
            UI.AchievementTracker.OnEnemyKilled(scoreValue);

            var loot = GetComponent<Pickups.LootDrop>();
            if (loot != null) loot.SpawnLoot(transform.position);

            ExplosionEffect.Spawn(transform.position, 0.4f);
            AudioManager.Play("sfx_explosion", 0.4f);
            Destroy(gameObject, 1.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            var pos = Application.isPlaying ? origin : transform.position;
            Gizmos.DrawLine(new Vector3(pos.x - patrolRange, pos.y, 0), new Vector3(pos.x + patrolRange, pos.y, 0));
            Gizmos.DrawWireSphere(pos, detectRange);
        }
    }
}
