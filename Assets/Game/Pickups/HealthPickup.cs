using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Pickups
{
    /// <summary>
    /// 生命药剂拾取物。
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int healAmount = 1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;
            if (health.Team != Team.Player)
                return;

            health.Heal(healAmount);
            AudioManager.Play("sfx_pickup");
            gameObject.SetActive(false);
        }
    }
}
