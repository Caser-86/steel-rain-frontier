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
        private ShieldGuard shieldGuard;
        private GameObject warningObject;
        private float nextAttackTime;
        private float warningUntil;
        private bool pendingAttack;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            shieldGuard = GetComponent<ShieldGuard>();
            health.Initialize(definition.maxHealth, Team.Enemy);
            health.Died += () => Destroy(gameObject);
            warningObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warningObject.name = "AttackWarning";
            warningObject.transform.SetParent(transform, false);
            warningObject.SetActive(false);
            var warningCollider = warningObject.GetComponent<Collider>();
            if (warningCollider != null)
                Destroy(warningCollider);
            var warningRenderer = warningObject.GetComponent<Renderer>();
            warningRenderer.material = new Material(Shader.Find("Sprites/Default"));
            warningRenderer.material.color = new Color(1f, 0.08f, 0.02f, 0.75f);
        }

        private void OnDestroy()
        {
            if (warningObject != null)
                Destroy(warningObject);
        }

        private void FixedUpdate()
        {
            if (target == null)
                return;

            var delta = target.position - transform.position;
            var distance = Mathf.Abs(delta.x);
            var direction = Mathf.Sign(delta.x);
            if (shieldGuard != null)
                shieldGuard.SetFacing(direction);

            if (distance > definition.detectRange)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            var decision = EnemyTactics.GetMoveDecision(distance, definition.attackRange, definition.retreatRange, definition.canRetreat);
            if (decision == EnemyMoveDecision.Advance)
            {
                body.linearVelocity = new Vector2(direction * definition.moveSpeed, body.linearVelocity.y);
                return;
            }

            if (decision == EnemyMoveDecision.Retreat)
            {
                body.linearVelocity = new Vector2(-direction * definition.moveSpeed * 0.75f, body.linearVelocity.y);
                return;
            }

            body.linearVelocity = definition.attackPattern == EnemyAttackPattern.DroneDive
                ? new Vector2(0f, Mathf.Sin(Time.time * 3f) * 0.45f)
                : new Vector2(0f, body.linearVelocity.y);
            TryAttack();
        }

        private void Update()
        {
            if (!pendingAttack || Time.time < warningUntil)
                return;

            pendingAttack = false;
            HideWarning();
            FirePattern();
        }

        private void TryAttack()
        {
            if (pendingAttack || Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + definition.attackCooldown;
            pendingAttack = true;
            warningUntil = Time.time + EnemyTactics.GetWarningDuration(definition.attackPattern);
            ShowWarning();
        }

        private void FirePattern()
        {
            if (target == null)
                return;

            switch (definition.attackPattern)
            {
                case EnemyAttackPattern.GrenadeArc:
                    Fire(Vector2.Lerp(Vector2.up, DirectionToTarget(), 0.6f).normalized, 0.75f);
                    break;
                case EnemyAttackPattern.MortarMarker:
                    SpawnStrikeMarker(1.35f, 0.12f);
                    break;
                case EnemyAttackPattern.DroneDive:
                    Fire(DirectionToTarget(), 1.2f);
                    break;
                case EnemyAttackPattern.SniperShot:
                    Fire(DirectionToTarget(), 1.35f);
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
            if (definition.projectilePrefab == null)
                return;

            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var projectile = Instantiate(definition.projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, definition.projectileDamage, definition.projectileSpeed * speedMultiplier);
        }

        private void SpawnStrikeMarker(float radius, float delay)
        {
            var landing = EnemyStrikeMath.ClampLandingPoint(transform.position, target.position, definition.attackRange);
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "EnemyStrikeMarker";
            go.transform.position = new Vector3(landing.x, 0.08f, -0.35f);
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            var marker = go.AddComponent<EnemyStrikeMarker>();
            marker.Configure(radius, delay, definition.projectileDamage);
        }

        private void ShowWarning()
        {
            if (warningObject == null)
                return;

            warningObject.SetActive(true);
            if (definition.attackPattern == EnemyAttackPattern.MortarMarker)
            {
                warningObject.transform.SetParent(null);
                warningObject.transform.position = target != null ? new Vector3(target.position.x, 0.06f, -0.35f) : transform.position;
                warningObject.transform.localScale = new Vector3(1.6f, 0.08f, 1f);
                return;
            }

            if (definition.attackPattern == EnemyAttackPattern.GrenadeArc)
            {
                warningObject.transform.SetParent(null);
                var landing = target != null
                    ? EnemyStrikeMath.ClampLandingPoint(transform.position, target.position, definition.attackRange)
                    : (Vector2)transform.position;
                warningObject.transform.position = new Vector3(landing.x, 0.06f, -0.35f);
                warningObject.transform.localRotation = Quaternion.identity;
                warningObject.transform.localScale = new Vector3(1.2f, 0.08f, 1f);
                return;
            }

            warningObject.transform.SetParent(transform, false);
            var direction = DirectionToTarget();
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            warningObject.transform.localPosition = direction * 1.4f;
            warningObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            warningObject.transform.localScale = definition.attackPattern == EnemyAttackPattern.SniperShot
                ? new Vector3(7.5f, 0.035f, 1f)
                : new Vector3(1.3f, 0.08f, 1f);
        }

        private void HideWarning()
        {
            if (warningObject == null)
                return;

            warningObject.transform.SetParent(transform, false);
            warningObject.SetActive(false);
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
