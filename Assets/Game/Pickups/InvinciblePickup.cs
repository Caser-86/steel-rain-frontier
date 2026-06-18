using SteelRain.Audio;
using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Pickups
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class InvinciblePickup : MonoBehaviour
    {
        [SerializeField] private float duration = 3f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;
            if (health.Team != Team.Player)
                return;

            StartCoroutine(InvincibilityRoutine(health));
            AudioManager.Play("sfx_upgrade", 0.7f);
            gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator InvincibilityRoutine(Health health)
        {
            health.Initialize(health.Max, Team.Neutral);
            yield return new WaitForSeconds(duration);
            if (health != null && !health.IsDead)
                health.InitializeWithCurrent(health.Max, Team.Player, health.Current);
        }
    }
}
