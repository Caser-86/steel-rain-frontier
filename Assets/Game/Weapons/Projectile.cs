using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D body;
        private Team sourceTeam;
        private int damage;
        private int pierceRemaining;
        private float despawnAt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, WeaponFormDefinition form, Team team)
        {
            sourceTeam = team;
            damage = form.damage;
            pierceRemaining = form.pierceCount;
            despawnAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * form.projectileSpeed;
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

            if (health.Team == sourceTeam)
                return;

            health.ApplyDamage(new DamageInfo(damage, sourceTeam, body.linearVelocity.normalized));

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                return;
            }

            Destroy(gameObject);
        }
    }
}
