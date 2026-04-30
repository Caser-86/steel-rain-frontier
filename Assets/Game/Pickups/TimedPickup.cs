using UnityEngine;

namespace SteelRain.Pickups
{
    public sealed class TimedPickup : MonoBehaviour
    {
        [SerializeField] private PickupKind kind;

        private float spawnTime;

        private void OnEnable()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (ShouldExpire(kind, Time.time - spawnTime))
                gameObject.SetActive(false);
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
    }
}
