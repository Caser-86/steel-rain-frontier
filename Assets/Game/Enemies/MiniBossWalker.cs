using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MiniBossWalker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float gunCooldown = 1.2f;
        [SerializeField] private float jumpCooldown = 4f;
        [SerializeField] private float jumpVelocity = 8.5f;
        [SerializeField] private float jumpLungeSpeed = 4.5f;
        [SerializeField] private float stompRadius = 3.2f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int contactDamage = 2;
        [SerializeField] private int projectileDamage = 1;
        [SerializeField] private int stompDamage = 2;

        private Health health;
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private float nextGunTime;
        private float nextJumpTime;
        private float weakPointOpenUntil;
        private bool wasAirborne;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            health.Initialize(35, Team.Enemy);
            health.Died += () => Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (target == null)
                return;

            var delta = target.position - transform.position;
            var absX = Mathf.Abs(delta.x);
            var grounded = IsGrounded();
            var phase = CurrentPhase;

            if (grounded)
            {
                if (wasAirborne)
                    Stomp();

                wasAirborne = false;
                var direction = Mathf.Sign(delta.x);
                var speedMultiplier = phase == BossPhase.Advancing ? 1f : phase == BossPhase.Enraged ? 1.35f : 1.1f;
                var desiredSpeed = absX > 3.5f ? direction * moveSpeed * speedMultiplier : 0f;
                body.linearVelocity = new Vector2(desiredSpeed, body.linearVelocity.y);

                if (Time.time >= nextJumpTime && absX < 8f)
                    Jump(direction, phase);
            }
            else
            {
                wasAirborne = true;
            }
        }

        private void Update()
        {
            if (target == null)
                return;

            if (Time.time >= nextGunTime)
            {
                var phase = CurrentPhase;
                nextGunTime = Time.time + GetGunCooldown(phase);
                FireBurst(phase);
                if (phase == BossPhase.CoreExposed)
                    OpenWeakPointWindow();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponent(out Health other))
                return;

            other.ApplyDamage(new DamageInfo(contactDamage, Team.Enemy, Vector2.right));
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void FireBurst(BossPhase phase)
        {
            if (projectilePrefab == null)
                return;

            var baseDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            var count = BossPhaseTactics.GetBurstProjectileCount(phase);
            var angle = phase == BossPhase.CoreExposed ? 14f : 36f;
            var startAngle = -angle * 0.5f;
            var step = count == 1 ? 0f : angle / (count - 1);
            for (var i = 0; i < count; i++)
            {
                Fire(Quaternion.Euler(0f, 0f, startAngle + step * i) * baseDirection);
            }
        }

        private void Fire(Vector2 direction)
        {
            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileDamage, projectileSpeed);
        }

        private void Jump(float direction, BossPhase phase)
        {
            var jumpMultiplier = phase == BossPhase.Advancing ? 1f : phase == BossPhase.Enraged ? 1.25f : 0.9f;
            nextJumpTime = Time.time + (phase == BossPhase.Enraged ? jumpCooldown * 0.7f : jumpCooldown);
            body.linearVelocity = new Vector2(direction * jumpLungeSpeed * jumpMultiplier, jumpVelocity);
            wasAirborne = true;
        }

        private bool IsGrounded()
        {
            if (bodyCollider == null)
                return false;

            var bounds = bodyCollider.bounds;
            var origin = new Vector2(bounds.center.x, bounds.min.y - 0.03f);
            var hits = Physics2D.RaycastAll(origin, Vector2.down, 0.18f);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider != bodyCollider && !hit.collider.isTrigger)
                    return true;
            }

            return false;
        }

        private BossPhase CurrentPhase => health != null
            ? BossPhaseTactics.GetPhase(health.Current, health.Max)
            : BossPhase.Advancing;

        private float GetGunCooldown(BossPhase phase)
        {
            return phase switch
            {
                BossPhase.Enraged => gunCooldown * 0.65f,
                BossPhase.CoreExposed => gunCooldown * 1.25f,
                _ => gunCooldown
            };
        }

        private void OpenWeakPointWindow()
        {
            weakPointOpenUntil = Time.time + BossPhaseTactics.GetWeakPointWindow(BossPhase.CoreExposed);
            ImpactBurst.Spawn(transform.position + Vector3.up * 0.6f, new Color(1f, 0.85f, 0.05f, 0.9f), 0.4f, 2.8f, 0.22f);
        }

        private void Stomp()
        {
            var phase = CurrentPhase;
            var radius = Time.time < weakPointOpenUntil ? stompRadius * 0.75f : stompRadius;
            CameraShake.ShakeGlobal(0.22f, phase == BossPhase.Advancing ? 0.2f : 0.28f);
            ImpactBurst.Spawn(transform.position, new Color(1f, phase == BossPhase.Advancing ? 0.65f : 0.15f, 0.05f, 0.65f), 1.2f, radius * 2f, 0.24f);
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Health targetHealth))
                    continue;

                targetHealth.ApplyDamage(new DamageInfo(stompDamage, Team.Enemy, Vector2.up));
            }
        }
    }
}
