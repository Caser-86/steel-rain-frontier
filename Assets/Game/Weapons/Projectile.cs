using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.VFX;
using UnityEngine;

namespace SteelRain.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool createExplosionOnHit = false;

        private Rigidbody2D body;
        private Team sourceTeam;
        private int damage;
        private int pierceRemaining;
        private float despawnAt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Launch(Vector2 direction, WeaponFormDefinition form, Team team)
        {
            EnsureBody();
            sourceTeam = team;
            damage = form.damage;
            pierceRemaining = form.pierceCount;
            despawnAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * form.projectileSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void LaunchWithDamage(Vector2 direction, float speed, int dmg, int pierce, Team team)
        {
            EnsureBody();
            sourceTeam = team;
            damage = dmg;
            pierceRemaining = pierce;
            despawnAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void EnsureBody()
        {
            if (body == null)
                body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Time.time >= despawnAt)
            {
                // 优先归还对象池，无池则销毁
                var pooled = GetComponent<PooledObject>();
                if (pooled != null) pooled.Despawn();
                else Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;

            if (health.Team == sourceTeam)
                return;

            // 计算击退方向，如果速度为零则使用子弹到目标的方向
            var knockbackDir = body.linearVelocity.normalized;
            if (knockbackDir == Vector2.zero)
                knockbackDir = (other.transform.position - transform.position).normalized;
            if (knockbackDir == Vector2.zero)
                knockbackDir = Vector2.right;

            health.ApplyDamage(new DamageInfo(damage, sourceTeam, knockbackDir));

            if (createExplosionOnHit)
            {
                ExplosionEffect.Spawn(transform.position, 0.8f);
                AudioManager.Play("sfx_explosion", 0.6f);
            }

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                return;
            }

            // 优先归还对象池，无池则销毁
            var pooled = GetComponent<PooledObject>();
            if (pooled != null) pooled.Despawn();
            else Destroy(gameObject);
        }
    }
}
