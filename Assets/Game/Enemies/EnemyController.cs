using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private Transform attackOrigin;

        private Health health;
        private Rigidbody2D body;
        private float nextAttackTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            health.Initialize(definition.maxHealth, Team.Enemy);
            health.Died += () => Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (target == null)
                return;

            var delta = target.position - transform.position;
            var distance = Mathf.Abs(delta.x);

            if (distance > definition.detectRange)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if (distance > definition.attackRange)
            {
                var direction = Mathf.Sign(delta.x);
                body.linearVelocity = new Vector2(direction * definition.moveSpeed, body.linearVelocity.y);
                return;
            }

            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            TryAttack();
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + definition.attackCooldown;
            FirePattern();
        }

        private void FirePattern()
        {
            if (definition.projectilePrefab == null || target == null)
                return;

            switch (definition.attackPattern)
            {
                case EnemyAttackPattern.GrenadeArc:
                    Fire(Vector2.Lerp(Vector2.up, DirectionToTarget(), 0.6f).normalized, 0.75f);
                    break;
                case EnemyAttackPattern.MortarMarker:
                    Fire(Vector2.up, 0.55f);
                    break;
                case EnemyAttackPattern.DroneDive:
                    Fire(DirectionToTarget(), 1.2f);
                    break;
                case EnemyAttackPattern.FlamethrowerCone:
                    FireSpread(5, 30f, 0.75f);
                    break;
                case EnemyAttackPattern.RifleBurst:
                    FireSpread(3, 8f, 1f);
                    break;
                case EnemyAttackPattern.ShieldAdvance:
                    break;
            }
        }

        private void FireSpread(int count, float angle, float speedMultiplier)
        {
            var startAngle = -angle * 0.5f;
            var step = count == 1 ? 0f : angle / (count - 1);
            var baseDirection = DirectionToTarget();

            for (var i = 0; i < count; i++)
            {
                var direction = Quaternion.Euler(0f, 0f, startAngle + step * i) * baseDirection;
                Fire(direction, speedMultiplier);
            }
        }

        private void Fire(Vector2 direction, float speedMultiplier)
        {
            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var projectile = Instantiate(definition.projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, definition.projectileDamage, definition.projectileSpeed * speedMultiplier);
        }

        private Vector2 DirectionToTarget()
        {
            var targetPoint = (Vector2)target.position + Vector2.up * definition.aimHeightOffset;
            return (targetPoint - (Vector2)transform.position).normalized;
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
