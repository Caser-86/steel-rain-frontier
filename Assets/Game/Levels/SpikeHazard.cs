using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Levels
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class SpikeHazard : MonoBehaviour
    {
        [SerializeField] private int damage = 2;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Health health))
                return;
            if (health.Team != Team.Player)
                return;

            health.ApplyDamage(new DamageInfo(damage, Team.Enemy, Vector2.up));
        }
    }
}
