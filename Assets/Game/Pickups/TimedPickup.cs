using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 定时消失的拾取物。
    /// </summary>
    public sealed class TimedPickup : MonoBehaviour
    {
        [SerializeField] private PickupKind kind = PickupKind.SmallHealth;

        private float spawnTime;
        private Vector3 originalPosition;
        private Vector3 originalLocalPosition;
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            spawnTime = Time.time;
            originalPosition = transform.position;
            originalLocalPosition = transform.localPosition;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (ShouldExpire(kind, Time.time - spawnTime))
            {
                gameObject.SetActive(false);
                return;
            }

            // 浮动动画（使用localPosition避免覆盖父对象移动）
            var bobY = Mathf.Sin(Time.time * 3f) * 0.1f;
            if (transform.parent != null)
            {
                // 保留原始localPosition的x和z，只修改y
                transform.localPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + bobY, originalLocalPosition.z);
            }
            else
            {
                transform.position = originalPosition + Vector3.up * bobY;
            }

            // 最后 5 秒闪烁
            var age = Time.time - spawnTime;
            var expiry = GetExpiry(kind);
            if (expiry > 0f && age > expiry - 5f && spriteRenderer != null)
            {
                var blink = Mathf.Sin(Time.time * 15f) > 0f;
                spriteRenderer.enabled = blink;
            }
        }

        public static bool ShouldExpire(PickupKind kind, float age)
        {
            return kind switch
            {
                PickupKind.SmallHealth => age >= 18f,
                PickupKind.LargeHealth => age >= 25f,
                PickupKind.Shield => age >= 30f,
                PickupKind.Invincible => age >= 30f,
                _ => age >= 30f
            };
        }

        public static float GetExpiry(PickupKind kind)
        {
            return kind switch
            {
                PickupKind.SmallHealth => 18f,
                PickupKind.LargeHealth => 25f,
                PickupKind.Shield => 30f,
                PickupKind.Invincible => 30f,
                _ => 30f
            };
        }
    }
}
