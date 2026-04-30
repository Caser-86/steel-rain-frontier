using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Pickups
{
    public sealed class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health) || health.Team != Team.Player)
                return;

            health.Heal(amount);
            gameObject.SetActive(false);
        }
    }
}
