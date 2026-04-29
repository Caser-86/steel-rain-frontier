using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;

        private Rigidbody2D body;
        private int damage;
        private float despawnAt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, int damageAmount, float speed)
        {
            damage = damageAmount;
            despawnAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            if (Time.time >= despawnAt)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;

            if (health.Team == Team.Enemy)
                return;

            health.ApplyDamage(new DamageInfo(damage, Team.Enemy, body.linearVelocity.normalized));
            Destroy(gameObject);
        }
    }
}
