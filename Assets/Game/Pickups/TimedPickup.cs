using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 定时消失的拾取物。武器升级胶囊永不过期。
    /// </summary>
    public sealed class TimedPickup : MonoBehaviour
    {
        [SerializeField] private PickupKind kind = PickupKind.SmallHealth;

        private float spawnTime;
        private Vector3 originalPosition;
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            spawnTime = Time.time;
            originalPosition = transform.position;
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

            // 浮动动画
            var bobY = Mathf.Sin(Time.time * 3f) * 0.1f;
            transform.position = originalPosition + Vector3.up * bobY;

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
                PickupKind.WeaponUpgrade => false,
                PickupKind.SmallHealth => age >= 18f,
                PickupKind.LargeHealth => age >= 25f,
                PickupKind.Ammo => age >= 30f,
                PickupKind.Shield => age >= 30f,
                PickupKind.Invincible => age >= 30f,
                _ => age >= 30f
            };
        }

        public static float GetExpiry(PickupKind kind)
        {
            return kind switch
            {
                PickupKind.WeaponUpgrade => -1f,
                PickupKind.SmallHealth => 18f,
                PickupKind.LargeHealth => 25f,
                PickupKind.Ammo => 30f,
                PickupKind.Shield => 30f,
                PickupKind.Invincible => 30f,
                _ => 30f
            };
        }
    }
}
