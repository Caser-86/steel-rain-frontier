using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class MiniBossWalker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private float gunCooldown = 1.2f;
        [SerializeField] private float jumpCooldown = 4f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int contactDamage = 2;
        [SerializeField] private int projectileDamage = 1;

        private Health health;
        private float nextGunTime;
        private float nextJumpTime;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.Initialize(35, Team.Enemy);
            health.Died += () => Destroy(gameObject);
        }

        private void Update()
        {
            if (target == null)
                return;

            if (Time.time >= nextGunTime)
            {
                nextGunTime = Time.time + gunCooldown;
                FireBurst();
            }

            if (Time.time >= nextJumpTime && Mathf.Abs(target.position.x - transform.position.x) < 7f)
                nextJumpTime = Time.time + jumpCooldown;
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
        }

        private void Fire(Vector2 direction)
        {
            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileDamage, projectileSpeed);
        }
    }
}
