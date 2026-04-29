using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class MiniBossWalker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float gunCooldown = 1.2f;
        [SerializeField] private float jumpCooldown = 4f;
        [SerializeField] private int contactDamage = 2;

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
                nextGunTime = Time.time + gunCooldown;

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
    }
}
