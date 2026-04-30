using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Pickups
{
    public sealed class WeaponUpgradePickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;

            combat.UpgradeCurrentWeapon();
            gameObject.SetActive(false);
        }
    }
}
