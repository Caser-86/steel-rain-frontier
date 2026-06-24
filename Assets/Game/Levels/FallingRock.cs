using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    /// <summary>
    /// 落石陷阱：玩家靠近时从头顶掉落，砸中造成伤害。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class FallingRock : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int damage = 2;
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private float fallSpeed = 12f;
        [SerializeField] private float resetDelay = 4f;
        [SerializeField] private float warningDuration = 0.4f;

        private Rigidbody2D body;
        private Vector3 origin;
        private bool falling;
        private bool warning;
        private float warningTimer;
        private float resetTimer;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.isKinematic = true;
            origin = transform.position;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (falling)
            {
                // 下落中
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;

                // 落地检测
                if (transform.position.y <= origin.y - 10f)
                    ResetRock();
                return;
            }

            if (warning)
            {
                warningTimer -= Time.deltaTime;
                // 震动警告
                transform.position = origin + new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    Random.Range(-0.05f, 0.05f),
                    0);
                if (warningTimer <= 0f)
                {
                    warning = false;
                    falling = true;
                    body.isKinematic = false;
                }
                return;
            }

            // 等待重置
            if (resetTimer > 0f)
            {
                resetTimer -= Time.deltaTime;
                return;
            }

            // 检测玩家是否在下方
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var dist = Mathf.Abs(player.transform.position.x - origin.x);
            if (dist < triggerDistance && player.transform.position.y < origin.y)
            {
                warning = true;
                warningTimer = warningDuration;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!falling) return;
            if (!collision.collider.CompareTag("Player")) return;

            var health = collision.collider.GetComponent<Health>();
            if (health != null)
                health.ApplyDamage(new DamageInfo(damage, Team.Enemy, transform.position));

            ResetRock();
        }

        private void ResetRock()
        {
            falling = false;
            body.isKinematic = true;
            body.linearVelocity = Vector2.zero;
            transform.position = origin;
            resetTimer = resetDelay;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.6f, 0.2f, 0.3f);
            var pos = Application.isPlaying ? origin : transform.position;
            Gizmos.DrawLine(pos, pos + Vector3.down * 10f);
            Gizmos.DrawWireSphere(pos, triggerDistance);
        }
    }
}
