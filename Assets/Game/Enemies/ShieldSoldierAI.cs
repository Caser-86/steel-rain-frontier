using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 盾兵：正面挡子弹，需要绕后或穿甲。
    /// 受到来自正面的伤害时，如果盾牌朝向攻击方向，伤害减少 80%。
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ShieldSoldierAI : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer shieldRenderer;
        [SerializeField] private float blockReduction = 0.8f;

        private Health health;
        private Rigidbody2D body;
        private bool facingRight = true;

        private void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            health.Initialize(definition != null ? definition.maxHealth : 6, Team.Enemy);
            health.Damaged += OnDamaged;
            health.Died += () =>
            {
                ExplosionEffect.Spawn(transform.position, 0.6f);
                AudioManager.Play("sfx_explosion", 0.5f);
                Destroy(gameObject);
            };
        }

        private void FixedUpdate()
        {
            if (target == null || definition == null) return;

            var dx = target.position.x - transform.position.x;
            var dist = Mathf.Abs(dx);

            facingRight = dx > 0;
            if (spriteRenderer != null) spriteRenderer.flipX = !facingRight;
            if (shieldRenderer != null) shieldRenderer.flipX = !facingRight;

            if (dist > definition.attackRange)
            {
                var dir = Mathf.Sign(dx);
                if (dir == 0) dir = 1f;
                body.linearVelocity = new Vector2(dir * definition.moveSpeed, body.linearVelocity.y);
            }
            else
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            // 如果攻击来自正面，恢复被减去的部分血量（挡弹）
            var attackFromRight = info.Direction.x > 0;
            if (attackFromRight == facingRight)
            {
                var blocked = Mathf.RoundToInt(info.Amount * blockReduction);
                if (blocked > 0 && !health.IsDead)
                    health.Heal(blocked);
                AudioManager.Play("sfx_boss_hit", 0.3f);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null) return;
            if (!collision.collider.TryGetComponent(out Health other)) return;
            if (other.Team == Team.Enemy) return;
            other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(definition.contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, Vector2.right));
        }

        public void Initialize(EnemyDefinition def, Transform player)
        {
            definition = def;
            target = player;
            if (health == null) health = GetComponent<Health>();
            health.Initialize(def.maxHealth, Team.Enemy);
            if (spriteRenderer != null) spriteRenderer.color = def.spriteColor;
        }

        public void AssignTarget(Transform t) => target = t;
    }
}
