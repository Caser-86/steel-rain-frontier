using SteelRain.Audio;
using SteelRain.Player;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Pickups
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WeaponSwapPickup : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private int ammo = -1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;
            if (weapon == null) return;

            combat.SwapWeapon(weapon, ammo);
            AudioManager.Play("sfx_pickup", 0.8f);
            gameObject.SetActive(false);
        }
    }
}
