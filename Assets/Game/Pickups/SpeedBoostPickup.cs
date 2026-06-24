using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Pickups
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class SpeedBoostPickup : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float duration = 8f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var controller = other.GetComponent<PlayerController2D>();
            if (controller == null) return;

            TempBuffState.SpeedBoostActive = true;
            TempBuffState.SpeedBoostTimer = duration;
            TempBuffState.SpeedBoostMultiplier = speedMultiplier;
            AudioManager.Play("sfx_pickup", 0.7f);
            gameObject.SetActive(false);
        }
    }
}
