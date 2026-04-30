using SteelRain.Core;
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
            var enraged = IsEnraged();

            if (grounded)
            {
                if (wasAirborne)
                    Stomp();

                wasAirborne = false;
                var direction = Mathf.Sign(delta.x);
                var desiredSpeed = absX > 3.5f ? direction * moveSpeed * (enraged ? 1.35f : 1f) : 0f;
                body.linearVelocity = new Vector2(desiredSpeed, body.linearVelocity.y);

                if (Time.time >= nextJumpTime && absX < 8f)
                    Jump(direction, enraged);
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
                nextGunTime = Time.time + (IsEnraged() ? gunCooldown * 0.65f : gunCooldown);
                FireBurst();
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

        private void FireBurst()
        {
            if (projectilePrefab == null)
                return;

            var baseDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Fire(Quaternion.Euler(0f, 0f, -9f) * baseDirection);
            Fire(baseDirection);
            Fire(Quaternion.Euler(0f, 0f, 9f) * baseDirection);

            if (IsEnraged())
            {
                Fire(Quaternion.Euler(0f, 0f, -18f) * baseDirection);
                Fire(Quaternion.Euler(0f, 0f, 18f) * baseDirection);
            }
        }

        private void Fire(Vector2 direction)
        {
            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileDamage, projectileSpeed);
        }

        private void Jump(float direction, bool enraged)
        {
            nextJumpTime = Time.time + (enraged ? jumpCooldown * 0.7f : jumpCooldown);
            body.linearVelocity = new Vector2(direction * jumpLungeSpeed * (enraged ? 1.25f : 1f), jumpVelocity);
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

        private bool IsEnraged()
        {
            return health != null && health.Current <= health.Max / 2;
        }

        private void Stomp()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, stompRadius);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Health targetHealth))
                    continue;

                targetHealth.ApplyDamage(new DamageInfo(stompDamage, Team.Enemy, Vector2.up));
            }
        }
    }
}
