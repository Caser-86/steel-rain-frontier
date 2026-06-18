using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;

namespace SteelRain.Pickups
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ShieldPickup : MonoBehaviour
    {
        [SerializeField] private float duration = 5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;
            if (health.Team != Team.Player)
                return;

            CharacterSkill.ShieldActive = true;
            CharacterSkill.ShieldTimer = duration;
            AudioManager.Play("sfx_upgrade", 0.7f);
            gameObject.SetActive(false);
        }
    }
}
