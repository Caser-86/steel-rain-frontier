using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    /// <summary>
    /// 激光束陷阱：周期性发射水平激光，命中造成伤害。
    /// 可设定开关周期、预警时间、激光宽度。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LaserBeam : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float activeDuration = 2f;
        [SerializeField] private float inactiveDuration = 3f;
        [SerializeField] private float warningDuration = 0.5f;

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float laserLength = 10f;
        [SerializeField] private float laserWidth = 0.3f;

        [Header("Visual")]
        [SerializeField] private Color warningColor = new(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private Color activeColor = new(1f, 0.1f, 0.1f, 0.8f);

        private BoxCollider2D hitbox;
        private SpriteRenderer laserRenderer;
        private float stateTimer;
        private float damageTimer;
        private enum LaserState { Inactive, Warning, Active }
        private LaserState state = LaserState.Inactive;

        private void Awake()
        {
            hitbox = GetComponent<BoxCollider2D>();
            hitbox.isTrigger = true;

            // 创建激光视觉
            var visual = new GameObject("LaserVisual");
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;

            laserRenderer = visual.AddComponent<SpriteRenderer>();
            laserRenderer.color = warningColor;
            laserRenderer.sortingOrder = 5;

            UpdateLaserVisual(false);
            stateTimer = inactiveDuration;
        }

        private void Update()
        {
            stateTimer -= Time.deltaTime;

            switch (state)
            {
                case LaserState.Inactive:
                    UpdateLaserVisual(false);
                    if (stateTimer <= 0f)
                    {
                        state = LaserState.Warning;
                        stateTimer = warningDuration;
                        UpdateLaserVisual(true);
                        laserRenderer.color = warningColor;
                    }
                    break;

                case LaserState.Warning:
                    // 闪烁预警
                    var blink = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f;
                    laserRenderer.color = blink ? warningColor : Color.clear;
                    if (stateTimer <= 0f)
                    {
                        state = LaserState.Active;
                        stateTimer = activeDuration;
                        laserRenderer.color = activeColor;
                        hitbox.enabled = true;
                        damageTimer = 0f;
                    }
                    break;

                case LaserState.Active:
                    UpdateLaserVisual(true);
                    if (stateTimer <= 0f)
                    {
                        state = LaserState.Inactive;
                        stateTimer = inactiveDuration;
                        hitbox.enabled = false;
                    }
                    break;
            }
        }

        private void UpdateLaserVisual(bool visible)
        {
            if (laserRenderer == null) return;

            if (!visible)
            {
                laserRenderer.size = Vector2.zero;
                return;
            }

            // 水平激光
            var localDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
            laserRenderer.size = new Vector2(laserLength, laserWidth);
            hitbox.size = new Vector2(laserLength, laserWidth);
            hitbox.offset = localDir * laserLength * 0.5f;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (state != LaserState.Active) return;
            if (damageTimer > Time.time) return;

            if (!other.CompareTag("Player")) return;
            var health = other.GetComponent<Health>();
            if (health != null)
                health.ApplyDamage(new DamageInfo(damage, Team.Enemy, transform.position));

            damageTimer = Time.time + 0.3f;
        }
    }
}
