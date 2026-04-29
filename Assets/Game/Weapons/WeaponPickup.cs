using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Weapons
{
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weapon;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerCombat combat))
                return;

            combat.EquipWeapon(weapon);
            gameObject.SetActive(false);
        }
    }
}
