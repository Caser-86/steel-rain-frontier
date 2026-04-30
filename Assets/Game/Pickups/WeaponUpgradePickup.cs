using SteelRain.Player;
using UnityEngine;
using SteelRain.Core;

namespace SteelRain.Pickups
{
    public sealed class WeaponUpgradePickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;

            if (!combat.UpgradeCurrentWeapon() && other.TryGetComponent(out Health health))
                health.SetInvulnerable(3f);

            gameObject.SetActive(false);
        }
    }
}
