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
                if (definition != null)
                {
                    ScoreManager.AddKill(definition.scoreValue);
                    UI.AchievementTracker.OnEnemyKilled(definition.scoreValue);
                }
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
                // 应用难度速度倍率
                var speed = definition.moveSpeed * DifficultyManager.GetEnemySpeedMultiplier();
                body.linearVelocity = new Vector2(dir * speed, body.linearVelocity.y);
            }
            else
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            // 使用点积判断攻击是否来自正面：盾牌法向量与攻击方向夹角需大于约107度
            // 修复原逻辑仅用 Direction.x > 0 判断导致从正上方射击可绕过盾牌的漏洞
            var shieldNormal = facingRight ? Vector2.right : Vector2.left;
            var dot = Vector2.Dot(info.Direction, shieldNormal);
            // dot < -0.3 表示攻击方向与盾牌法向量相反（从正面攻击）
            // 从正上方/正下方攻击时 dot ≈ 0，不触发挡弹，符合"打头顶绕过盾牌"的设计
            if (dot < -0.3f)
            {
                var blocked = Mathf.RoundToInt(info.Amount * blockReduction);
                // 恢复被挡弹减少的血量，但不超过最大值
                if (blocked > 0 && !health.IsDead)
                {
                    health.Heal(blocked);
                }
                AudioManager.Play("sfx_boss_hit", 0.3f);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (definition == null) return;
            if (!collision.collider.TryGetComponent(out Health other)) return;
            if (other.Team == Team.Enemy) return;
            // 根据碰撞方向计算击退方向
            var dir = (other.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Vector2.right;
            other.ApplyDamage(new DamageInfo(Mathf.RoundToInt(definition.contactDamage * DifficultyManager.GetDamageMultiplier()), Team.Enemy, dir));
        }

        public void Initialize(EnemyDefinition def, Transform player)
        {
            definition = def;
            target = player;
            if (health == null) health = GetComponent<Health>();
            // 应用难度生命倍率
            var scaledMaxHealth = Mathf.Max(1, Mathf.RoundToInt(def.maxHealth * DifficultyManager.GetHealthMultiplier()));
            health.Initialize(scaledMaxHealth, Team.Enemy);
            if (spriteRenderer != null) spriteRenderer.color = def.spriteColor;
        }

        public void AssignTarget(Transform t) => target = t;
    }
}
