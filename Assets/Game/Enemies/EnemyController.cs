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
        }

        public void AssignTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
